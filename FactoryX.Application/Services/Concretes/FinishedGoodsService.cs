using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class FinishedGoodsService : IFinishedGoodsService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public FinishedGoodsService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        AppDbContext context)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<FinishedGoodsStockDto>> GetStockAsync(
        int? warehouseId = null,
        int? locationId = null,
        int? productId = null,
        string? batchNumber = null)
    {
        var stocks = await _repositoryManager.FinishedGoodsStockRepository.GetStockBalancesAsync(
            warehouseId, locationId, productId, batchNumber);

        return _mapper.Map<IEnumerable<FinishedGoodsStockDto>>(stocks);
    }

    public async Task<FinishedGoodsStockDto?> GetStockByIdAsync(int id)
    {
        var stock = await _repositoryManager.FinishedGoodsStockRepository.GetByIdWithDetailsAsync(id);
        if (stock == null) return null;

        var dto = _mapper.Map<FinishedGoodsStockDto>(stock);

        // Fetch WorkOrder if available
        if (stock.ProductionBatch != null)
        {
            dto.WorkOrderId = stock.ProductionBatch.WorkOrderId;
            dto.WorkOrderNumber = stock.ProductionBatch.WorkOrder?.OrderNumber;
            dto.QCStatus = stock.ProductionBatch.QualityStatus;
        }

        if (stock.QCInspection != null)
        {
            dto.QCInspectionNumber = stock.QCInspection.InspectionNumber;
        }

        if (stock.PackagingOrder != null)
        {
            dto.PackagingOrderNumber = stock.PackagingOrder.OrderNumber;
            dto.PackagingBOMName = stock.PackagingOrder.PackagingBOM?.Name;
        }

        return dto;
    }

    public async Task<FinishedGoodsStockSummaryDto> GetStockSummaryAsync()
    {
        var allStocks = (await _repositoryManager.FinishedGoodsStockRepository.GetStockBalancesAsync()).ToList();

        var totalQty = allStocks.Sum(s => s.Quantity);
        var totalVal = allStocks.Sum(s => s.TotalCost);
        var batchesCount = allStocks.Select(s => s.BatchNumber).Distinct().Count();
        var productsCount = allStocks.Select(s => s.ProductId).Distinct().Count();

        var now = DateTime.UtcNow;
        var in30Days = now.AddDays(30);

        var expiringSoon = allStocks.Count(s => s.Quantity > 0 && s.ExpiryDate > now && s.ExpiryDate <= in30Days);
        var expired = allStocks.Count(s => s.Quantity > 0 && s.ExpiryDate <= now);

        return new FinishedGoodsStockSummaryDto
        {
            TotalQuantity = totalQty,
            TotalValue = totalVal,
            TotalBatchesCount = batchesCount,
            TotalProductsCount = productsCount,
            ExpiringSoonCount = expiringSoon,
            ExpiredCount = expired
        };
    }

    public async Task<IEnumerable<FinishedGoodsMovementDto>> GetStockMovementsAsync(
        int? warehouseId = null,
        int? productId = null,
        string? batchNumber = null,
        InventoryTransactionType? transactionType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.InventoryTransactions.AsNoTracking();

        // Filter by FG transaction types
        if (transactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == transactionType.Value);
        }
        else
        {
            query = query.Where(t =>
                t.TransactionType == InventoryTransactionType.FinishedGoodsReceipt ||
                t.TransactionType == InventoryTransactionType.FinishedGoodsAdjustment ||
                t.TransactionType == InventoryTransactionType.FinishedGoodsTransfer ||
                t.TransactionType == InventoryTransactionType.StockAdjustment ||
                t.TransactionType == InventoryTransactionType.StockTransfer);
        }

        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            query = query.Where(t => t.WarehouseId == warehouseId.Value);
        }

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(batchNumber))
        {
            var cleanBatch = batchNumber.Trim();
            query = query.Where(t => t.BatchNumber.Contains(cleanBatch));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(t => t.TransactionDate <= endOfDay);
        }

        var list = await query
            .Include(t => t.Product)
            .Include(t => t.Warehouse)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .Include(t => t.User)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        return list.Select(t => new FinishedGoodsMovementDto
        {
            Id = t.Id,
            TransactionType = t.TransactionType,
            TransactionDate = t.TransactionDate,
            ProductId = t.ProductId,
            ProductName = t.Product?.Name ?? string.Empty,
            ProductCode = t.Product?.Code ?? string.Empty,
            BatchNumber = t.BatchNumber,
            WarehouseId = t.WarehouseId,
            WarehouseName = t.Warehouse?.Name ?? string.Empty,
            SourceLocationId = t.SourceLocationId,
            SourceLocationName = t.SourceLocation?.Name,
            DestinationLocationId = t.DestinationLocationId,
            DestinationLocationName = t.DestinationLocation?.Name,
            Quantity = t.Quantity,
            Unit = t.Unit,
            UnitCost = t.UnitCost,
            TotalCost = t.TotalCost,
            ReferenceDocumentNumber = t.ReferenceDocumentNumber,
            UserId = t.UserId,
            UserName = t.User?.FullName ?? t.User?.Username,
            Notes = t.Notes
        });
    }

    public async Task<bool> AdjustStockAsync(FinishedGoodsAdjustmentRequest request, int userId)
    {
        if (request.ActualQuantity < 0)
        {
            throw new InvalidOperationException("الكمية الفعلية للمخزون لا يمكن أن تكون سالبة.");
        }

        var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse == null || warehouse.Type != WarehouseType.FinishedGoods)
        {
            throw new InvalidOperationException("مستودع التسوية يجب أن يكون مستودع منتجات تامة (Finished Goods Warehouse).");
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var stock = await _repositoryManager.FinishedGoodsStockRepository.FindStockAsync(
                request.WarehouseId, request.LocationId, request.ProductId, request.BatchNumber, trackChanges: true);

            var product = await _repositoryManager.ProductRepository.GetByIdAsync(request.ProductId);
            var oldQty = stock?.Quantity ?? 0m;
            var diff = request.ActualQuantity - oldQty;

            var unitCost = stock?.UnitCost ?? product?.StandardCost ?? 0m;
            var totalDiffValue = Math.Abs(diff) * unitCost;

            if (stock == null)
            {
                stock = new FinishedGoodsStock
                {
                    ProductId = request.ProductId,
                    ProductionBatchId = 0,
                    WarehouseId = request.WarehouseId,
                    LocationId = request.LocationId,
                    BatchNumber = request.BatchNumber,
                    Quantity = request.ActualQuantity,
                    Unit = request.Unit,
                    ProductionDate = DateTime.UtcNow.Date,
                    ExpiryDate = DateTime.UtcNow.Date.AddDays(product?.ExpiryPeriodDays ?? 180),
                    UnitCost = unitCost,
                    TotalCost = request.ActualQuantity * unitCost,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _repositoryManager.FinishedGoodsStockRepository.Create(stock);
            }
            else
            {
                stock.Quantity = request.ActualQuantity;
                stock.TotalCost = request.ActualQuantity * stock.UnitCost;
                stock.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.FinishedGoodsStockRepository.Update(stock);
            }

            var refDoc = $"ADJ-FG-{DateTime.UtcNow:yyyyMMddHHmmss}";
            var invTx = new InventoryTransaction
            {
                TransactionType = InventoryTransactionType.FinishedGoodsAdjustment,
                TransactionDate = DateTime.UtcNow,
                WarehouseId = request.WarehouseId,
                DestinationLocationId = request.LocationId,
                ProductId = request.ProductId,
                BatchNumber = request.BatchNumber,
                Quantity = diff,
                Unit = request.Unit,
                UnitCost = unitCost,
                TotalCost = totalDiffValue,
                ReferenceDocumentNumber = refDoc,
                UserId = userId,
                Notes = $"تسوية مخزون منتج تام (فارق: {diff:+0.##;-0.##}). السبب: {request.Reason}".Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _repositoryManager.InventoryTransactionRepository.Create(invTx);

            await _repositoryManager.SaveAsync();
            await dbTransaction.CommitAsync();
            return true;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> TransferStockAsync(FinishedGoodsTransferRequest request, int userId)
    {
        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("كمية النقل يجب أن تكون أكبر من الصفر.");
        }

        if (request.SourceWarehouseId == request.DestinationWarehouseId && request.SourceLocationId == request.DestinationLocationId)
        {
            throw new InvalidOperationException("مستودع وموقع المصدر لا يمكن أن يتطابقا مع مستودع وموقع الوجهة.");
        }

        var srcWh = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.SourceWarehouseId);
        var destWh = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.DestinationWarehouseId);

        if (srcWh == null || srcWh.Type != WarehouseType.FinishedGoods)
        {
            throw new InvalidOperationException("مستودع المصدر يجب أن يكون مستودع منتجات تامة (Finished Goods Warehouse).");
        }

        if (destWh == null || destWh.Type != WarehouseType.FinishedGoods)
        {
            throw new InvalidOperationException("مستودع الوجهة يجب أن يكون مستودع منتجات تامة (Finished Goods Warehouse). لا يمكن نقل المنتجات التامة إلى مستودع خامات أو تعبئة.");
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Find source stock
            var srcStock = await _repositoryManager.FinishedGoodsStockRepository.FindStockAsync(
                request.SourceWarehouseId, request.SourceLocationId, request.ProductId, request.BatchNumber, trackChanges: true);

            if (srcStock == null || srcStock.Quantity < request.Quantity)
            {
                throw new InvalidOperationException($"الرصيد المتاح في موقع المصدر غير كافٍ. المطلوب: {request.Quantity}، المتاح: {srcStock?.Quantity ?? 0}");
            }

            // Deduct from source
            srcStock.Quantity -= request.Quantity;
            srcStock.TotalCost = srcStock.Quantity * srcStock.UnitCost;
            srcStock.UpdatedAt = DateTime.UtcNow;
            _repositoryManager.FinishedGoodsStockRepository.Update(srcStock);

            // Find or create destination stock
            var destStock = await _repositoryManager.FinishedGoodsStockRepository.FindStockAsync(
                request.DestinationWarehouseId, request.DestinationLocationId, request.ProductId, request.BatchNumber, trackChanges: true);

            var unitCost = srcStock.UnitCost;
            var transferValue = request.Quantity * unitCost;

            if (destStock == null)
            {
                destStock = new FinishedGoodsStock
                {
                    ProductId = request.ProductId,
                    ProductionBatchId = srcStock.ProductionBatchId,
                    WarehouseId = request.DestinationWarehouseId,
                    LocationId = request.DestinationLocationId,
                    BatchNumber = request.BatchNumber,
                    Quantity = request.Quantity,
                    Unit = request.Unit,
                    ProductionDate = srcStock.ProductionDate,
                    ExpiryDate = srcStock.ExpiryDate,
                    UnitCost = unitCost,
                    TotalCost = transferValue,
                    QCInspectionId = srcStock.QCInspectionId,
                    PackagingOrderId = srcStock.PackagingOrderId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _repositoryManager.FinishedGoodsStockRepository.Create(destStock);
            }
            else
            {
                destStock.Quantity += request.Quantity;
                destStock.TotalCost += transferValue;
                destStock.UnitCost = destStock.Quantity > 0 ? (destStock.TotalCost / destStock.Quantity) : unitCost;
                destStock.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.FinishedGoodsStockRepository.Update(destStock);
            }

            var refDoc = !string.IsNullOrWhiteSpace(request.ReferenceNumber) ? request.ReferenceNumber : $"TRSF-FG-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var invTx = new InventoryTransaction
            {
                TransactionType = InventoryTransactionType.FinishedGoodsTransfer,
                TransactionDate = DateTime.UtcNow,
                WarehouseId = request.SourceWarehouseId,
                SourceLocationId = request.SourceLocationId,
                DestinationLocationId = request.DestinationLocationId,
                ProductId = request.ProductId,
                BatchNumber = request.BatchNumber,
                Quantity = request.Quantity,
                Unit = request.Unit,
                UnitCost = unitCost,
                TotalCost = transferValue,
                ReferenceDocumentNumber = refDoc,
                UserId = userId,
                Notes = $"نقل منتج تام من مستودع {srcWh.Name} إلى مستودع {destWh.Name}. {request.Notes}".Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _repositoryManager.InventoryTransactionRepository.Create(invTx);

            await _repositoryManager.SaveAsync();
            await dbTransaction.CommitAsync();
            return true;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
}
