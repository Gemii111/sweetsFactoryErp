using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum PackagingMaterialType
{
    None = 0,
    Box = 1,
    PlasticBag = 2,
    Label = 3,
    Sticker = 4,
    Carton = 5,
    Ribbon = 6,
    Other = 7
}

public class Material : EntityBase
{
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? MaterialCategoryId { get; set; }
    
    // Packaging Classification
    public bool IsPackagingMaterial { get; set; } = false;
    public PackagingMaterialType PackagingType { get; set; } = PackagingMaterialType.None;

    // Unit Foundation
    public string Unit { get; set; } = string.Empty; // Stock Unit (e.g. KG, Liter, Piece, PCS)
    public string PurchaseUnit { get; set; } = string.Empty; // Purchase Unit (e.g. Ton, Bag, Box)
    public decimal ConversionFactor { get; set; } = 1.0m; // 1 PurchaseUnit = ConversionFactor * Unit

    // Stock Control Levels
    public decimal MinimumStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MaximumStock { get; set; }
    public decimal CurrentStock { get; set; }

    // Multi-tier Cost Foundation
    public decimal StandardCost { get; set; }
    public decimal CurrentCost { get; set; }
    public decimal LastPurchaseCost { get; set; }
    public decimal UnitCost { get; set; } // Backward compatibility

    // Tracking & Relations
    public int? SupplierId { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? WarehouseId { get; set; }
    public bool IsActive { get; set; } = true;

    public MaterialCategory? MaterialCategory { get; set; }
    public Supplier? Supplier { get; set; }
    public Warehouse? Warehouse { get; set; }
    public ICollection<MaterialUsage>? MaterialUsages { get; set; }
}