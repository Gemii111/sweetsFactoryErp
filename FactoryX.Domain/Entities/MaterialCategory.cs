using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum MaterialCategoryType
{
    RawMaterial = 1,
    PackagingMaterial = 2
}

public class MaterialCategory : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MaterialCategoryType CategoryType { get; set; } = MaterialCategoryType.RawMaterial;
    public bool IsActive { get; set; } = true;

    public ICollection<Material>? Materials { get; set; }
}
