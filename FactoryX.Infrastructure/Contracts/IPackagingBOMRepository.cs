using FactoryX.Domain.Entities;
using FactoryX.Domain.Interfaces;

namespace FactoryX.Infrastructure.Contracts;

public interface IPackagingBOMRepository : IBaseRepository<PackagingBOM>
{
    Task<IEnumerable<PackagingBOM>> GetAllWithDetailsAsync(bool onlyActive = false, int? productId = null);
    Task<PackagingBOM?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<PackagingBOM?> GetByCodeAsync(string code, bool trackChanges = false);
    Task<PackagingBOMVersion?> GetActiveVersionForBOMAsync(int packagingBomId, DateTime? date = null);
    Task<PackagingBOMVersion?> GetVersionWithItemsAsync(int versionId, bool trackChanges = false);
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    Task<bool> HasOverlappingActiveVersionAsync(int packagingBomId, DateTime from, DateTime? to, int? excludeVersionId = null);
}
