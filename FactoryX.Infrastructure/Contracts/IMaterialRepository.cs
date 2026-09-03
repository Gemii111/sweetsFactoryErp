using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IMaterialRepository : IBaseRepository<Material>
{
    Task<IEnumerable<Material>> GetAllWithDetailsAsync(bool trackChanges = false);
    Task<Material?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId = null);
    Task<bool> ExistsBySKUAsync(string sku, int? excludeId = null);
    Task<IEnumerable<Material>> GetLowStockMaterialsAsync(bool trackChanges = false);
    Task<IEnumerable<Material>> GetMaterialsBelowReorderLevelAsync(bool trackChanges = false);
    Task<IEnumerable<Material>> GetExpiredMaterialsAsync(bool trackChanges = false);
    Task<IEnumerable<Material>> GetMaterialsExpiringSoonAsync(int days, bool trackChanges = false);
}
