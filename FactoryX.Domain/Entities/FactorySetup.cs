using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class Factory : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Branch>? Branches { get; set; }
}

public class Branch : EntityBase
{
    public int FactoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Factory? Factory { get; set; }
    public ICollection<Warehouse>? Warehouses { get; set; }
    public ICollection<ProductionArea>? ProductionAreas { get; set; }
}

public enum WarehouseType
{
    RawMaterial = 1,
    Packaging = 2,
    Production = 3,
    FinishedGoods = 4,
    Waste = 5
}

public class Warehouse : EntityBase
{
    public int? BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WarehouseType Type { get; set; }
    public bool IsActive { get; set; } = true;

    public Branch? Branch { get; set; }
    public ICollection<WarehouseLocation>? Locations { get; set; }
}

public class WarehouseLocation : EntityBase
{
    public int WarehouseId { get; set; }
    public string Section { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Warehouse? Warehouse { get; set; }
}

public class ProductionArea : EntityBase
{
    public int BranchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public Branch? Branch { get; set; }
    public ICollection<ProductionLine>? Lines { get; set; }
}

public class ProductionLine : EntityBase
{
    public int ProductionAreaId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public ProductionArea? ProductionArea { get; set; }
    public ICollection<WorkCenter>? WorkCenters { get; set; }
}

public class WorkCenter : EntityBase
{
    public int ProductionLineId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public ProductionLine? ProductionLine { get; set; }
    public ICollection<Machine>? Machines { get; set; }
}

public class Department : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;

    public ICollection<Employee>? Employees { get; set; }
}

public class Employee : EntityBase
{
    public string EmployeeNumber { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public Department? Department { get; set; }
}
