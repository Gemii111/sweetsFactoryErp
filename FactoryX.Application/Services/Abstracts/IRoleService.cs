using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IRoleService
{
    Task<List<RoleItemDto>> GetAllRolesAsync();
    Task<RoleItemDto?> GetRoleByIdAsync(int id);
    Task<int> CreateRoleAsync(CreateRoleRequestDto request, string createdByUsername);
    Task UpdateRoleAsync(EditRoleRequestDto request, string updatedByUsername);
    Task<bool> ToggleRoleStatusAsync(int roleId, bool isActive, string modifiedByUsername);
}
