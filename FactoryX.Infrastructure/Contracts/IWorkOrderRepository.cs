using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IWorkOrderRepository : IBaseRepository<WorkOrder>
{
    Task<IEnumerable<WorkOrder>> GetFilteredOrdersAsync(
        string? search = null,
        int? productId = null,
        ProductionOrderStatus? status = null,
        ProductionOrderPriority? priority = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool trackChanges = false);

    Task<WorkOrder?> GetOrderWithDetailsAsync(int id, bool trackChanges = false);
    Task<WorkOrder?> GetOrderWithRequirementsAsync(int id, bool trackChanges = false);
    Task<bool> IsOrderNumberUniqueAsync(string orderNumber, int? excludeId = null);
    Task<bool> HasActiveOrdersForRecipeVersionAsync(int recipeVersionId);
    Task<bool> HasActiveOrdersForProductAsync(int productId);
}
