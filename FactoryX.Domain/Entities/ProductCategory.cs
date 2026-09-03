using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class ProductCategory : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Product>? Products { get; set; }
}
