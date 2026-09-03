using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IProductCategoryRepository : IBaseRepository<ProductCategory>
{
    Task<IEnumerable<ProductCategory>> GetAllWithProductsAsync(bool trackChanges = false);
    Task<IEnumerable<ProductCategory>> GetActiveCategoriesAsync();
    Task<ProductCategory?> GetCategoryWithProductsAsync(int id);
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    Task<bool> IsNameUniqueAsync(string name, int? excludeId = null);
}
