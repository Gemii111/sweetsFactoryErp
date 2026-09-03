using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IInventoryService
{
    Task<IEnumerable<StockBalanceDto>> GetStockAsync(
        int? warehouseId, int? locationId, int? materialId, int? productId, string? batchNumber);

    Task<IEnumerable<InventoryTransactionDto>> GetStockMovementsAsync(
        int? warehouseId, int? materialId, int? productId, InventoryTransactionType? transactionType, DateTime? startDate, DateTime? endDate);

    Task<bool> TransferStockAsync(StockTransferRequest request, int userId);

    Task<bool> AdjustStockAsync(StockAdjustmentRequest request, int userId);

    Task<IEnumerable<StockBalanceDto>> GetExpiringItemsAsync(int daysUntilExpiry);

    Task<InventoryTransaction> ConsumeStockForProductionAsync(
        int warehouseId,
        int? locationId,
        int materialId,
        string rawMaterialBatchNumber,
        decimal quantity,
        string unit,
        string referenceDoc,
        int userId,
        string? notes = null);

    Task<InventoryTransaction> ConsumeStockForWasteAsync(
        int warehouseId,
        int? locationId,
        int materialId,
        string? rawMaterialBatchNumber,
        decimal quantity,
        string unit,
        string referenceWasteNumber,
        int userId,
        string? notes = null);

    Task<List<InventoryTransaction>> ConsumeStockForPackagingBatchAsync(
        int packagingOrderId,
        string packagingOrderNumber,
        IEnumerable<PackagingConsumptionItemRequest> items,
        int userId);
}
