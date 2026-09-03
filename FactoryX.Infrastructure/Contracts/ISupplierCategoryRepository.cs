using FactoryX.Domain.Entities;
using FactoryX.Domain.Interfaces;

namespace FactoryX.Infrastructure.Contracts;

public interface ISupplierCategoryRepository : IBaseRepository<SupplierCategory>
{
    Task<IEnumerable<SupplierCategory>> GetAllCategoriesAsync(bool onlyActive = false);
    Task<SupplierCategory?> GetByCodeAsync(string code, bool trackChanges = false);
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
}
