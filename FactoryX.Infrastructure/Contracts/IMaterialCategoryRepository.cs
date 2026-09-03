using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IMaterialCategoryRepository : IBaseRepository<MaterialCategory>
{
    Task<IEnumerable<MaterialCategory>> GetAllWithMaterialsAsync(bool trackChanges = false);
    Task<MaterialCategory?> GetByIdWithMaterialsAsync(int id, bool trackChanges = false);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
}
