using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum PackagingBOMStatus
{
    Draft = 1,
    Active = 2,
    Inactive = 3
}

public class PackagingBOM : EntityBase
{
    public string Code { get; set; } = string.Empty; // e.g. SES-500-PKG
    public string Name { get; set; } = string.Empty;
    public int ProductId { get; set; }

    public decimal PackSize { get; set; } = 1.0m; // e.g. 500 or 1
    public decimal PackSizeKg { get; set; } = 1.0m; // Weight in KG (e.g. 0.5 for 500g, 1.0 for 1KG)
    public string PackUnit { get; set; } = "Box"; // Box, Pcs, Pack
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    // Backward compatibility properties
    public decimal OutputProductQuantity { get; set; } = 1.0m;
    public string Unit { get; set; } = "Box";

    public Product? Product { get; set; }
    public ICollection<PackagingBOMVersion> Versions { get; set; } = new List<PackagingBOMVersion>();
    public ICollection<PackagingItem> Items { get; set; } = new List<PackagingItem>();
}

public class PackagingBOMVersion : EntityBase
{
    public int PackagingBOMId { get; set; }
    public int VersionNumber { get; set; } = 1;
    public string VersionName { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    public PackagingBOMStatus Status { get; set; } = PackagingBOMStatus.Active;
    public string? Notes { get; set; }

    public PackagingBOM? PackagingBOM { get; set; }
    public ICollection<PackagingItem> Items { get; set; } = new List<PackagingItem>();
}

public class PackagingItem : EntityBase
{
    public int? PackagingBOMId { get; set; }
    public int? PackagingBOMVersionId { get; set; }
    public int MaterialId { get; set; } // Packaging Material (Box, Plastic Bag, Label, Sticker, etc.)
    public decimal QuantityRequired { get; set; } = 1.0m;
    public string Unit { get; set; } = "Pcs";
    public int Sequence { get; set; } = 1;
    public bool IsOptional { get; set; } = false;
    public string? Notes { get; set; }

    public PackagingBOM? PackagingBOM { get; set; }
    public PackagingBOMVersion? PackagingBOMVersion { get; set; }
    public Material? Material { get; set; }
}
