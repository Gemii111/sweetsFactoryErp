using FactoryX.Application.DTOs;
using FactoryX.Application.DTOs.Requests.AuthenticationRequests;
using FactoryX.Application.DTOs.Requests.UserManagementRequests;
using FactoryX.Application.DTOs.Responses.AuthenticationResponses;
using FactoryX.Application.DTOs.Responses.UserManagementResponses;

namespace FactoryX.Application.Services.Abstracts;

public interface IUserService
{
    Task<LoginResponse?> AuthenticateAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<UserDto?> GetByIdAsync(int id);
    Task<UserDto?> GetByUsernameAsync(string username);
    Task<GetUserProfileResponse?> GetProfileAsync(int userId);
    Task UpdateProfileAsync(UserProfileDto dto);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
}
