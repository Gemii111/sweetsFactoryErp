using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IUserAdminService
{
    Task<List<UserAdminItemDto>> GetAllUsersAsync();
    Task<UserAdminItemDto?> GetUserByIdAsync(int id);
    Task<int> CreateUserAsync(CreateUserRequestDto request, string createdByUsername);
    Task UpdateUserAsync(EditUserRequestDto request, string updatedByUsername);
    Task<bool> ToggleUserStatusAsync(int userId, bool isActive, string modifiedByUsername);
    Task<bool> UnlockUserAsync(int userId, string modifiedByUsername);
    Task<bool> ResetPasswordAsync(int userId, string newPassword, string modifiedByUsername);
    Task DeleteUserSafeAsync(int userId, string modifiedByUsername);
}
