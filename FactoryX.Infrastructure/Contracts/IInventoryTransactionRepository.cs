using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IInventoryTransactionRepository : IBaseRepository<InventoryTransaction>
{
    Task<IEnumerable<InventoryTransaction>> GetFilteredTransactionsAsync(
        int? warehouseId, int? materialId, int? productId, InventoryTransactionType? transactionType, DateTime? startDate, DateTime? endDate);
}
