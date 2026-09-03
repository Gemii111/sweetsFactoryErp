using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public PurchaseOrderService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<PurchaseOrderDto>> GetAllOrdersAsync(
        PurchaseOrderStatus? status = null,
        int? supplierId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var orders = await _repositoryManager.PurchaseOrderRepository.GetAllOrdersAsync(
            status, supplierId, warehouseId, fromDate, toDate, searchTerm);
        return _mapper.Map<IEnumerable<PurchaseOrderDto>>(orders);
    }

    public async Task<PurchaseOrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _repositoryManager.PurchaseOrderRepository.GetByIdWithDetailsAsync(id);
        return _mapper.Map<PurchaseOrderDto>(order);
    }

    public async Task<PurchaseOrderDto> CreateOrderAsync(CreatePurchaseOrderRequest request, int userId)
    {
        if (request.Items == null || !request.Items.Any())
        {
            throw new InvalidOperationException("يجب إضافة بند واحد على الأقل في أمر الشراء.");
        }

        // Validate Supplier
        var supplier = await _repositoryManager.SupplierRepository.GetByIdWithDetailsAsync(request.SupplierId);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"المورد بالمعرف #{request.SupplierId} غير موجود.");
        }
        if (!supplier.IsActive)
        {
            throw new InvalidOperationException($"المورد [{supplier.Name}] غير نشط (Inactive) ولا يمكن إنشاء أوامر شراء جديدة له.");
        }

        // Validate Warehouse
        var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse == null)
        {
            throw new KeyNotFoundException($"المستودع بالمعرف #{request.WarehouseId} غير موجود.");
        }
        if (warehouse.Type != WarehouseType.RawMaterial && warehouse.Type != WarehouseType.Packaging)
        {
            throw new InvalidOperationException("مستودع استلام خامات ومواد الشراء يجب أن يكون مستودع خامات (Raw Material) أو مستودع تعبئة وتغليف (Packaging). لا يمكن استخدام مستودع منتجات تامة.");
        }

        // Validate Materials & Quantities
        decimal totalBeforeTax = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        var poItems = new List<PurchaseOrderItem>();
        foreach (var item in request.Items)
        {
            if (item.OrderedQuantity <= 0)
            {
                throw new InvalidOperationException("الكمية المطلوبة في كل بند يجب أن تكون أكبر من الصفر.");
            }

            var material = await _repositoryManager.MaterialRepository.GetByIdWithDetailsAsync(item.MaterialId);
            if (material == null)
            {
                throw new InvalidOperationException($"المادة بالمعرف #{item.MaterialId} غير موجودة في قاعدة البيانات.");
            }
            if (!material.IsActive)
            {
                throw new InvalidOperationException($"المادة [{material.Name}] معطلة (Inactive) ولا يمكن إضافتها لأمر الشراء.");
            }

            var lineGross = item.OrderedQuantity * item.UnitPrice;
            var lineNet = lineGross - item.DiscountAmount + item.TaxAmount;

            totalBeforeTax += lineGross;
            totalDiscount += item.DiscountAmount;
            totalTax += item.TaxAmount;

            poItems.Add(new PurchaseOrderItem
            {
                MaterialId = item.MaterialId,
                OrderedQuantity = item.OrderedQuantity,
                ReceivedQuantity = 0,
                Unit = string.IsNullOrWhiteSpace(item.Unit) ? material.Unit : item.Unit,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                TaxAmount = item.TaxAmount,
                TotalPrice = lineNet,
                Notes = item.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Generate deterministic PO Number: PO-YYYYMMDD-XXXX
        var date = request.OrderDate.Date;
        var count = await _repositoryManager.PurchaseOrderRepository.GetCountForDateAsync(date);
        var orderNumber = $"PO-{date:yyyyMMdd}-{(count + 1):D4}";

        int suffix = 1;
        while (!await _repositoryManager.PurchaseOrderRepository.IsOrderNumberUniqueAsync(orderNumber))
        {
            orderNumber = $"PO-{date:yyyyMMdd}-{(count + 1 + suffix):D4}";
            suffix++;
        }

        var po = new PurchaseOrder
        {
            OrderNumber = orderNumber,
            SupplierId = request.SupplierId,
            PurchaseRequestId = request.PurchaseRequestId,
            OrderDate = request.OrderDate,
            ExpectedDeliveryDate = request.ExpectedDeliveryDate,
            WarehouseId = request.WarehouseId,
            Status = PurchaseOrderStatus.Draft,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "EGP" : request.Currency,
            TotalBeforeTax = totalBeforeTax,
            DiscountAmount = totalDiscount,
            TaxAmount = totalTax,
            TotalAmount = totalBeforeTax - totalDiscount + totalTax,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = poItems
        };

        _repositoryManager.PurchaseOrderRepository.Create(po);
        await _repositoryManager.SaveAsync();

        return (await GetOrderByIdAsync(po.Id))!;
    }

    public async Task<PurchaseOrderDto> CreateOrderFromRequestAsync(int purchaseRequestId, int supplierId, int warehouseId, int userId)
    {
        var pr = await _repositoryManager.PurchaseRequestRepository.GetByIdWithDetailsAsync(purchaseRequestId);
        if (pr == null)
        {
            throw new KeyNotFoundException($"طلب الشراء برقم #{purchaseRequestId} غير موجود.");
        }

        if (pr.Status != PurchaseRequestStatus.Approved)
        {
            throw new InvalidOperationException($"لا يمكن توليد أمر شراء من طلب شراء غير معتمد. الحالة الحالية: {pr.Status}.");
        }

        var req = new CreatePurchaseOrderRequest
        {
            SupplierId = supplierId,
            PurchaseRequestId = purchaseRequestId,
            OrderDate = DateTime.UtcNow.Date,
            ExpectedDeliveryDate = pr.RequiredDate,
            WarehouseId = warehouseId,
            Notes = $"تم إنشاء أمر الشراء بناءً على طلب الشراء المعتمد رقم [{pr.RequestNumber}]. {pr.Notes}".Trim(),
            Items = pr.Items.Select(i => new CreatePurchaseOrderItemRequest
            {
                MaterialId = i.MaterialId,
                OrderedQuantity = i.RequestedQuantity,
                Unit = i.Unit,
                UnitPrice = i.EstimatedUnitPrice,
                Notes = i.Notes
            }).ToList()
        };

        return await CreateOrderAsync(req, userId);
    }

    public async Task<PurchaseOrderDto> UpdateOrderAsync(UpdatePurchaseOrderRequest request)
    {
        var po = await _repositoryManager.PurchaseOrderRepository.GetByIdWithDetailsAsync(request.Id, trackChanges: true);
        if (po == null)
        {
            throw new KeyNotFoundException($"أمر الشراء برقم #{request.Id} غير موجود.");
        }

        if (po.Status != PurchaseOrderStatus.Draft)
        {
            throw new InvalidOperationException($"لا يمكن تعديل أمر الشراء إلا في حالة المسودة (Draft). الحالة الحالية: {po.Status}.");
        }

        var supplier = await _repositoryManager.SupplierRepository.GetByIdWithDetailsAsync(request.SupplierId);
        if (supplier == null || !supplier.IsActive)
        {
            throw new InvalidOperationException("المورد المحدد غير صالح أو غير نشط.");
        }

        var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse == null || (warehouse.Type != WarehouseType.RawMaterial && warehouse.Type != WarehouseType.Packaging))
        {
            throw new InvalidOperationException("مستودع الاستلام يجب أن يكون مستودع خامات أو مواد تعبئة.");
        }

        // Clear existing items and recalculate
        po.Items.Clear();
        decimal totalBeforeTax = 0;
        decimal totalDiscount = 0;
        decimal totalTax = 0;

        foreach (var item in request.Items)
        {
            var lineGross = item.OrderedQuantity * item.UnitPrice;
            var lineNet = lineGross - item.DiscountAmount + item.TaxAmount;

            totalBeforeTax += lineGross;
            totalDiscount += item.DiscountAmount;
            totalTax += item.TaxAmount;

            po.Items.Add(new PurchaseOrderItem
            {
                PurchaseOrderId = po.Id,
                MaterialId = item.MaterialId,
                OrderedQuantity = item.OrderedQuantity,
                ReceivedQuantity = 0,
                Unit = item.Unit,
                UnitPrice = item.UnitPrice,
                DiscountAmount = item.DiscountAmount,
                TaxAmount = item.TaxAmount,
                TotalPrice = lineNet,
                Notes = item.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        po.SupplierId = request.SupplierId;
        po.PurchaseRequestId = request.PurchaseRequestId;
        po.OrderDate = request.OrderDate;
        po.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
        po.WarehouseId = request.WarehouseId;
        po.Currency = string.IsNullOrWhiteSpace(request.Currency) ? "EGP" : request.Currency;
        po.TotalBeforeTax = totalBeforeTax;
        po.DiscountAmount = totalDiscount;
        po.TaxAmount = totalTax;
        po.TotalAmount = totalBeforeTax - totalDiscount + totalTax;
        po.Notes = request.Notes;
        po.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PurchaseOrderRepository.Update(po);
        await _repositoryManager.SaveAsync();

        return (await GetOrderByIdAsync(po.Id))!;
    }

    public async Task<PurchaseOrderDto> SubmitOrderAsync(int id, int userId)
    {
        var po = await _repositoryManager.PurchaseOrderRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (po == null)
        {
            throw new KeyNotFoundException($"أمر الشراء برقم #{id} غير موجود.");
        }

        if (po.Status != PurchaseOrderStatus.Draft)
        {
            throw new InvalidOperationException($"لا يمكن تقديم أمر الشراء إلا في حالة المسودة (Draft).");
        }

        po.Status = PurchaseOrderStatus.Submitted;
        po.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PurchaseOrderRepository.Update(po);
        await _repositoryManager.SaveAsync();

        return (await GetOrderByIdAsync(po.Id))!;
    }

    public async Task<PurchaseOrderDto> ApproveOrderAsync(int id, int userId)
    {
        var po = await _repositoryManager.PurchaseOrderRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (po == null)
        {
            throw new KeyNotFoundException($"أمر الشراء برقم #{id} غير موجود.");
        }

        if (po.Status != PurchaseOrderStatus.Submitted && po.Status != PurchaseOrderStatus.Draft)
        {
            throw new InvalidOperationException($"لا يمكن اعتماد أمر الشراء إلا إذا كان مقدماً أو مسودة. الحالة الحالية: {po.Status}.");
        }

        po.Status = PurchaseOrderStatus.Approved;
        po.ApprovedByUserId = userId;
        po.ApprovedAt = DateTime.UtcNow;
        po.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PurchaseOrderRepository.Update(po);
        await _repositoryManager.SaveAsync();

        return (await GetOrderByIdAsync(po.Id))!;
    }

    public async Task<PurchaseOrderDto> CancelOrderAsync(int id, int userId, string? reason)
    {
        var po = await _repositoryManager.PurchaseOrderRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (po == null)
        {
            throw new KeyNotFoundException($"أمر الشراء برقم #{id} غير موجود.");
        }

        if (po.Status == PurchaseOrderStatus.FullyReceived || po.Status == PurchaseOrderStatus.Closed)
        {
            throw new InvalidOperationException("لا يمكن إلغاء أمر شراء تم استلامه كلياً أو مغلق.");
        }

        if (po.Receipts != null && po.Receipts.Any(r => r.Status == PurchaseReceiptStatus.Posted))
        {
            throw new InvalidOperationException("لا يمكن إلغاء أمر شراء تم ترحيل واستلام شحنات فعلية له مسبقاً.");
        }

        po.Status = PurchaseOrderStatus.Cancelled;
        po.Notes = string.IsNullOrWhiteSpace(reason) ? po.Notes : $"{po.Notes} [سبب الإلغاء: {reason}]".Trim();
        po.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PurchaseOrderRepository.Update(po);
        await _repositoryManager.SaveAsync();

        return (await GetOrderByIdAsync(po.Id))!;
    }

    public async Task<PurchaseOrderDto> CloseOrderAsync(int id, int userId, string? reason)
    {
        var po = await _repositoryManager.PurchaseOrderRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (po == null)
        {
            throw new KeyNotFoundException($"أمر الشراء برقم #{id} غير موجود.");
        }

        if (po.Status == PurchaseOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("لا يمكن إغلاق أمر شراء ملغي.");
        }

        po.Status = PurchaseOrderStatus.Closed;
        po.Notes = string.IsNullOrWhiteSpace(reason) ? po.Notes : $"{po.Notes} [تم الإغلاق: {reason}]".Trim();
        po.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PurchaseOrderRepository.Update(po);
        await _repositoryManager.SaveAsync();

        return (await GetOrderByIdAsync(po.Id))!;
    }

    public async Task<PurchaseOrderSummaryDto> GetSummaryAsync()
    {
        var orders = await _repositoryManager.PurchaseOrderRepository.GetAllOrdersAsync();

        return new PurchaseOrderSummaryDto
        {
            TotalOrders = orders.Count(),
            DraftOrders = orders.Count(o => o.Status == PurchaseOrderStatus.Draft),
            ApprovedOrders = orders.Count(o => o.Status == PurchaseOrderStatus.Approved),
            PartiallyReceivedOrders = orders.Count(o => o.Status == PurchaseOrderStatus.PartiallyReceived),
            FullyReceivedOrders = orders.Count(o => o.Status == PurchaseOrderStatus.FullyReceived),
            TotalPurchasingValue = orders.Where(o => o.Status != PurchaseOrderStatus.Cancelled).Sum(o => o.TotalAmount)
        };
    }

    public async Task<POReceivingInfoDto?> GetReceivingInfoAsync(int purchaseOrderId)
    {
        var po = await _repositoryManager.PurchaseOrderRepository.GetByIdWithDetailsAsync(purchaseOrderId);
        if (po == null) return null;

        return new POReceivingInfoDto
        {
            PurchaseOrderId = po.Id,
            OrderNumber = po.OrderNumber,
            SupplierId = po.SupplierId,
            SupplierName = po.Supplier?.Name ?? string.Empty,
            WarehouseId = po.WarehouseId,
            WarehouseName = po.Warehouse?.Name ?? string.Empty,
            Currency = po.Currency,
            Items = po.Items.Select(i => new POReceivingItemInfoDto
            {
                PurchaseOrderItemId = i.Id,
                MaterialId = i.MaterialId,
                MaterialName = i.Material?.Name ?? string.Empty,
                MaterialCode = i.Material?.Code ?? string.Empty,
                OrderedQuantity = i.OrderedQuantity,
                AlreadyReceivedQuantity = i.ReceivedQuantity,
                Unit = i.Unit,
                UnitPrice = i.UnitPrice,
                RequiresBatchNumber = true,
                RequiresExpiryDate = true
            }).ToList()
        };
    }
}
