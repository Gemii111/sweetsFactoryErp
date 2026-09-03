using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IFinishedGoodsService
{
    Task<IEnumerable<FinishedGoodsStockDto>> GetStockAsync(
        int? warehouseId = null,
        int? locationId = null,
        int? productId = null,
        string? batchNumber = null);

    Task<FinishedGoodsStockDto?> GetStockByIdAsync(int id);
    Task<FinishedGoodsStockSummaryDto> GetStockSummaryAsync();

    Task<IEnumerable<FinishedGoodsMovementDto>> GetStockMovementsAsync(
        int? warehouseId = null,
        int? productId = null,
        string? batchNumber = null,
        InventoryTransactionType? transactionType = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    Task<bool> AdjustStockAsync(FinishedGoodsAdjustmentRequest request, int userId);
    Task<bool> TransferStockAsync(FinishedGoodsTransferRequest request, int userId);
}
