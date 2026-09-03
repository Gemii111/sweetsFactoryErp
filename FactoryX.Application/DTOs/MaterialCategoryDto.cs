namespace FactoryX.Application.DTOs;

public class MaterialCategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int MaterialCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record CreateMaterialCategoryRequest(
    string Code,
    string Name,
    string? Description);

public record UpdateMaterialCategoryRequest(
    int Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive);
