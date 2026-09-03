using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class Role : EntityBase
{
    public string Name { get; set; } = string.Empty; // e.g. "Super Admin", "General Manager"
    public string Code { get; set; } = string.Empty; // e.g. SUPER_ADMIN, GENERAL_MANAGER, PRODUCTION_MANAGER
    public string DisplayName { get; set; } = string.Empty; // e.g. "مدير النظام الأعلى", "المدير العام"
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public class Permission : EntityBase
{
    public string Name { get; set; } = string.Empty; // e.g. "إنشاء أمر بيع"
    public string Code { get; set; } = string.Empty; // e.g. Sales.Order.Create
    public string Module { get; set; } = string.Empty; // e.g. Sales, Purchasing, Inventory, Production, QC, Packaging, FinishedGoods, Accounting, Reports, Security
    public string Action { get; set; } = string.Empty; // View, Create, Edit, Delete, Approve, Execute, Post, Reverse, Export, Manage
    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

public class RolePermission : EntityBase
{
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    public Role? Role { get; set; }
    public Permission? Permission { get; set; }
}

public class UserRole : EntityBase
{
    public int UserId { get; set; }
    public int RoleId { get; set; }

    public User? User { get; set; }
    public Role? Role { get; set; }
}

public class UserWarehouse : EntityBase
{
    public int UserId { get; set; }
    public int WarehouseId { get; set; }

    public User? User { get; set; }
    public Warehouse? Warehouse { get; set; }
}
