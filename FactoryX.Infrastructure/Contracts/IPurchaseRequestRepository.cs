using FactoryX.Domain.Entities;
using FactoryX.Domain.Interfaces;

namespace FactoryX.Infrastructure.Contracts;

public interface IPurchaseRequestRepository : IBaseRepository<PurchaseRequest>
{
    Task<IEnumerable<PurchaseRequest>> GetAllRequestsAsync(
        PurchaseRequestStatus? status = null,
        int? departmentId = null,
        int? requestedById = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<PurchaseRequest?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<PurchaseRequest?> GetByNumberAsync(string requestNumber, bool trackChanges = false);
    Task<int> GetCountForDateAsync(DateTime date);
    Task<bool> IsRequestNumberUniqueAsync(string requestNumber, int? excludeId = null);
}
