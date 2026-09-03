using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IPackagingBOMService
{
    Task<IEnumerable<PackagingBOMDto>> GetAllBOMsAsync(bool onlyActive = false, int? productId = null);
    Task<PackagingBOMDto> GetBOMByIdAsync(int id);
    Task<PackagingBOMDto> CreateBOMAsync(CreatePackagingBOMRequest request, int? userId = null);
    Task<PackagingBOMDto> UpdateBOMAsync(UpdatePackagingBOMRequest request, int? userId = null);
    Task<bool> DeleteBOMAsync(int id);
    Task<PackagingBOMVersionDto> CreateVersionAsync(CreatePackagingBOMVersionRequest request, int? userId = null);
    Task<PackagingBOMVersionDto> ActivateVersionAsync(int versionId, int? userId = null);
    Task<PackagingBOMVersionDto> DeactivateVersionAsync(int versionId, int? userId = null);
    Task<IEnumerable<MaterialDto>> GetAvailablePackagingMaterialsAsync();
}
