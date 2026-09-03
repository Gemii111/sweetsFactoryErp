using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class AuditLog : EntityBase
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty; // Create, Update, Delete, Approve, Release, Post, Reverse, Execute
    public string Module { get; set; } = string.Empty; // Sales, Purchasing, Inventory, Production, QC, Packaging, FinishedGoods, Accounting, Security
    public string EntityType { get; set; } = string.Empty; // SalesOrder, JournalEntry, Waste, etc.
    public string EntityId { get; set; } = string.Empty;
    public string EntityNumber { get; set; } = string.Empty; // e.g. SO-001, JE-001
    public string Description { get; set; } = string.Empty;
    public string? OldValues { get; set; } // JSON
    public string? NewValues { get; set; } // JSON
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? CorrelationId { get; set; }

    public User? User { get; set; }
}

public class SecurityEvent : EntityBase
{
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string EventType { get; set; } = string.Empty; // LoginSuccess, LoginFailure, AccountLocked, AccountUnlocked, PasswordChanged, RoleAssigned, RoleRemoved, PermissionChanged, UserStatusChanged
    public int? UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string Details { get; set; } = string.Empty;
    public string Severity { get; set; } = "Info"; // Info, Warning, Critical

    public User? User { get; set; }
}
