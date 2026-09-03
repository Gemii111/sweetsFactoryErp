using FactoryX.Application.DTOs;
using FactoryX.Application.DTOs.Requests.AuthenticationRequests;
using FactoryX.Application.DTOs.Requests.UserManagementRequests;
using FactoryX.Application.DTOs.Responses.AuthenticationResponses;
using FactoryX.Application.DTOs.Responses.UserManagementResponses;
using FactoryX.Application.Helpers;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;

    public UserService(AppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<LoginResponse?> AuthenticateAsync(LoginRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user == null)
        {
            await _auditService.LogSecurityEventAsync("LoginFailure", null, request.Username, "محاولة تسجيل دخول لاسم مستخدم غير موجود", severity: "Warning");
            return null;
        }

        if (!user.IsActive)
        {
            await _auditService.LogSecurityEventAsync("LoginFailure", user.Id, user.Username, "محاولة تسجيل دخول لحساب مستخدم معطل", severity: "Warning");
            return null;
        }

        if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
        {
            await _auditService.LogSecurityEventAsync("LoginFailure", user.Id, user.Username, $"محاولة تسجيل دخول لحساب مقفل مؤقتاً حتى {user.LockedUntil.Value:HH:mm:ss}", severity: "Warning");
            return null;
        }

        bool passwordMatches = false;
        try
        {
            passwordMatches = PasswordHasher.VerifyPassword(request.Password, user.PasswordHash);
        }
        catch
        {
            if (user.PasswordHash == request.Password)
            {
                passwordMatches = true;
                user.PasswordHash = PasswordHasher.HashPassword(request.Password);
            }
        }

        if (!passwordMatches && user.PasswordHash == request.Password)
        {
            passwordMatches = true;
            user.PasswordHash = PasswordHasher.HashPassword(request.Password);
        }

        if (!passwordMatches)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
            {
                user.LockedUntil = DateTime.UtcNow.AddMinutes(15);
                await _auditService.LogSecurityEventAsync("AccountLocked", user.Id, user.Username, "تم قفل الحساب مؤقتاً لمدة 15 دقيقة لتكرار المحاولات الخاطئة", severity: "Critical");
            }
            else
            {
                await _auditService.LogSecurityEventAsync("LoginFailure", user.Id, user.Username, $"كلمة المرور غير صحيحة (المحاولة {user.FailedLoginCount} من 5)", severity: "Warning");
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return null;
        }

        // Login success
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync("LoginSuccess", user.Id, user.Username, "تسجيل دخول ناجح للنظام", severity: "Info");

        return new LoginResponse(
            Id: user.Id,
            Username: user.Username,
            Role: user.Role
        );
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            throw new InvalidOperationException("Username already exists.");

        var user = new User
        {
            Username = request.Username.Trim(),
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            Role = request.Role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new RegisterResponse(
            Id: user.Id,
            Username: user.Username,
            Role: user.Role
        );
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        return user == null ? null : ToDto(user);
    }

    public async Task<UserDto?> GetByUsernameAsync(string username)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == username);
        return user == null ? null : ToDto(user);
    }

    public async Task<GetUserProfileResponse?> GetProfileAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return null;

        return new GetUserProfileResponse(
            Id: user.Id,
            UserName: user.Username,
            FullName: user.FullName,
            Email: user.Email
        );
    }

    public async Task UpdateProfileAsync(UserProfileDto dto)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.Id);
        if (user == null) throw new InvalidOperationException("المستخدم غير موجود.");

        user.Username = dto.UserName;
        user.FullName = dto.FullName;
        user.Email = dto.Email;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        if (!PasswordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync("PasswordChanged", user.Id, user.Username, "تم تغيير كلمة المرور للمستخدم", severity: "Info");

        return true;
    }

    private static UserDto ToDto(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        Role = user.Role
    };
}
