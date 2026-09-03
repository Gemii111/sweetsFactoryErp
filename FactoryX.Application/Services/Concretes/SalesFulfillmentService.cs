using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using FactoryX.Infrastructure;

namespace FactoryX.Application.Services.Concretes;

public class SalesFulfillmentService : ISalesFulfillmentService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IAccountingPostingService _postingService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public SalesFulfillmentService(
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

    public async Task<IEnumerable<SalesFulfillmentDto>> GetAllFulfillmentsAsync(
        SalesFulfillmentStatus? status = null,
        int? salesOrderId = null,
        int? customerId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var fulfillments = await _repositoryManager.SalesFulfillmentRepository.GetAllFulfillmentsAsync(
            status, salesOrderId, customerId, warehouseId, fromDate, toDate, searchTerm);

        return _mapper.Map<IEnumerable<SalesFulfillmentDto>>(fulfillments);
    }

    public async Task<SalesFulfillmentDto?> GetFulfillmentByIdAsync(int id)
    {
        var fulfillment = await _repositoryManager.SalesFulfillmentRepository.GetByIdWithDetailsAsync(id);
        return _mapper.Map<SalesFulfillmentDto>(fulfillment);
    }

    public async Task<SalesFulfillmentDto?> GetFulfillmentByNumberAsync(string fulfillmentNumber)
    {
        var fulfillment = await _repositoryManager.SalesFulfillmentRepository.GetByFulfillmentNumberAsync(fulfillmentNumber);
        return _mapper.Map<SalesFulfillmentDto>(fulfillment);
    }

    public async Task<SalesFulfillmentDto> CreateFulfillmentAsync(CreateSalesFulfillmentRequest request, int userId)
    {
        // 1. Validate Sales Order
        var order = await _repositoryManager.SalesOrderRepository.GetByIdWithDetailsAsync(request.SalesOrderId, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر البيع بالمعرف #{request.SalesOrderId} غير موجود.");
        }

        if (order.Status != SalesOrderStatus.Confirmed && order.Status != SalesOrderStatus.PartiallyFulfilled)
        {
            throw new InvalidOperationException($"لا يمكن صرف شحنة لأمر بيع في حالة '{order.Status}'. يجب أن يكون أمر البيع معتمداً (Confirmed) أو مستلماً جزئياً (PartiallyFulfilled).");
        }

        // 2. Validate Customer
        var customer = await _repositoryManager.CustomerRepository.GetByIdAsync(order.CustomerId);
        if (customer == null)
        {
            throw new KeyNotFoundException($"العميل بالمعرف #{order.CustomerId} غير موجود.");
        }

        // 3. Validate Warehouse
        var targetWarehouseId = request.WarehouseId > 0 ? request.WarehouseId : order.WarehouseId;
        var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(targetWarehouseId);
        if (warehouse == null || warehouse.Type != WarehouseType.FinishedGoods)
        {
            throw new InvalidOperationException("يجب أن يكون مستودع الصرف والتسليم من نوع مستودع منتجات تامة (Finished Goods Warehouse).");
        }

        // 4. Validate Items
        var validItems = request.Items.Where(i => i.ShippedQuantity > 0).ToList();
        if (!validItems.Any())
        {
            throw new InvalidOperationException("يجب تحديد بند واحد على الأقل بكمية صرف أكبر من الصفر.");
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var fulfillmentNumber = await GenerateNextFulfillmentNumberAsync(request.FulfillmentDate);

            var fulfillment = new SalesFulfillment
            {
                FulfillmentNumber = fulfillmentNumber,
                SalesOrderId = order.Id,
                CustomerId = order.CustomerId,
                WarehouseId = targetWarehouseId,
                FulfillmentDate = request.FulfillmentDate == default ? DateTime.UtcNow : request.FulfillmentDate,
                Status = SalesFulfillmentStatus.Shipped,
                ShippedByUserId = userId,
                Notes = request.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            decimal totalFulfillmentQty = 0;
            decimal totalFulfillmentCost = 0;
            decimal totalFulfillmentPrice = 0;
            var today = DateTime.UtcNow.Date;

            foreach (var itemReq in validItems)
            {
                var product = await _repositoryManager.ProductRepository.GetByIdAsync(itemReq.ProductId);
                if (product == null || !product.IsActive)
                {
                    throw new InvalidOperationException($"المنتج التام بالمعرف #{itemReq.ProductId} غير موجود أو معطل.");
                }

                // Match with SalesOrderItem if provided
                SalesOrderItem? orderItem = null;
                if (itemReq.SalesOrderItemId.HasValue && itemReq.SalesOrderItemId.Value > 0)
                {
                    orderItem = order.Items.FirstOrDefault(i => i.Id == itemReq.SalesOrderItemId.Value);
                }
                else
                {
                    orderItem = order.Items.FirstOrDefault(i => i.ProductId == itemReq.ProductId);
                }

                if (orderItem == null)
                {
                    throw new InvalidOperationException($"المنتج '{product.Name}' غير موجود ضمن بنود أمر البيع {order.OrderNumber}.");
                }

                var remainingQty = Math.Max(0, orderItem.OrderedQuantity - orderItem.FulfilledQuantity);
                if (itemReq.ShippedQuantity > remainingQty)
                {
                    throw new InvalidOperationException($"الكمية المصروفة ({itemReq.ShippedQuantity}) للمنتج '{product.Name}' تتجاوز الكمية المتبقية للتسليم في أمر البيع ({remainingQty} {orderItem.Unit}).");
                }

                // Locate FinishedGoodsStock
                FinishedGoodsStock? stock = null;
                if (itemReq.FinishedGoodsStockId.HasValue && itemReq.FinishedGoodsStockId.Value > 0)
                {
                    stock = await _context.FinishedGoodsStocks
                        .FirstOrDefaultAsync(s => s.Id == itemReq.FinishedGoodsStockId.Value);
                }

                if (stock == null)
                {
                    stock = await _context.FinishedGoodsStocks
                        .Where(s => s.ProductId == itemReq.ProductId &&
                                    s.WarehouseId == targetWarehouseId &&
                                    s.BatchNumber == itemReq.BatchNumber.Trim())
                        .FirstOrDefaultAsync();
                }

                if (stock == null || stock.Quantity < itemReq.ShippedQuantity)
                {
                    var available = stock?.Quantity ?? 0m;
                    throw new InvalidOperationException($"عجز في رصيد المنتج التام '{product.Name}' للتشغيلة '{itemReq.BatchNumber}'. المطلوب: {itemReq.ShippedQuantity} {orderItem.Unit}، المتاح: {available} {orderItem.Unit}.");
                }

                // Expiry Check
                if (stock.ExpiryDate.Date <= today)
                {
                    throw new InvalidOperationException($"لا يمكن صرف تشغيلة منتهية الصلاحية: التشغيلة '{stock.BatchNumber}' للمنتج '{product.Name}' انتهت صلاحيتها بتاريخ {stock.ExpiryDate:yyyy-MM-dd}.");
                }

                // Location validation if provided
                if (itemReq.LocationId.HasValue && itemReq.LocationId.Value > 0)
                {
                    var loc = await _repositoryManager.WarehouseLocationRepository.GetByIdAsync(itemReq.LocationId.Value);
                    if (loc == null || loc.WarehouseId != targetWarehouseId)
                    {
                        throw new InvalidOperationException($"مكان التخزين المحدد لا يتبع للمستودع '{warehouse.Name}'.");
                    }
                }

                // Deduct from FinishedGoodsStock
                stock.Quantity -= itemReq.ShippedQuantity;
                stock.TotalCost = Math.Round(stock.Quantity * stock.UnitCost, 2);
                stock.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.FinishedGoodsStockRepository.Update(stock);

                // Create InventoryTransaction (SalesShipment)
                var lineUnitCost = stock.UnitCost > 0 ? stock.UnitCost : product.StandardCost;
                var lineTotalCost = Math.Round(lineUnitCost * itemReq.ShippedQuantity, 4);
                var unitPrice = itemReq.UnitPrice > 0 ? itemReq.UnitPrice : orderItem.UnitPrice;
                var lineTotalPrice = Math.Round(unitPrice * itemReq.ShippedQuantity, 2);

                var invTransaction = new InventoryTransaction
                {
                    TransactionType = InventoryTransactionType.SalesShipment,
                    TransactionDate = DateTime.UtcNow,
                    WarehouseId = targetWarehouseId,
                    SourceLocationId = itemReq.LocationId ?? stock.LocationId,
                    ProductId = itemReq.ProductId,
                    BatchNumber = stock.BatchNumber,
                    Quantity = itemReq.ShippedQuantity,
                    Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? orderItem.Unit : itemReq.Unit.Trim(),
                    UnitCost = lineUnitCost,
                    TotalCost = lineTotalCost,
                    ReferenceDocumentNumber = fulfillmentNumber,
                    UserId = userId,
                    Notes = itemReq.Notes ?? $"صرف مبيعات بموجب إذن تسليم {fulfillmentNumber} لأمر البيع {order.OrderNumber}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _repositoryManager.InventoryTransactionRepository.Create(invTransaction);
                await _repositoryManager.SaveAsync(); // Generates invTransaction.Id

                // Update Order Item Fulfilled Quantity
                orderItem.FulfilledQuantity += itemReq.ShippedQuantity;
                orderItem.UpdatedAt = DateTime.UtcNow;

                // Add to fulfillment items
                var fulfillmentItem = new SalesFulfillmentItem
                {
                    SalesOrderItemId = orderItem.Id,
                    ProductId = itemReq.ProductId,
                    FinishedGoodsStockId = stock.Id,
                    BatchNumber = stock.BatchNumber,
                    ProductionDate = stock.ProductionDate,
                    ExpiryDate = stock.ExpiryDate,
                    WarehouseId = targetWarehouseId,
                    LocationId = itemReq.LocationId ?? stock.LocationId,
                    OrderedQuantity = orderItem.OrderedQuantity,
                    ShippedQuantity = itemReq.ShippedQuantity,
                    Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? orderItem.Unit : itemReq.Unit.Trim(),
                    UnitCost = lineUnitCost,
                    TotalCost = lineTotalCost,
                    UnitPrice = unitPrice,
                    TotalPrice = lineTotalPrice,
                    InventoryTransactionId = invTransaction.Id,
                    Notes = itemReq.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                fulfillment.Items.Add(fulfillmentItem);

                totalFulfillmentQty += itemReq.ShippedQuantity;
                totalFulfillmentCost += lineTotalCost;
                totalFulfillmentPrice += lineTotalPrice;
            }

            fulfillment.TotalQuantity = totalFulfillmentQty;
            fulfillment.TotalCost = totalFulfillmentCost;
            fulfillment.TotalPrice = totalFulfillmentPrice;
            _repositoryManager.SalesFulfillmentRepository.Create(fulfillment);

            // Update Sales Order Status (PartiallyFulfilled vs FullyFulfilled)
            bool isAllFullyFulfilled = order.Items.All(i => i.FulfilledQuantity >= i.OrderedQuantity);
            if (isAllFullyFulfilled)
            {
                order.Status = SalesOrderStatus.FullyFulfilled;
            }
            else if (order.Items.Any(i => i.FulfilledQuantity > 0))
            {
                order.Status = SalesOrderStatus.PartiallyFulfilled;
            }
            order.UpdatedAt = DateTime.UtcNow;
            _repositoryManager.SalesOrderRepository.Update(order);

            await _repositoryManager.SaveAsync();
            await dbTransaction.CommitAsync();

            // Automatic Accounting Posting: Dr COGS, Cr Finished Goods Inventory
            await _postingService.PostSalesFulfillmentAsync(fulfillment.Id, userId);

            return (await GetFulfillmentByIdAsync(fulfillment.Id))!;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<SalesFulfillmentSummaryDto> GetSummaryAsync()
    {
        var fulfillments = (await _repositoryManager.SalesFulfillmentRepository.GetAllFulfillmentsAsync()).ToList();

        return new SalesFulfillmentSummaryDto
        {
            TotalFulfillments = fulfillments.Count,
            ShippedCount = fulfillments.Count(f => f.Status == SalesFulfillmentStatus.Shipped),
            TotalShippedQuantity = fulfillments.Where(f => f.Status == SalesFulfillmentStatus.Shipped).Sum(f => f.TotalQuantity),
            TotalShippedValue = fulfillments.Where(f => f.Status == SalesFulfillmentStatus.Shipped).Sum(f => f.TotalPrice)
        };
    }

    public async Task<string> GenerateNextFulfillmentNumberAsync(DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var datePrefix = $"SF-{targetDate:yyyyMMdd}";

        var count = await _repositoryManager.SalesFulfillmentRepository.GetCountForDateAsync(targetDate);
        var nextNum = count + 1;
        var fulfillmentNumber = $"{datePrefix}-{nextNum:D4}";

        while (!await _repositoryManager.SalesFulfillmentRepository.IsFulfillmentNumberUniqueAsync(fulfillmentNumber))
        {
            nextNum++;
            fulfillmentNumber = $"{datePrefix}-{nextNum:D4}";
        }

        return fulfillmentNumber;
    }
}
