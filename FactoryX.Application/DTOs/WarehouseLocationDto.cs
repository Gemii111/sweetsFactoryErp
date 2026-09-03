namespace FactoryX.Application.DTOs;

public class WarehouseLocationDto
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string Section { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public record CreateWarehouseLocationRequest(
    int WarehouseId,
    string Code,
    string Name,
    string Section,
    string Description);

public record UpdateWarehouseLocationRequest(
    int Id,
    int WarehouseId,
    string Code,
    string Name,
    string Section,
    string Description,
    bool IsActive);
