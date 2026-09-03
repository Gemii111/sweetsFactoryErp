using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using FactoryX.Infrastructure;

namespace FactoryX.Application.Services.Concretes;

public class InventoryService : IInventoryService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public InventoryService(IRepositoryManager repositoryManager, IMapper mapper, AppDbContext context)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<StockBalanceDto>> GetStockAsync(
        int? warehouseId, int? locationId, int? materialId, int? productId, string? batchNumber)
    {
        var balances = await _repositoryManager.StockBalanceRepository.GetStockBalancesAsync(
            warehouseId, locationId, materialId, productId, batchNumber);

        return _mapper.Map<IEnumerable<StockBalanceDto>>(balances);
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetStockMovementsAsync(
        int? warehouseId, int? materialId, int? productId, InventoryTransactionType? transactionType, DateTime? startDate, DateTime? endDate)
    {
        var transactions = await _repositoryManager.InventoryTransactionRepository.GetFilteredTransactionsAsync(
            warehouseId, materialId, productId, transactionType, startDate, endDate);

        return _mapper.Map<IEnumerable<InventoryTransactionDto>>(transactions);
    }

    public async Task<bool> TransferStockAsync(StockTransferRequest request, int userId)
    {
        if (request.SourceWarehouseId == request.DestinationWarehouseId && request.SourceLocationId == request.DestinationLocationId)
        {
            throw new InvalidOperationException("Source and destination location cannot be identical.");
        }

        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("Stock transfer quantity must be greater than zero.");
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Find & validate source stock balance
            var sourceStock = await _repositoryManager.StockBalanceRepository.FindStockAsync(
                request.SourceWarehouseId, request.SourceLocationId, request.MaterialId, request.ProductId, request.BatchNumber);

            if (sourceStock == null || sourceStock.Quantity < request.Quantity)
            {
                throw new InvalidOperationException($"Insufficient source stock available. Required: {request.Quantity}, Available: {sourceStock?.Quantity ?? 0}");
            }

            // 2. Deduct quantity from source stock
            sourceStock.Quantity -= request.Quantity;
            sourceStock.UpdatedAt = DateTime.UtcNow;
            _repositoryManager.StockBalanceRepository.Update(sourceStock);

            // 3. Find or create destination stock balance
            var destStock = await _repositoryManager.StockBalanceRepository.FindStockAsync(
                request.DestinationWarehouseId, request.DestinationLocationId, request.MaterialId, request.ProductId, request.BatchNumber);

            if (destStock == null)
            {
                destStock = new StockBalance
                {
                    WarehouseId = request.DestinationWarehouseId,
                    LocationId = request.DestinationLocationId,
                    MaterialId = request.MaterialId,
                    ProductId = request.ProductId,
                    BatchNumber = request.BatchNumber ?? string.Empty,
                    Quantity = request.Quantity,
                    Unit = request.Unit,
                    ManufacturingDate = sourceStock.ManufacturingDate,
                    ExpiryDate = sourceStock.ExpiryDate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _repositoryManager.StockBalanceRepository.Create(destStock);
            }
            else
            {
                destStock.Quantity += request.Quantity;
                destStock.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.StockBalanceRepository.Update(destStock);
            }

            // 4. Create inventory transaction log
            var transactionLog = new InventoryTransaction
            {
                TransactionType = InventoryTransactionType.StockTransfer,
                TransactionDate = DateTime.UtcNow,
                WarehouseId = request.SourceWarehouseId,
                SourceLocationId = request.SourceLocationId,
                DestinationLocationId = request.DestinationLocationId,
                MaterialId = request.MaterialId,
                ProductId = request.ProductId,
                BatchNumber = request.BatchNumber ?? string.Empty,
                Quantity = request.Quantity,
                Unit = request.Unit,
                ReferenceDocumentNumber = string.IsNullOrWhiteSpace(request.ReferenceNumber) ? $"TRSF-{DateTime.UtcNow:yyyyMMddHHmmss}" : request.ReferenceNumber,
                UserId = userId,
                Notes = $"Stock transfer from Warehouse #{request.SourceWarehouseId} to Warehouse #{request.DestinationWarehouseId}. {request.Notes}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _repositoryManager.InventoryTransactionRepository.Create(transactionLog);

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

    public async Task<bool> AdjustStockAsync(StockAdjustmentRequest request, int userId)
    {
        if (request.ActualQuantity < 0)
        {
            throw new InvalidOperationException("Actual physical count cannot be negative.");
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Find existing stock balance
            var stock = await _repositoryManager.StockBalanceRepository.FindStockAsync(
                request.WarehouseId, request.LocationId, request.MaterialId, request.ProductId, request.BatchNumber);

            decimal oldQuantity = stock?.Quantity ?? 0;
            decimal adjustmentDifference = request.ActualQuantity - oldQuantity;

            if (stock == null)
            {
                stock = new StockBalance
                {
                    WarehouseId = request.WarehouseId,
                    LocationId = request.LocationId,
                    MaterialId = request.MaterialId,
                    ProductId = request.ProductId,
                    BatchNumber = request.BatchNumber ?? string.Empty,
                    Quantity = request.ActualQuantity,
                    Unit = request.Unit,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _repositoryManager.StockBalanceRepository.Create(stock);
            }
            else
            {
                stock.Quantity = request.ActualQuantity;
                stock.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.StockBalanceRepository.Update(stock);
            }

            // Create inventory adjustment transaction record
            var transactionLog = new InventoryTransaction
            {
                TransactionType = InventoryTransactionType.StockAdjustment,
                TransactionDate = DateTime.UtcNow,
                WarehouseId = request.WarehouseId,
                SourceLocationId = request.LocationId,
                MaterialId = request.MaterialId,
                ProductId = request.ProductId,
                BatchNumber = request.BatchNumber ?? string.Empty,
                Quantity = adjustmentDifference,
                Unit = request.Unit,
                ReferenceDocumentNumber = $"ADJ-{DateTime.UtcNow:yyyyMMddHHmmss}",
                UserId = userId,
                Notes = $"Stock Adjustment: Physical count = {request.ActualQuantity}, System count = {oldQuantity} (Diff: {adjustmentDifference:+#.##;-#.##;0}). Reason: {request.Reason}. {request.Notes}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _repositoryManager.InventoryTransactionRepository.Create(transactionLog);

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

    public async Task<IEnumerable<StockBalanceDto>> GetExpiringItemsAsync(int daysUntilExpiry)
    {
        var balances = await _repositoryManager.StockBalanceRepository.GetExpiringStockAsync(daysUntilExpiry);
        return _mapper.Map<IEnumerable<StockBalanceDto>>(balances);
    }

    public async Task<InventoryTransaction> ConsumeStockForProductionAsync(
        int warehouseId,
        int? locationId,
        int materialId,
        string rawMaterialBatchNumber,
        decimal quantity,
        string unit,
        string referenceDoc,
        int userId,
        string? notes = null)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("كمية استهلاك الخامة للتشغيل يجب أن تكون أكبر من الصفر.");
        }

        var material = await _repositoryManager.MaterialRepository.GetByIdAsync(materialId, trackChanges: true);
        if (material == null || !material.IsActive)
        {
            throw new InvalidOperationException($"المادة الخام بالمعرف #{materialId} غير موجودة أو معطلة.");
        }

        var stockBalance = await _repositoryManager.StockBalanceRepository.FindStockAsync(
            warehouseId, locationId, materialId, null, rawMaterialBatchNumber);

        if (stockBalance == null || stockBalance.Quantity < quantity)
        {
            var available = stockBalance?.Quantity ?? 0m;
            throw new InvalidOperationException($"عجز في رصيد المادة الخام '{material.Name}' في المستودع المحدد. المطلوب: {quantity} {unit}، المتاح: {available} {unit}.");
        }

        if (stockBalance.ExpiryDate.HasValue && stockBalance.ExpiryDate.Value.Date < DateTime.UtcNow.Date)
        {
            throw new InvalidOperationException($"دفعة المادة الخام '{rawMaterialBatchNumber}' منتهية الصلاحية بتاريخ ({stockBalance.ExpiryDate.Value:yyyy-MM-dd}). لا يمكن استخدام خامات منتهية الصلاحية.");
        }

        // Deduct from stock balance
        stockBalance.Quantity -= quantity;
        stockBalance.UpdatedAt = DateTime.UtcNow;
        _repositoryManager.StockBalanceRepository.Update(stockBalance);

        // Update total material current stock
        material.CurrentStock = Math.Max(0m, material.CurrentStock - quantity);
        material.UpdatedAt = DateTime.UtcNow;
        _repositoryManager.MaterialRepository.Update(material);

        var unitCost = material.StandardCost;
        var totalCost = Math.Round(unitCost * quantity, 2);

        var transactionLog = new InventoryTransaction
        {
            TransactionType = InventoryTransactionType.ProductionConsumption,
            TransactionDate = DateTime.UtcNow,
            WarehouseId = warehouseId,
            SourceLocationId = locationId,
            MaterialId = materialId,
            BatchNumber = rawMaterialBatchNumber ?? string.Empty,
            Quantity = quantity,
            Unit = string.IsNullOrWhiteSpace(unit) ? material.Unit : unit,
            UnitCost = unitCost,
            TotalCost = totalCost,
            ReferenceDocumentNumber = referenceDoc,
            UserId = userId,
            Notes = notes ?? $"صرف خامات لأمر الإنتاج/الدفعة {referenceDoc}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryManager.InventoryTransactionRepository.Create(transactionLog);
        await _repositoryManager.SaveAsync();

        return transactionLog;
    }

    public async Task<InventoryTransaction> ConsumeStockForWasteAsync(
        int warehouseId,
        int? locationId,
        int materialId,
        string? rawMaterialBatchNumber,
        decimal quantity,
        string unit,
        string referenceWasteNumber,
        int userId,
        string? notes = null)
    {
        if (quantity <= 0)
        {
            throw new InvalidOperationException("كمية هالك الخامة يجب أن تكون أكبر من الصفر.");
        }

        var material = await _repositoryManager.MaterialRepository.GetByIdAsync(materialId, trackChanges: true);
        if (material == null || !material.IsActive)
        {
            throw new InvalidOperationException($"المادة الخام بالمعرف #{materialId} غير موجودة أو معطلة.");
        }

        var stockBalance = await _repositoryManager.StockBalanceRepository.FindStockAsync(
            warehouseId, locationId, materialId, null, rawMaterialBatchNumber);

        if (stockBalance == null || stockBalance.Quantity < quantity)
        {
            var available = stockBalance?.Quantity ?? 0m;
            throw new InvalidOperationException($"عجز في رصيد المادة الخام '{material.Name}' في المستودع المحدد لإسقاط الهالك. المطلوب: {quantity} {unit}، المتاح: {available} {unit}.");
        }

        // Deduct from stock balance
        stockBalance.Quantity -= quantity;
        stockBalance.UpdatedAt = DateTime.UtcNow;
        _repositoryManager.StockBalanceRepository.Update(stockBalance);

        // Update total material current stock
        material.CurrentStock = Math.Max(0m, material.CurrentStock - quantity);
        material.UpdatedAt = DateTime.UtcNow;
        _repositoryManager.MaterialRepository.Update(material);

        var unitCost = material.CurrentCost > 0 ? material.CurrentCost : material.StandardCost;
        var totalCost = Math.Round(unitCost * quantity, 2);

        var transactionLog = new InventoryTransaction
        {
            TransactionType = InventoryTransactionType.Waste,
            TransactionDate = DateTime.UtcNow,
            WarehouseId = warehouseId,
            SourceLocationId = locationId,
            MaterialId = materialId,
            BatchNumber = rawMaterialBatchNumber ?? string.Empty,
            Quantity = quantity,
            Unit = string.IsNullOrWhiteSpace(unit) ? material.Unit : unit,
            UnitCost = unitCost,
            TotalCost = totalCost,
            ReferenceDocumentNumber = referenceWasteNumber,
            UserId = userId,
            Notes = notes ?? $"إسقاط هالك مواد خام معتمد برقم {referenceWasteNumber}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryManager.InventoryTransactionRepository.Create(transactionLog);
        await _repositoryManager.SaveAsync();

        return transactionLog;
    }

    public async Task<List<InventoryTransaction>> ConsumeStockForPackagingBatchAsync(
        int packagingOrderId,
        string packagingOrderNumber,
        IEnumerable<PackagingConsumptionItemRequest> items,
        int userId)
    {
        if (items == null || !items.Any())
        {
            return new List<InventoryTransaction>();
        }

        var results = new List<InventoryTransaction>();
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var itemReq in items)
            {
                if (itemReq.Quantity <= 0) continue;

                var material = await _repositoryManager.MaterialRepository.GetByIdAsync(itemReq.MaterialId, trackChanges: true);
                if (material == null || !material.IsActive)
                {
                    throw new InvalidOperationException($"مادة التعبئة بالمعرف #{itemReq.MaterialId} غير موجودة أو معطلة.");
                }

                var stockBalances = await _context.StockBalances
                    .Where(sb => sb.WarehouseId == itemReq.WarehouseId &&
                                 sb.MaterialId == itemReq.MaterialId &&
                                 (!itemReq.LocationId.HasValue || sb.LocationId == itemReq.LocationId.Value) &&
                                 (string.IsNullOrEmpty(itemReq.BatchNumber) || sb.BatchNumber == itemReq.BatchNumber))
                    .OrderBy(sb => sb.ExpiryDate ?? DateTime.MaxValue)
                    .ThenBy(sb => sb.CreatedAt)
                    .ToListAsync();

                // Check for expired stock
                var now = DateTime.UtcNow.Date;
                var validStock = stockBalances.Where(sb => !sb.ExpiryDate.HasValue || sb.ExpiryDate.Value.Date >= now).ToList();
                var availableQuantity = validStock.Sum(sb => sb.Quantity);

                if (availableQuantity < itemReq.Quantity)
                {
                    throw new InvalidOperationException($"عجز في رصيد مادة التعبئة '{material.Name}'. المطلوب: {itemReq.Quantity} {itemReq.Unit}، المتاح الصالح: {availableQuantity} {itemReq.Unit}.");
                }

                decimal remainingToDeduct = itemReq.Quantity;
                foreach (var sb in validStock)
                {
                    if (remainingToDeduct <= 0) break;

                    decimal deductFromThis = Math.Min(sb.Quantity, remainingToDeduct);
                    sb.Quantity -= deductFromThis;
                    sb.UpdatedAt = DateTime.UtcNow;
                    _repositoryManager.StockBalanceRepository.Update(sb);

                    remainingToDeduct -= deductFromThis;
                }

                // Update material total stock
                material.CurrentStock = Math.Max(0m, material.CurrentStock - itemReq.Quantity);
                material.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.MaterialRepository.Update(material);

                var unitCost = material.CurrentCost > 0 ? material.CurrentCost : (material.StandardCost > 0 ? material.StandardCost : material.UnitCost);
                var totalCost = Math.Round(unitCost * itemReq.Quantity, 4);

                var transactionLog = new InventoryTransaction
                {
                    TransactionType = InventoryTransactionType.PackagingConsumption,
                    TransactionDate = DateTime.UtcNow,
                    WarehouseId = itemReq.WarehouseId,
                    SourceLocationId = itemReq.LocationId,
                    MaterialId = itemReq.MaterialId,
                    BatchNumber = itemReq.BatchNumber ?? string.Empty,
                    Quantity = itemReq.Quantity,
                    Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? material.Unit : itemReq.Unit,
                    UnitCost = unitCost,
                    TotalCost = totalCost,
                    ReferenceDocumentNumber = packagingOrderNumber,
                    UserId = userId,
                    Notes = itemReq.Notes ?? $"صرف مواد تعبئة وتغليف لأمر التعبئة {packagingOrderNumber}",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _repositoryManager.InventoryTransactionRepository.Create(transactionLog);
                results.Add(transactionLog);
            }

            await _repositoryManager.SaveAsync();
            await dbTransaction.CommitAsync();

            return results;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }
}


