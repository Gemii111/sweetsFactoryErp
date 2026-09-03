using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface ISalesOrderRepository : IBaseRepository<SalesOrder>
{
    Task<IEnumerable<SalesOrder>> GetAllOrdersAsync(
        SalesOrderStatus? status = null,
        int? customerId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool trackChanges = false);

    Task<SalesOrder?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<SalesOrder?> GetByOrderNumberAsync(string orderNumber, bool trackChanges = false);
    Task<bool> IsOrderNumberUniqueAsync(string orderNumber, int? excludeId = null);
    Task<int> GetCountForDateAsync(DateTime date);
}
