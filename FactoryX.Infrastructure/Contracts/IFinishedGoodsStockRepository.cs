using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IFinishedGoodsStockRepository : IBaseRepository<FinishedGoodsStock>
{
    Task<IEnumerable<FinishedGoodsStock>> GetStockBalancesAsync(
        int? warehouseId = null,
        int? locationId = null,
        int? productId = null,
        string? batchNumber = null,
        bool trackChanges = false);

    Task<FinishedGoodsStock?> FindStockAsync(
        int warehouseId,
        int? locationId,
        int productId,
        string batchNumber,
        bool trackChanges = false);

    Task<FinishedGoodsStock?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<IEnumerable<FinishedGoodsStock>> GetStockForBatchAsync(int batchId);
    Task<decimal> GetTotalStockQuantityAsync(int? productId = null);
    Task<decimal> GetTotalStockValueAsync(int? productId = null);
    Task<IEnumerable<FinishedGoodsStock>> GetExpiringStockAsync(int days);
}
