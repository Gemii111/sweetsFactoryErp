using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using FactoryX.Infrastructure;

namespace FactoryX.Application.Services.Concretes;

public class SalesOrderService : ISalesOrderService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public SalesOrderService(IRepositoryManager repositoryManager, IMapper mapper, AppDbContext context)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<SalesOrderDto>> GetAllOrdersAsync(
        SalesOrderStatus? status = null,
        int? customerId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var orders = await _repositoryManager.SalesOrderRepository.GetAllOrdersAsync(
            status, customerId, warehouseId, fromDate, toDate, searchTerm);

        return _mapper.Map<IEnumerable<SalesOrderDto>>(orders);
    }

    public async Task<SalesOrderDto?> GetOrderByIdAsync(int id)
    {
        var order = await _repositoryManager.SalesOrderRepository.GetByIdWithDetailsAsync(id);
        if (order == null) return null;

        var dto = _mapper.Map<SalesOrderDto>(order);

        // Map fulfillments if present
        if (order.Fulfillments != null && order.Fulfillments.Any())
        {
            dto.Fulfillments = order.Fulfillments
                .OrderByDescending(f => f.Id)
                .Select(f => _mapper.Map<SalesFulfillmentDto>(f))
                .ToList();
            dto.FulfillmentsCount = order.Fulfillments.Count;
        }

        return dto;
    }

    public async Task<SalesOrderDto?> GetOrderByNumberAsync(string orderNumber)
    {
        var order = await _repositoryManager.SalesOrderRepository.GetByOrderNumberAsync(orderNumber);
        return _mapper.Map<SalesOrderDto>(order);
    }

    public async Task<SalesOrderDto> CreateOrderAsync(CreateSalesOrderRequest request)
    {
        // 1. Customer validation
        var customer = await _repositoryManager.CustomerRepository.GetByIdAsync(request.CustomerId);
        if (customer == null)
        {
            throw new KeyNotFoundException($"العميل بالمعرف #{request.CustomerId} غير موجود.");
        }

        if (!customer.IsActive)
        {
            throw new InvalidOperationException($"لا يمكن إنشاء أمر بيع لعميل غير نشط/معطل: '{customer.Name}'.");
        }

        // 2. Warehouse validation
        var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse == null || warehouse.Type != WarehouseType.FinishedGoods)
        {
            throw new InvalidOperationException("يجب أن يكون مستودع أمر البيع من نوع مستودع منتجات تامة (Finished Goods Warehouse).");
        }

        // 3. Generate Order Number
        var orderNumber = await GenerateNextOrderNumberAsync(request.OrderDate);

        var order = new SalesOrder
        {
            OrderNumber = orderNumber,
            CustomerId = request.CustomerId,
            WarehouseId = request.WarehouseId,
            OrderDate = request.OrderDate == default ? DateTime.UtcNow : request.OrderDate,
            RequiredDeliveryDate = request.RequiredDeliveryDate,
            Status = SalesOrderStatus.Draft,
            Priority = request.Priority,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        decimal calculatedSubTotal = 0;
        decimal calculatedDiscount = 0;
        decimal calculatedTax = 0;

        foreach (var itemReq in request.Items)
        {
            if (itemReq.OrderedQuantity <= 0)
            {
                throw new InvalidOperationException("كمية بنود أمر البيع يجب أن تكون أكبر من الصفر.");
            }

            var product = await _repositoryManager.ProductRepository.GetByIdAsync(itemReq.ProductId);
            if (product == null || !product.IsActive)
            {
                throw new InvalidOperationException($"المنتج التام بالمعرف #{itemReq.ProductId} غير موجود أو معطل.");
            }

            var unitPrice = itemReq.UnitPrice >= 0 ? itemReq.UnitPrice : product.SellingPrice;
            var lineSubTotal = itemReq.OrderedQuantity * unitPrice;
            var lineDiscount = Math.Max(0, itemReq.DiscountAmount);
            var lineTax = Math.Max(0, itemReq.TaxAmount);
            var lineTotal = Math.Max(0, lineSubTotal - lineDiscount + lineTax);

            calculatedSubTotal += lineSubTotal;
            calculatedDiscount += lineDiscount;
            calculatedTax += lineTax;

            var item = new SalesOrderItem
            {
                ProductId = itemReq.ProductId,
                OrderedQuantity = itemReq.OrderedQuantity,
                FulfilledQuantity = 0,
                Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? product.Unit : itemReq.Unit.Trim(),
                UnitPrice = unitPrice,
                DiscountAmount = lineDiscount,
                TaxAmount = lineTax,
                TotalPrice = lineTotal,
                Notes = itemReq.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            order.Items.Add(item);
        }

        order.SubTotal = calculatedSubTotal;
        order.DiscountAmount = calculatedDiscount;
        order.TaxAmount = calculatedTax;
        order.TotalAmount = Math.Max(0, calculatedSubTotal - calculatedDiscount + calculatedTax);

        _repositoryManager.SalesOrderRepository.Create(order);
        await _repositoryManager.SaveAsync();

        return (await GetOrderByIdAsync(order.Id))!;
    }

    public async Task<SalesOrderDto> UpdateOrderAsync(UpdateSalesOrderRequest request)
    {
        var order = await _repositoryManager.SalesOrderRepository.GetByIdWithDetailsAsync(request.Id, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر البيع بالمعرف #{request.Id} غير موجود.");
        }

        if (order.Status != SalesOrderStatus.Draft)
        {
            throw new InvalidOperationException("لا يمكن تعديل أمر بيع تم اعتماده أو بدأ صرفه أو تم إلغاؤه.");
        }

        // Customer validation
        var customer = await _repositoryManager.CustomerRepository.GetByIdAsync(request.CustomerId);
        if (customer == null || !customer.IsActive)
        {
            throw new InvalidOperationException("العميل غير موجود أو معطل.");
        }

        // Warehouse validation
        var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse == null || warehouse.Type != WarehouseType.FinishedGoods)
        {
            throw new InvalidOperationException("مستودع أمر البيع يجب أن يكون مستودع منتجات تامة.");
        }

        order.CustomerId = request.CustomerId;
        order.WarehouseId = request.WarehouseId;
        order.OrderDate = request.OrderDate;
        order.RequiredDeliveryDate = request.RequiredDeliveryDate;
        order.Priority = request.Priority;
        order.Notes = request.Notes?.Trim();
        order.UpdatedAt = DateTime.UtcNow;

        // Clear existing items
        _context.SalesOrderItems.RemoveRange(order.Items);
        order.Items.Clear();

        decimal calculatedSubTotal = 0;
        decimal calculatedDiscount = 0;
        decimal calculatedTax = 0;

        foreach (var itemReq in request.Items)
        {
            if (itemReq.OrderedQuantity <= 0) continue;

            var product = await _repositoryManager.ProductRepository.GetByIdAsync(itemReq.ProductId);
            if (product == null || !product.IsActive)
            {
                throw new InvalidOperationException($"المنتج بالمعرف #{itemReq.ProductId} غير موجود أو معطل.");
            }

            var unitPrice = itemReq.UnitPrice >= 0 ? itemReq.UnitPrice : product.SellingPrice;
            var lineSubTotal = itemReq.OrderedQuantity * unitPrice;
            var lineDiscount = Math.Max(0, itemReq.DiscountAmount);
            var lineTax = Math.Max(0, itemReq.TaxAmount);
            var lineTotal = Math.Max(0, lineSubTotal - lineDiscount + lineTax);

            calculatedSubTotal += lineSubTotal;
            calculatedDiscount += lineDiscount;
            calculatedTax += lineTax;

            var item = new SalesOrderItem
            {
                SalesOrderId = order.Id,
                ProductId = itemReq.ProductId,
                OrderedQuantity = itemReq.OrderedQuantity,
                FulfilledQuantity = 0,
                Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? product.Unit : itemReq.Unit.Trim(),
                UnitPrice = unitPrice,
                DiscountAmount = lineDiscount,
                TaxAmount = lineTax,
                TotalPrice = lineTotal,
                Notes = itemReq.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            order.Items.Add(item);
        }

        order.SubTotal = calculatedSubTotal;
        order.DiscountAmount = calculatedDiscount;
        order.TaxAmount = calculatedTax;
        order.TotalAmount = Math.Max(0, calculatedSubTotal - calculatedDiscount + calculatedTax);

        _repositoryManager.SalesOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return (await GetOrderByIdAsync(order.Id))!;
    }

    public async Task<bool> ConfirmOrderAsync(int id, int userId)
    {
        var order = await _repositoryManager.SalesOrderRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر البيع بالمعرف #{id} غير موجود.");
        }

        if (order.Status != SalesOrderStatus.Draft)
        {
            throw new InvalidOperationException($"لا يمكن اعتماد أمر البيع في حالته الحالية: {order.Status}.");
        }

        if (!order.Items.Any())
        {
            throw new InvalidOperationException("لا يمكن اعتماد أمر بيع بدون بنود منتجات.");
        }

        order.Status = SalesOrderStatus.Confirmed;
        order.ConfirmedByUserId = userId;
        order.ConfirmedAt = DateTime.UtcNow;
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.SalesOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return true;
    }

    public async Task<bool> CancelOrderAsync(int id, string? reason, int userId)
    {
        var order = await _repositoryManager.SalesOrderRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر البيع بالمعرف #{id} غير موجود.");
        }

        if (order.Status == SalesOrderStatus.PartiallyFulfilled || order.Status == SalesOrderStatus.FullyFulfilled)
        {
            throw new InvalidOperationException("لا يمكن إلغاء أمر بيع تم تسليم أو صرف منتجاته كلياً أو جزئياً.");
        }

        if (order.Status == SalesOrderStatus.Cancelled || order.Status == SalesOrderStatus.Closed)
        {
            throw new InvalidOperationException("أمر البيع ملغي أو مغلق بالفعل.");
        }

        order.Status = SalesOrderStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                ? $"سبب الإلغاء: {reason.Trim()}"
                : $"{order.Notes} | سبب الإلغاء: {reason.Trim()}";
        }
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.SalesOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return true;
    }

    public async Task<bool> CloseOrderAsync(int id, int userId)
    {
        var order = await _repositoryManager.SalesOrderRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر البيع بالمعرف #{id} غير موجود.");
        }

        if (order.Status != SalesOrderStatus.FullyFulfilled && order.Status != SalesOrderStatus.PartiallyFulfilled)
        {
            throw new InvalidOperationException("لا يمكن إغلاق أمر بيع إلا بعد صرفه جزئياً أو كلياً.");
        }

        order.Status = SalesOrderStatus.Closed;
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.SalesOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return true;
    }

    public async Task<SalesOrderSummaryDto> GetSummaryAsync()
    {
        var all = (await _repositoryManager.SalesOrderRepository.GetAllOrdersAsync()).ToList();

        return new SalesOrderSummaryDto
        {
            TotalOrders = all.Count,
            DraftOrders = all.Count(o => o.Status == SalesOrderStatus.Draft),
            ConfirmedOrders = all.Count(o => o.Status == SalesOrderStatus.Confirmed),
            PartiallyFulfilledOrders = all.Count(o => o.Status == SalesOrderStatus.PartiallyFulfilled),
            FullyFulfilledOrders = all.Count(o => o.Status == SalesOrderStatus.FullyFulfilled),
            TotalOrderValue = all.Where(o => o.Status != SalesOrderStatus.Cancelled).Sum(o => o.TotalAmount)
        };
    }

    public async Task<string> GenerateNextOrderNumberAsync(DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var datePrefix = $"SO-{targetDate:yyyyMMdd}";

        var count = await _repositoryManager.SalesOrderRepository.GetCountForDateAsync(targetDate);
        var nextNum = count + 1;
        var orderNumber = $"{datePrefix}-{nextNum:D4}";

        while (!await _repositoryManager.SalesOrderRepository.IsOrderNumberUniqueAsync(orderNumber))
        {
            nextNum++;
            orderNumber = $"{datePrefix}-{nextNum:D4}";
        }

        return orderNumber;
    }

    public async Task<SalesOrderFulfillmentInfoDto?> GetFulfillmentInfoAsync(int id)
    {
        var order = await _repositoryManager.SalesOrderRepository.GetByIdWithDetailsAsync(id);
        if (order == null) return null;

        var result = new SalesOrderFulfillmentInfoDto
        {
            SalesOrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerName = order.Customer?.Name ?? string.Empty,
            CustomerCode = order.Customer?.Code ?? string.Empty,
            WarehouseId = order.WarehouseId,
            WarehouseName = order.Warehouse?.Name ?? string.Empty,
            OrderDate = order.OrderDate,
            Status = order.Status.ToString()
        };

        var today = DateTime.UtcNow.Date;

        foreach (var item in order.Items)
        {
            var remaining = Math.Max(0, item.OrderedQuantity - item.FulfilledQuantity);

            // Fetch available finished goods stocks for this product and warehouse
            var stocks = await _context.FinishedGoodsStocks
                .Include(s => s.Location)
                .Include(s => s.Warehouse)
                .Where(s => s.ProductId == item.ProductId && s.WarehouseId == order.WarehouseId && s.Quantity > 0)
                .OrderBy(s => s.ExpiryDate) // FEFO
                .ThenBy(s => s.ProductionDate)
                .ToListAsync();

            var batchList = stocks.Select(s => new BatchAvailabilityDto
            {
                FinishedGoodsStockId = s.Id,
                BatchNumber = s.BatchNumber,
                ProductionDate = s.ProductionDate,
                ExpiryDate = s.ExpiryDate,
                WarehouseId = s.WarehouseId,
                WarehouseName = s.Warehouse?.Name ?? string.Empty,
                LocationId = s.LocationId,
                LocationName = s.Location?.Name,
                AvailableQuantity = s.Quantity,
                UnitCost = s.UnitCost,
                UnitPrice = item.UnitPrice,
                IsExpired = s.ExpiryDate.Date <= today
            }).ToList();

            var validAvailableQty = batchList.Where(b => !b.IsExpired).Sum(b => b.AvailableQuantity);

            result.Items.Add(new SalesOrderFulfillmentItemInfoDto
            {
                SalesOrderItemId = item.Id,
                ProductId = item.ProductId,
                ProductName = item.Product?.Name ?? string.Empty,
                ProductCode = item.Product?.Code ?? string.Empty,
                Unit = item.Unit,
                UnitPrice = item.UnitPrice,
                OrderedQuantity = item.OrderedQuantity,
                FulfilledQuantity = item.FulfilledQuantity,
                RemainingQuantity = remaining,
                AvailableStockQuantity = validAvailableQty,
                AvailableBatches = batchList
            });
        }

        return result;
    }
}
