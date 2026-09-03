namespace FactoryX.Application.DTOs;

public class ProductCategoryDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int ProductCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record CreateProductCategoryRequest(
    string Code,
    string Name,
    string? ArabicName,
    string? Description);

public record UpdateProductCategoryRequest(
    int Id,
    string Code,
    string Name,
    string? ArabicName,
    string? Description,
    bool IsActive);
