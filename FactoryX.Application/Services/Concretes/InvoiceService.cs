using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using FactoryX.Infrastructure;

namespace FactoryX.Application.Services.Concretes;

public class InvoiceService : IInvoiceService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IAccountingPostingService _postingService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public InvoiceService(
        IRepositoryManager repositoryManager,
        IAccountingPostingService postingService,
        IMapper mapper,
        AppDbContext context)
    {
        _repositoryManager = repositoryManager;
        _postingService = postingService;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync(
        InvoiceStatus? status = null,
        int? customerId = null,
        int? salesOrderId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var invoices = await _repositoryManager.InvoiceRepository.GetAllInvoicesAsync(
            status, customerId, salesOrderId, fromDate, toDate, searchTerm);

        return _mapper.Map<IEnumerable<InvoiceDto>>(invoices);
    }

    public async Task<InvoiceDto?> GetInvoiceByIdAsync(int id)
    {
        var invoice = await _repositoryManager.InvoiceRepository.GetByIdWithDetailsAsync(id);
        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber)
    {
        var invoice = await _repositoryManager.InvoiceRepository.GetByInvoiceNumberAsync(invoiceNumber);
        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<IEnumerable<InvoiceableOrderDto>> GetInvoiceableOrdersAsync(int? customerId = null)
    {
        var orders = await _context.SalesOrders
            .AsNoTracking()
            .Include(so => so.Customer)
            .Include(so => so.Warehouse)
            .Include(so => so.Items)
                .ThenInclude(i => i.Product)
            .Include(so => so.Fulfillments!)
                .ThenInclude(f => f.Items)
            .Where(so => (so.Status == SalesOrderStatus.Confirmed ||
                          so.Status == SalesOrderStatus.PartiallyFulfilled ||
                          so.Status == SalesOrderStatus.FullyFulfilled) &&
                         (!customerId.HasValue || so.CustomerId == customerId.Value))
            .OrderByDescending(so => so.OrderDate)
            .ToListAsync();

        var result = new List<InvoiceableOrderDto>();

        foreach (var order in orders)
        {
            // Get all existing non-cancelled invoice items for this sales order
            var existingInvoices = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Items)
                .Where(i => i.SalesOrderId == order.Id && i.Status != InvoiceStatus.Cancelled)
                .ToListAsync();

            var alreadyInvoicedBySoItem = existingInvoices
                .SelectMany(i => i.Items)
                .Where(item => item.SalesOrderItemId.HasValue)
                .GroupBy(item => item.SalesOrderItemId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

            var invoiceableItems = new List<InvoiceableItemDto>();

            foreach (var soItem in order.Items)
            {
                var fulfilledQty = soItem.FulfilledQuantity > 0 ? soItem.FulfilledQuantity : (order.Fulfillments?
                    .Where(f => f.Status == SalesFulfillmentStatus.Shipped)
                    .SelectMany(f => f.Items)
                    .Where(fi => fi.SalesOrderItemId == soItem.Id || fi.ProductId == soItem.ProductId)
                    .Sum(fi => fi.ShippedQuantity) ?? 0);

                alreadyInvoicedBySoItem.TryGetValue(soItem.Id, out decimal alreadyInvoiced);

                var invoiceableQty = Math.Max(0, fulfilledQty - alreadyInvoiced);

                if (invoiceableQty > 0 || fulfilledQty > 0)
                {
                    invoiceableItems.Add(new InvoiceableItemDto
                    {
                        SalesOrderItemId = soItem.Id,
                        ProductId = soItem.ProductId,
                        ProductName = soItem.Product?.Name ?? string.Empty,
                        ProductCode = soItem.Product?.Code ?? string.Empty,
                        ProductSKU = soItem.Product?.SKU,
                        OrderedQuantity = soItem.OrderedQuantity,
                        FulfilledQuantity = fulfilledQty,
                        AlreadyInvoicedQuantity = alreadyInvoiced,
                        Unit = soItem.Unit,
                        UnitPrice = soItem.UnitPrice,
                        DiscountAmount = soItem.DiscountAmount,
                        TaxRate = 14.00m
                    });
                }
            }

            if (invoiceableItems.Any(i => i.InvoiceableQuantity > 0))
            {
                result.Add(new InvoiceableOrderDto
                {
                    SalesOrderId = order.Id,
                    SalesOrderNumber = order.OrderNumber,
                    CustomerId = order.CustomerId,
                    CustomerName = order.Customer?.Name ?? string.Empty,
                    CustomerCode = order.Customer?.Code ?? string.Empty,
                    OrderDate = order.OrderDate,
                    WarehouseName = order.Warehouse?.Name ?? string.Empty,
                    Status = order.Status.ToString(),
                    Items = invoiceableItems
                });
            }
        }

        return result;
    }

    public async Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, int userId)
    {
        // 1. Validate Customer
        var customer = await _repositoryManager.CustomerRepository.GetByIdAsync(request.CustomerId);
        if (customer == null)
        {
            throw new InvalidOperationException("العميل المحدد غير موجود في سجل العملاء.");
        }
        if (!customer.IsActive)
        {
            throw new InvalidOperationException($"العميل '{customer.Name}' غير نشط (معطل) ولا يمكن إصدار فواتير له.");
        }

        // 2. Validate Sales Order
        var order = await _context.SalesOrders
            .Include(so => so.Items)
                .ThenInclude(i => i.Product)
            .Include(so => so.Fulfillments!)
                .ThenInclude(f => f.Items)
            .FirstOrDefaultAsync(so => so.Id == request.SalesOrderId);

        if (order == null)
        {
            throw new InvalidOperationException("أمر البيع المحدد غير موجود.");
        }

        if (order.CustomerId != request.CustomerId)
        {
            throw new InvalidOperationException("أمر البيع المحدد لا يخص العميل المختار.");
        }

        if (request.Items == null || !request.Items.Any())
        {
            throw new InvalidOperationException("يجب إضافة بند واحد على الأقل في الفاتورة.");
        }

        // 3. Check Fulfilled vs Already Invoiced Quantities to prevent over-invoicing
        var existingInvoices = await _context.Invoices
            .AsNoTracking()
            .Include(i => i.Items)
            .Where(i => i.SalesOrderId == order.Id && i.Status != InvoiceStatus.Cancelled)
            .ToListAsync();

        var alreadyInvoicedMap = existingInvoices
            .SelectMany(i => i.Items)
            .Where(item => item.SalesOrderItemId.HasValue)
            .GroupBy(item => item.SalesOrderItemId!.Value)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Quantity));

        var user = await _repositoryManager.UserRepository.GetByIdAsync(userId);
        var invoiceNumber = await GenerateNextInvoiceNumberAsync(request.InvoiceDate);

        var invoice = new Invoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId = request.CustomerId,
            SalesOrderId = request.SalesOrderId,
            FulfillmentId = request.FulfillmentId,
            InvoiceDate = request.InvoiceDate.Date,
            DueDate = request.DueDate,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "EGP" : request.Currency.Trim(),
            Status = request.IssueImmediately ? InvoiceStatus.Issued : InvoiceStatus.Draft,
            Notes = request.Notes,
            CreatedByUserId = userId,
            CreatedByName = user?.FullName ?? user?.Username ?? "المسؤول",
            TaxRate = request.TaxRate
        };

        decimal calculatedSubTotal = 0;
        decimal calculatedDiscount = 0;
        decimal calculatedTax = 0;

        foreach (var reqItem in request.Items)
        {
            if (reqItem.Quantity <= 0) continue;

            var soItem = order.Items.FirstOrDefault(i => i.Id == reqItem.SalesOrderItemId || i.ProductId == reqItem.ProductId);
            if (soItem == null)
            {
                throw new InvalidOperationException($"المنتج رقم [{reqItem.ProductId}] غير موجود في أمر البيع المعتمد.");
            }

            // Calculate total fulfilled quantity for this item
            var totalFulfilled = soItem.FulfilledQuantity > 0 ? soItem.FulfilledQuantity : (order.Fulfillments?
                .Where(f => f.Status == SalesFulfillmentStatus.Shipped)
                .SelectMany(f => f.Items)
                .Where(fi => fi.SalesOrderItemId == soItem.Id || fi.ProductId == soItem.ProductId)
                .Sum(fi => fi.ShippedQuantity) ?? 0);

            alreadyInvoicedMap.TryGetValue(soItem.Id, out decimal alreadyInvoiced);
            var maxInvoiceable = Math.Max(0, totalFulfilled - alreadyInvoiced);

            if (reqItem.Quantity > maxInvoiceable)
            {
                throw new InvalidOperationException(
                    $"الكمية المطلوب فوترتها للمنتج '{soItem.Product?.Name}' ({reqItem.Quantity} {reqItem.Unit}) تتجاوز الكمية المسلمة غير المفوترة المتاحة ({maxInvoiceable} {reqItem.Unit}).");
            }

            // Preserve historical unit price from sales order
            var unitPrice = soItem.UnitPrice > 0 ? soItem.UnitPrice : reqItem.UnitPrice;
            var grossLine = reqItem.Quantity * unitPrice;
            var lineDiscount = reqItem.DiscountAmount;
            var taxableLine = Math.Max(0, grossLine - lineDiscount);
            var taxRate = reqItem.TaxRate >= 0 ? reqItem.TaxRate : request.TaxRate;
            var lineTax = Math.Round(taxableLine * (taxRate / 100m), 2);
            var lineTotal = taxableLine + lineTax;

            calculatedSubTotal += grossLine;
            calculatedDiscount += lineDiscount;
            calculatedTax += lineTax;

            var invoiceItem = new InvoiceItem
            {
                ProductId = reqItem.ProductId,
                SalesOrderItemId = soItem.Id,
                SalesFulfillmentItemId = reqItem.SalesFulfillmentItemId,
                Description = reqItem.Description ?? soItem.Product?.Name,
                Quantity = reqItem.Quantity,
                Unit = string.IsNullOrWhiteSpace(reqItem.Unit) ? soItem.Unit : reqItem.Unit,
                UnitPrice = unitPrice,
                DiscountAmount = lineDiscount,
                TaxRate = taxRate,
                TaxAmount = lineTax,
                LineTotal = lineTotal,
                Notes = reqItem.Notes
            };

            invoice.Items.Add(invoiceItem);
        }

        if (!invoice.Items.Any())
        {
            throw new InvalidOperationException("لم يتم تحديد أي كميات صالحة للفوترة.");
        }

        invoice.SubTotal = calculatedSubTotal;
        invoice.DiscountAmount = calculatedDiscount;
        invoice.TaxAmount = calculatedTax;
        invoice.TotalAmount = Math.Max(0, calculatedSubTotal - calculatedDiscount + calculatedTax);
        invoice.PaidAmount = 0;
        invoice.RemainingAmount = invoice.TotalAmount;

        _repositoryManager.InvoiceRepository.Create(invoice);
        await _repositoryManager.SaveAsync();

        if (invoice.Status == InvoiceStatus.Issued)
        {
            await _postingService.PostSalesInvoiceAsync(invoice.Id, userId);
        }

        return await GetInvoiceByIdAsync(invoice.Id) ?? _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<InvoiceDto> IssueInvoiceAsync(int invoiceId, int userId)
    {
        var invoice = await _repositoryManager.InvoiceRepository.GetByIdWithDetailsAsync(invoiceId, trackChanges: true);
        if (invoice == null)
        {
            throw new InvalidOperationException("الفاتورة المطلوبة غير موجودة.");
        }

        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidOperationException("يمكن اعتماد وإصدار الفواتير المسودة فقط.");
        }

        invoice.Status = InvoiceStatus.Issued;
        await _repositoryManager.SaveAsync();

        // Automatic Accounting Posting
        await _postingService.PostSalesInvoiceAsync(invoice.Id, userId);

        return _mapper.Map<InvoiceDto>(invoice);
    }

    public async Task<bool> CancelInvoiceAsync(int invoiceId, string reason, int userId)
    {
        var invoice = await _repositoryManager.InvoiceRepository.GetByIdWithDetailsAsync(invoiceId, trackChanges: true);
        if (invoice == null)
        {
            throw new InvalidOperationException("الفاتورة المطلوبة غير موجودة.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            throw new InvalidOperationException("الفاتورة ملغاة بالفعل.");
        }

        if (invoice.Payments.Any(p => p.Status == PaymentStatus.Recorded))
        {
            throw new InvalidOperationException(
                "لا يمكن إلغاء الفاتورة لوجود مدفوعات مسجلة ومثبتة عليها. يرجى إلغاء/استرداد سندات القبض أولاً.");
        }

        if (invoice.PaidAmount > 0)
        {
            throw new InvalidOperationException("لا يمكن إلغاء فاتورة تم سداد جزء من قيمتها.");
        }

        invoice.Status = InvoiceStatus.Cancelled;
        invoice.CancellationReason = reason;
        invoice.CancelledAt = DateTime.UtcNow;
        invoice.CancelledByUserId = userId;

        await _repositoryManager.SaveAsync();

        // Reverse Accounting Journal if posted
        var existingJournal = await _repositoryManager.JournalEntryRepository
            .GetByReferenceAsync(JournalReferenceType.SalesInvoice, invoice.Id);
        if (existingJournal != null && existingJournal.Status == JournalEntryStatus.Posted)
        {
            await _postingService.ReverseJournalEntryAsync(existingJournal.Id, reason, userId);
        }

        return true;
    }

    public async Task<InvoiceSummaryDto> GetSummaryAsync()
    {
        var invoices = await _context.Invoices.AsNoTracking().ToListAsync();

        return new InvoiceSummaryDto
        {
            TotalInvoices = invoices.Count,
            DraftCount = invoices.Count(i => i.Status == InvoiceStatus.Draft),
            IssuedCount = invoices.Count(i => i.Status == InvoiceStatus.Issued),
            PartiallyPaidCount = invoices.Count(i => i.Status == InvoiceStatus.PartiallyPaid),
            PaidCount = invoices.Count(i => i.Status == InvoiceStatus.Paid),
            CancelledCount = invoices.Count(i => i.Status == InvoiceStatus.Cancelled),
            TotalInvoicedAmount = invoices.Where(i => i.Status != InvoiceStatus.Cancelled).Sum(i => i.TotalAmount),
            TotalPaidAmount = invoices.Where(i => i.Status != InvoiceStatus.Cancelled).Sum(i => i.PaidAmount),
            TotalOutstandingAmount = invoices.Where(i => i.Status != InvoiceStatus.Cancelled).Sum(i => i.RemainingAmount)
        };
    }

    public async Task<string> GenerateNextInvoiceNumberAsync(DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var datePrefix = targetDate.ToString("yyyyMMdd");
        var prefix = $"INV-{datePrefix}-";

        var latestToday = await _context.Invoices
            .AsNoTracking()
            .Where(i => i.InvoiceNumber.StartsWith(prefix))
            .OrderByDescending(i => i.InvoiceNumber)
            .Select(i => i.InvoiceNumber)
            .FirstOrDefaultAsync();

        int nextSeq = 1;
        if (!string.IsNullOrEmpty(latestToday) && latestToday.Length >= prefix.Length + 4)
        {
            var seqPart = latestToday.Substring(prefix.Length);
            if (int.TryParse(seqPart, out int parsed))
            {
                nextSeq = parsed + 1;
            }
        }

        return $"{prefix}{nextSeq:D4}";
    }
}
