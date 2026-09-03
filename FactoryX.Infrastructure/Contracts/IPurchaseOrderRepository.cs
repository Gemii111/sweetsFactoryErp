using FactoryX.Domain.Entities;
using FactoryX.Domain.Interfaces;

namespace FactoryX.Infrastructure.Contracts;

public interface IPurchaseOrderRepository : IBaseRepository<PurchaseOrder>
{
    Task<IEnumerable<PurchaseOrder>> GetAllOrdersAsync(
        PurchaseOrderStatus? status = null,
        int? supplierId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<PurchaseOrder?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, bool trackChanges = false);
    Task<int> GetCountForDateAsync(DateTime date);
    Task<bool> IsOrderNumberUniqueAsync(string orderNumber, int? excludeId = null);
    Task<IEnumerable<PurchaseOrder>> GetReleasableOrdersForSupplierAsync(int supplierId);
}
