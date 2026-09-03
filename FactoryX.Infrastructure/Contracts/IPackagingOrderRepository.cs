using FactoryX.Domain.Entities;
using FactoryX.Domain.Interfaces;

namespace FactoryX.Infrastructure.Contracts;

public interface IPackagingOrderRepository : IBaseRepository<PackagingOrder>
{
    Task<IEnumerable<PackagingOrder>> GetAllWithDetailsAsync(
        PackagingOrderStatus? status = null,
        int? batchId = null,
        int? productId = null,
        int? bomId = null,
        int? operatorId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<PackagingOrder?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<PackagingOrder?> GetByOrderNumberAsync(string orderNumber, bool trackChanges = false);
    Task<int> GetCountForDateAsync(DateTime date);
    Task<bool> IsOrderNumberUniqueAsync(string orderNumber, int? excludeId = null);
    Task<IEnumerable<PackagingOrder>> GetOrdersForBatchAsync(int batchId);
}
