using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class PackagingBOMDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }

    public decimal PackSize { get; set; }
    public decimal PackSizeKg { get; set; }
    public string PackUnit { get; set; } = "Box";
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public decimal TotalPackagingMaterialCost { get; set; }
    public int ActiveVersionNumber { get; set; }

    public List<PackagingBOMVersionDto> Versions { get; set; } = new();
    public List<PackagingItemDto> CurrentItems { get; set; } = new();
}

public class PackagingBOMVersionDto
{
    public int Id { get; set; }
    public int PackagingBOMId { get; set; }
    public int VersionNumber { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public PackagingBOMStatus Status { get; set; }
    public string StatusText => Status switch
    {
        PackagingBOMStatus.Draft => "مسودة (Draft)",
        PackagingBOMStatus.Active => "نشط (Active)",
        PackagingBOMStatus.Inactive => "غير نشط (Inactive)",
        _ => Status.ToString()
    };
    public string? Notes { get; set; }
    public decimal PackagingCost { get; set; }

    public List<PackagingItemDto> Items { get; set; } = new();
}

public class PackagingItemDto
{
    public int Id { get; set; }
    public int? PackagingBOMId { get; set; }
    public int? PackagingBOMVersionId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string? MaterialCode { get; set; }
    public string? MaterialArabicName { get; set; }
    public decimal MaterialUnitCost { get; set; }

    public decimal QuantityRequired { get; set; }
    public string Unit { get; set; } = "Pcs";
    public int Sequence { get; set; }
    public bool IsOptional { get; set; }
    public string? Notes { get; set; }

    public decimal LineCost => Math.Round(QuantityRequired * MaterialUnitCost, 4);
}

public class CreatePackagingBOMRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public decimal PackSize { get; set; } = 1.0m;
    public decimal PackSizeKg { get; set; } = 1.0m;
    public string PackUnit { get; set; } = "Box";
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public List<PackagingItemRequest> Items { get; set; } = new();
}

public class UpdatePackagingBOMRequest
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public decimal PackSize { get; set; }
    public decimal PackSizeKg { get; set; }
    public string PackUnit { get; set; } = "Box";
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public List<PackagingItemRequest> Items { get; set; } = new();
}

public class CreatePackagingBOMVersionRequest
{
    public int PackagingBOMId { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    public PackagingBOMStatus Status { get; set; } = PackagingBOMStatus.Draft;
    public string? Notes { get; set; }

    public List<PackagingItemRequest> Items { get; set; } = new();
}

public class PackagingItemRequest
{
    public int MaterialId { get; set; }
    public decimal QuantityRequired { get; set; } = 1.0m;
    public string Unit { get; set; } = "Pcs";
    public int Sequence { get; set; } = 1;
    public bool IsOptional { get; set; } = false;
    public string? Notes { get; set; }
}

public class PackagingCostSummaryDto
{
    public int PackagingBOMId { get; set; }
    public string PackagingCode { get; set; } = string.Empty;
    public string PackagingName { get; set; } = string.Empty;
    public decimal PackSizeKg { get; set; }
    public decimal CostPerPack { get; set; }
    public decimal CostPerKg { get; set; }
    public List<PackagingCostItemDetailDto> ItemBreakdown { get; set; } = new();
}

public class PackagingCostItemDetailDto
{
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal LineCost { get; set; }
}
