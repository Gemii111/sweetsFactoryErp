using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(int userId, string permissionCode);
    Task<List<string>> GetUserPermissionCodesAsync(int userId);
    Task<List<PermissionItemDto>> GetAllPermissionsAsync();
    Task<RolePermissionMatrixDto> GetRolePermissionMatrixAsync(int roleId);
    Task UpdateRolePermissionsAsync(int roleId, List<int> permissionIds);
    Task SeedDefaultPermissionsAndRolesAsync();
}
