using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class RoleService : IRoleService
{
    private readonly AppDbContext _context;
    private readonly IAuditService _auditService;

    public RoleService(AppDbContext context, IAuditService auditService)
    {
        _context = context;
        _auditService = auditService;
    }

    public async Task<List<RoleItemDto>> GetAllRolesAsync()
    {
        var roles = await _context.Roles.AsNoTracking()
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .OrderBy(r => r.Id)
            .ToListAsync();

        return roles.Select(r => new RoleItemDto
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            DisplayName = string.IsNullOrWhiteSpace(r.DisplayName) ? r.Name : r.DisplayName,
            Description = r.Description,
            IsActive = r.IsActive,
            UsersCount = r.UserRoles.Count,
            PermissionsCount = r.RolePermissions.Count
        }).ToList();
    }

    public async Task<RoleItemDto?> GetRoleByIdAsync(int id)
    {
        var r = await _context.Roles.AsNoTracking()
            .Include(r => r.UserRoles)
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (r == null) return null;

        return new RoleItemDto
        {
            Id = r.Id,
            Name = r.Name,
            Code = r.Code,
            DisplayName = string.IsNullOrWhiteSpace(r.DisplayName) ? r.Name : r.DisplayName,
            Description = r.Description,
            IsActive = r.IsActive,
            UsersCount = r.UserRoles.Count,
            PermissionsCount = r.RolePermissions.Count
        };
    }

    public async Task<int> CreateRoleAsync(CreateRoleRequestDto request, string createdByUsername)
    {
        if (await _context.Roles.AnyAsync(r => r.Code == request.Code || r.Name == request.Name))
            throw new InvalidOperationException($"الدور '{request.Name}' أو الرمز '{request.Code}' موجود مسبقاً.");

        var role = new Role
        {
            Name = request.Name.Trim(),
            Code = request.Code.Trim().ToUpperInvariant(),
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Name.Trim() : request.DisplayName.Trim(),
            Description = request.Description?.Trim() ?? "",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        if (request.SelectedPermissionIds != null && request.SelectedPermissionIds.Any())
        {
            var validPids = await _context.Permissions.Where(p => request.SelectedPermissionIds.Contains(p.Id)).Select(p => p.Id).ToListAsync();
            foreach (var pid in validPids)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = pid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();
        }

        await _auditService.LogActivityAsync(
            null,
            createdByUsername,
            "Create",
            "Security",
            "Role",
            role.Id.ToString(),
            role.Code,
            $"إنشاء دور وظيفي جديد: {role.DisplayName} ({role.Code})"
        );

        await _auditService.LogSecurityEventAsync(
            "RoleCreated",
            null,
            createdByUsername,
            $"تم إنشاء الدور {role.Code} بواسطة {createdByUsername}"
        );

        return role.Id;
    }

    public async Task UpdateRoleAsync(EditRoleRequestDto request, string updatedByUsername)
    {
        var role = await _context.Roles
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == request.Id);

        if (role == null) throw new InvalidOperationException("الدور غير موجود.");

        // Check duplicate name or code
        if (await _context.Roles.AnyAsync(r => r.Id != request.Id && (r.Code == request.Code || r.Name == request.Name)))
            throw new InvalidOperationException("اسم الدور أو الرمز مستخدم بالفعل في دور آخر.");

        role.Name = request.Name.Trim();
        role.Code = request.Code.Trim().ToUpperInvariant();
        role.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Name.Trim() : request.DisplayName.Trim();
        role.Description = request.Description?.Trim() ?? "";
        role.IsActive = request.IsActive;
        role.UpdatedAt = DateTime.UtcNow;

        _context.RolePermissions.RemoveRange(role.RolePermissions);
        if (request.SelectedPermissionIds != null && request.SelectedPermissionIds.Any())
        {
            var validPids = await _context.Permissions.Where(p => request.SelectedPermissionIds.Contains(p.Id)).Select(p => p.Id).ToListAsync();
            foreach (var pid in validPids)
            {
                _context.RolePermissions.Add(new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = pid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _context.SaveChangesAsync();

        await _auditService.LogActivityAsync(
            null,
            updatedByUsername,
            "Update",
            "Security",
            "Role",
            role.Id.ToString(),
            role.Code,
            $"تعديل بيانات وصلاحيات الدور: {role.DisplayName}"
        );
    }

    public async Task<bool> ToggleRoleStatusAsync(int roleId, bool isActive, string modifiedByUsername)
    {
        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId);
        if (role == null) return false;

        if (role.Code == "SUPER_ADMIN" && !isActive)
            throw new InvalidOperationException("لا يمكن تعطيل دور مدير النظام الأعلى.");

        role.IsActive = isActive;
        role.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        await _auditService.LogSecurityEventAsync(
            isActive ? "RoleEnabled" : "RoleDisabled",
            null,
            modifiedByUsername,
            $"تم {(isActive ? "تفعيل" : "تعطيل")} الدور {role.Code} بواسطة {modifiedByUsername}"
        );

        return true;
    }
}
