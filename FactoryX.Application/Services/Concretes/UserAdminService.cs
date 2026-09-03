using FactoryX.Application.DTOs;
using FactoryX.Application.Helpers;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class UserAdminService : IUserAdminService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;

    public UserAdminService(AppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<UserAdminItemDto>> GetAllUsersAsync()
    {
        var users = await _context.Users.AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserWarehouses).ThenInclude(uw => uw.Warehouse)
            .OrderBy(u => u.Username)
            .ToListAsync();

        return users.Select(u => new UserAdminItemDto
        {
            Id = u.Id,
            Username = u.Username,
            FullName = u.FullName ?? u.Username,
            Email = u.Email ?? "",
            Role = u.Role,
            AssignedRoles = u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.DisplayName ?? ur.Role!.Name).ToList(),
            IsActive = u.IsActive,
            IsLocked = u.LockedUntil.HasValue && u.LockedUntil.Value > DateTime.UtcNow,
            LockedUntil = u.LockedUntil,
            FailedLoginCount = u.FailedLoginCount,
            LastLoginAt = u.LastLoginAt,
            CreatedAt = u.CreatedAt,
            IsAllWarehousesAllowed = u.IsAllWarehousesAllowed,
            AllowedWarehouseNames = u.UserWarehouses.Where(uw => uw.Warehouse != null).Select(uw => uw.Warehouse!.Name).ToList()
        }).ToList();
    }

    public async Task<UserAdminItemDto?> GetUserByIdAsync(int id)
    {
        var u = await _context.Users.AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserWarehouses).ThenInclude(uw => uw.Warehouse)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (u == null) return null;

        return new UserAdminItemDto
        {
            Id = u.Id,
            Username = u.Username,
            FullName = u.FullName ?? u.Username,
            Email = u.Email ?? "",
            Role = u.Role,
            AssignedRoles = u.UserRoles.Where(ur => ur.Role != null).Select(ur => ur.Role!.DisplayName ?? ur.Role!.Name).ToList(),
            IsActive = u.IsActive,
            IsLocked = u.LockedUntil.HasValue && u.LockedUntil.Value > DateTime.UtcNow,
            LockedUntil = u.LockedUntil,
            FailedLoginCount = u.FailedLoginCount,
            LastLoginAt = u.LastLoginAt,
            CreatedAt = u.CreatedAt,
            IsAllWarehousesAllowed = u.IsAllWarehousesAllowed,
            AllowedWarehouseNames = u.UserWarehouses.Where(uw => uw.Warehouse != null).Select(uw => uw.Warehouse!.Name).ToList()
        };
    }

    public async Task<int> CreateUserAsync(CreateUserRequestDto request, string createdByUsername)
    {
        if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            throw new InvalidOperationException($"اسم المستخدم '{request.Username}' مستخدم بالفعل.");

        var user = new User
        {
            Username = request.Username.Trim(),
            FullName = request.FullName?.Trim() ?? request.Username,
            Email = request.Email?.Trim(),
            PasswordHash = PasswordHasher.HashPassword(request.Password),
            Role = !string.IsNullOrWhiteSpace(request.PrimaryRole) ? request.PrimaryRole : "User",
            IsActive = true,
            IsAllWarehousesAllowed = request.IsAllWarehousesAllowed,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Assign Roles
        if (request.RoleIds != null && request.RoleIds.Any())
        {
            var validRoleIds = await _context.Roles.Where(r => request.RoleIds.Contains(r.Id)).Select(r => r.Id).ToListAsync();
            foreach (var rid in validRoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = rid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Assign Warehouses
        if (!request.IsAllWarehousesAllowed && request.AllowedWarehouseIds != null && request.AllowedWarehouseIds.Any())
        {
            var validWhIds = await _context.Warehouses.Where(w => request.AllowedWarehouseIds.Contains(w.Id)).Select(w => w.Id).ToListAsync();
            foreach (var wid in validWhIds)
            {
                _context.UserWarehouses.Add(new UserWarehouse
                {
                    UserId = user.Id,
                    WarehouseId = wid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        await _auditService.LogActivityAsync(
            null,
            createdByUsername,
            "Create",
            "Security",
            "User",
            user.Id.ToString(),
            user.Username,
            $"إنشاء حساب مستخدم جديد: {user.Username} ({user.FullName})"
        );

        await _auditService.LogSecurityEventAsync(
            "UserCreated",
            user.Id,
            user.Username,
            $"تم إنشاء المستخدم بواسطة {createdByUsername}",
            severity: "Info"
        );

        return user.Id;
    }

    public async Task UpdateUserAsync(EditUserRequestDto request, string updatedByUsername)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserWarehouses)
            .FirstOrDefaultAsync(u => u.Id == request.Id);

        if (user == null) throw new InvalidOperationException("المستخدم غير موجود.");

        // Check last super admin protection
        if (!request.IsActive && user.IsActive)
        {
            await ValidateNotLastSuperAdminAsync(user.Id, "تعطيل");
        }

        user.FullName = request.FullName?.Trim() ?? user.Username;
        user.Email = request.Email?.Trim();
        user.IsActive = request.IsActive;
        user.IsAllWarehousesAllowed = request.IsAllWarehousesAllowed;
        if (!string.IsNullOrWhiteSpace(request.PrimaryRole)) user.Role = request.PrimaryRole;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            user.PasswordHash = PasswordHasher.HashPassword(request.NewPassword);
            await _auditService.LogSecurityEventAsync("PasswordReset", user.Id, user.Username, $"تم إعادة تعيين كلمة المرور بواسطة {updatedByUsername}", severity: "Warning");
        }

        // Update Roles
        _context.UserRoles.RemoveRange(user.UserRoles);
        if (request.RoleIds != null && request.RoleIds.Any())
        {
            var validRoleIds = await _context.Roles.Where(r => request.RoleIds.Contains(r.Id)).Select(r => r.Id).ToListAsync();
            foreach (var rid in validRoleIds)
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.Id,
                    RoleId = rid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        // Update Warehouses
        _context.UserWarehouses.RemoveRange(user.UserWarehouses);
        if (!request.IsAllWarehousesAllowed && request.AllowedWarehouseIds != null && request.AllowedWarehouseIds.Any())
        {
            var validWhIds = await _context.Warehouses.Where(w => request.AllowedWarehouseIds.Contains(w.Id)).Select(w => w.Id).ToListAsync();
            foreach (var wid in validWhIds)
            {
                _context.UserWarehouses.Add(new UserWarehouse
                {
                    UserId = user.Id,
                    WarehouseId = wid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogActivityAsync(
            null,
            updatedByUsername,
            "Update",
            "Security",
            "User",
            user.Id.ToString(),
            user.Username,
            $"تعديل بيانات المستخدم: {user.Username}"
        );
    }

    public async Task<bool> ToggleUserStatusAsync(int userId, bool isActive, string modifiedByUsername)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        if (!isActive)
        {
            await ValidateNotLastSuperAdminAsync(userId, "تعطيل");
        }

        user.IsActive = isActive;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(
            isActive ? "UserEnabled" : "UserDisabled",
            user.Id,
            user.Username,
            $"تم {(isActive ? "تفعيل" : "تعطيل")} حساب المستخدم بواسطة {modifiedByUsername}",
            severity: isActive ? "Info" : "Warning"
        );

        return true;
    }

    public async Task<bool> UnlockUserAsync(int userId, string modifiedByUsername)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(
            "AccountUnlocked",
            user.Id,
            user.Username,
            $"تم إلغاء قفل الحساب يدوياً بواسطة {modifiedByUsername}",
            severity: "Info"
        );

        return true;
    }

    public async Task<bool> ResetPasswordAsync(int userId, string newPassword, string modifiedByUsername)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null) return false;

        user.PasswordHash = PasswordHasher.HashPassword(newPassword);
        user.FailedLoginCount = 0;
        user.LockedUntil = null;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(
            "PasswordReset",
            user.Id,
            user.Username,
            $"تم تغيير كلمة المرور بواسطة {modifiedByUsername}",
            severity: "Warning"
        );

        return true;
    }

    public async Task DeleteUserSafeAsync(int userId, string modifiedByUsername)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .Include(u => u.UserWarehouses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return;

        await ValidateNotLastSuperAdminAsync(userId, "حذف");

        // Safe soft-deactivate instead of physical row purge to preserve audit & foreign keys
        user.IsActive = false;
        user.Username = $"{user.Username}_deleted_{DateTime.UtcNow.Ticks}";
        user.UpdatedAt = DateTime.UtcNow;

        _context.UserRoles.RemoveRange(user.UserRoles);
        _context.UserWarehouses.RemoveRange(user.UserWarehouses);
        await _context.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(
            "UserDeleted",
            userId,
            user.Username,
            $"تم حذف/تعطيل الحساب نهائياً بواسطة {modifiedByUsername}",
            severity: "Critical"
        );
    }

    private async Task ValidateNotLastSuperAdminAsync(int targetUserId, string actionName)
    {
        var superAdminRole = await _context.Roles.FirstOrDefaultAsync(r => r.Code == "SUPER_ADMIN");
        if (superAdminRole == null) return;

        var activeSuperAdmins = await _context.UserRoles
            .Include(ur => ur.User)
            .Where(ur => ur.RoleId == superAdminRole.Id && ur.User != null && ur.User.IsActive)
            .Select(ur => ur.UserId)
            .ToListAsync();

        var isTargetSuperAdmin = activeSuperAdmins.Contains(targetUserId);

        if (isTargetSuperAdmin && activeSuperAdmins.Count <= 1)
        {
            throw new InvalidOperationException($"لا يمكن {actionName} حساب مدير النظام الأعلى الوحيد النشط في النظام.");
        }
    }
}
