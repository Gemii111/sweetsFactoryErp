using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class Recipe : EntityBase
{
    public int ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public string? Description { get; set; }
    public decimal BaseOutputQuantity { get; set; } = 100.0m; // Default 100 KG batch
    public string Unit { get; set; } = "KG";
    public bool IsActive { get; set; } = true;

    public Product? Product { get; set; }
    public ICollection<RecipeVersion>? Versions { get; set; }
}

public class RecipeVersion : EntityBase
{
    public int RecipeId { get; set; }
    public string VersionNumber { get; set; } = "V1.0";
    public string? VersionName { get; set; }
    public RecipeStatus Status { get; set; } = RecipeStatus.Draft;

    // Effective Date Range
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow;
    public DateTime? EffectiveTo { get; set; }
    public DateTime EffectiveDate { get; set; } = DateTime.UtcNow; // Legacy compatibility
    
    // Expected Output & Waste Parameters
    public decimal ExpectedOutput { get; set; } = 100.0m;
    public string OutputUnit { get; set; } = "KG";
    public decimal ExpectedWastePercentage { get; set; } = 0.0m;

    // Costs Breakdown
    public decimal MaterialCost { get; set; }
    public decimal PackagingCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal MachineCost { get; set; }
    public decimal OverheadCost { get; set; }
    public decimal TotalProductionCost { get; set; }
    
    public decimal CostPerKg { get; set; }
    public decimal CostPerPiece { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public Recipe? Recipe { get; set; }
    public ICollection<RecipeItem>? Items { get; set; }
    public ICollection<WorkOrder>? WorkOrders { get; set; }
}

public class RecipeItem : EntityBase
{
    public int RecipeVersionId { get; set; }
    public int MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";
    public decimal? Percentage { get; set; }
    public int Sequence { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }

    public RecipeVersion? RecipeVersion { get; set; }
    public Material? Material { get; set; }
}
