namespace FactoryX.Application.DTOs;

#region User Management DTOs
public class UserAdminItemDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public List<string> AssignedRoles { get; set; } = new();
    public bool IsActive { get; set; } = true;
    public bool IsLocked { get; set; }
    public DateTime? LockedUntil { get; set; }
    public int FailedLoginCount { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsAllWarehousesAllowed { get; set; } = true;
    public List<string> AllowedWarehouseNames { get; set; } = new();
}

public class CreateUserRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string PrimaryRole { get; set; } = string.Empty;
    public List<int> RoleIds { get; set; } = new();
    public bool IsAllWarehousesAllowed { get; set; } = true;
    public List<int> AllowedWarehouseIds { get; set; } = new();
}

public class EditUserRequestDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NewPassword { get; set; }
    public string PrimaryRole { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<int> RoleIds { get; set; } = new();
    public bool IsAllWarehousesAllowed { get; set; } = true;
    public List<int> AllowedWarehouseIds { get; set; } = new();
}

public class UserWarehouseAssignmentDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public bool IsAllWarehousesAllowed { get; set; } = true;
    public List<int> SelectedWarehouseIds { get; set; } = new();
    public List<WarehouseLookupItemDto> AvailableWarehouses { get; set; } = new();
}

public class WarehouseLookupItemDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
#endregion

#region Role & Permission DTOs
public class RoleItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int UsersCount { get; set; }
    public int PermissionsCount { get; set; }
}

public class CreateRoleRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<int> SelectedPermissionIds { get; set; } = new();
}

public class EditRoleRequestDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public List<int> SelectedPermissionIds { get; set; } = new();
}

public class PermissionItemDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsAssigned { get; set; }
}

public class RolePermissionMatrixDto
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleDisplayName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public Dictionary<string, List<PermissionItemDto>> PermissionsByModule { get; set; } = new();
}
#endregion

#region Audit Trail & Security Event DTOs
public class AuditLogFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? UserId { get; set; }
    public string? Module { get; set; }
    public string? Action { get; set; }
    public string? EntityType { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class AuditLogItemDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string EntityNumber { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public string? CorrelationId { get; set; }
}

public class SecurityEventFilterDto
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? EventType { get; set; }
    public string? Severity { get; set; }
    public string? Username { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class SecurityEventItemDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string EventType { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string Details { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info";
}

public class SecurityDashboardDto
{
    public int TotalUsersCount { get; set; }
    public int ActiveUsersCount { get; set; }
    public int DisabledUsersCount { get; set; }
    public int LockedUsersCount { get; set; }
    public int TotalRolesCount { get; set; }
    public int TotalPermissionsCount { get; set; }

    public int TodaySuccessfulLoginsCount { get; set; }
    public int TodayFailedLoginsCount { get; set; }
    public int TodayHighRiskActionsCount { get; set; }

    public List<SecurityEventItemDto> RecentSecurityEvents { get; set; } = new();
    public List<AuditLogItemDto> RecentHighRiskAudits { get; set; } = new();
}
#endregion
