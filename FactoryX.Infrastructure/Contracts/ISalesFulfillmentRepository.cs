using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface ISalesFulfillmentRepository : IBaseRepository<SalesFulfillment>
{
    Task<IEnumerable<SalesFulfillment>> GetAllFulfillmentsAsync(
        SalesFulfillmentStatus? status = null,
        int? salesOrderId = null,
        int? customerId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool trackChanges = false);

    Task<SalesFulfillment?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<SalesFulfillment?> GetByFulfillmentNumberAsync(string fulfillmentNumber, bool trackChanges = false);
    Task<bool> IsFulfillmentNumberUniqueAsync(string fulfillmentNumber, int? excludeId = null);
    Task<int> GetCountForDateAsync(DateTime date);
    Task<decimal> GetTotalFulfilledQuantityForOrderItemAsync(int salesOrderItemId);
}
