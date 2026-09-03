using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class Product : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public string? Description { get; set; }
    public int? ProductCategoryId { get; set; }
    public ProductType ProductType { get; set; } = ProductType.FinishedProduct;

    // Weight & Packaging Units
    public decimal Weight { get; set; } = 1.0m;
    public string WeightUnit { get; set; } = "GRAM"; // GRAM, KG, PIECE
    public string Unit { get; set; } = "علبة";      // علبة, قطعة, كرتونة, كجم
    public decimal UnitWeightKg { get; set; } = 1.0m;

    // Pricing & Standard Cost
    public decimal SellingPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? DistributorPrice { get; set; }
    public decimal StandardCost { get; set; }
    public decimal MinimumStock { get; set; }

    // Shelf Life & Expiry
    public int ExpiryPeriod { get; set; } = 180;
    public string ExpiryUnit { get; set; } = "Days"; // Days, Months
    public int ExpiryPeriodDays { get; set; } = 180;

    public bool IsActive { get; set; } = true;

    // Navigation Properties
    public ProductCategory? ProductCategory { get; set; }
    public ICollection<WorkOrder>? WorkOrders { get; set; }
    public ICollection<Recipe>? Recipes { get; set; }
    public ICollection<InvoiceItem>? InvoiceItems { get; set; }
}