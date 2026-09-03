using FactoryX.Domain.Entities;
using FactoryX.Domain.Interfaces;

namespace FactoryX.Infrastructure.Contracts;

public interface ISupplierRepository : IBaseRepository<Supplier>
{
    Task<IEnumerable<Supplier>> GetAllSuppliersAsync(
        string? searchTerm = null,
        int? categoryId = null,
        bool? isActive = null);

    Task<Supplier?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<Supplier?> GetByCodeAsync(string code, bool trackChanges = false);
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    Task<bool> HasPurchasingHistoryAsync(int id);
    Task<int> GetCountAsync();
}
