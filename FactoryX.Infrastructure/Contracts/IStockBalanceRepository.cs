using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IStockBalanceRepository : IBaseRepository<StockBalance>
{
    Task<StockBalance?> FindStockAsync(int warehouseId, int? locationId, int? materialId, int? productId, string batchNumber);
    Task<IEnumerable<StockBalance>> GetStockBalancesAsync(int? warehouseId, int? locationId, int? materialId, int? productId, string? batchNumber);
    Task<IEnumerable<StockBalance>> GetExpiringStockAsync(int daysUntilExpiry);
}
