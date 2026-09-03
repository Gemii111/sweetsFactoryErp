using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class WarehouseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WarehouseType Type { get; set; }
    public string TypeName => Type.ToString();
    public bool IsActive { get; set; }
    public int? BranchId { get; set; }
    public int LocationCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public record CreateWarehouseRequest(
    string Code,
    string Name,
    string Description,
    WarehouseType Type,
    int? BranchId);

public record UpdateWarehouseRequest(
    int Id,
    string Code,
    string Name,
    string Description,
    WarehouseType Type,
    bool IsActive,
    int? BranchId);
