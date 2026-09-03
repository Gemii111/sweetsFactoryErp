using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum UserStatus
{
    Active = 1,
    Disabled = 2,
    Locked = 3
}

public class User : EntityBase
{
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; // Legacy primary role name
    public string? FullName { get; set; }
    public string? Email { get; set; }

    // Phase 18: Enhanced Security & Status Lifecycle
    public bool IsActive { get; set; } = true;
    public int FailedLoginCount { get; set; } = 0;
    public DateTime? LockedUntil { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public bool IsAllWarehousesAllowed { get; set; } = true;

    // Navigation Properties
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<UserWarehouse> UserWarehouses { get; set; } = new List<UserWarehouse>();
}