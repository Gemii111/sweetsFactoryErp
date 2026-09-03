using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class ProductDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public string? Description { get; set; }
    public int? ProductCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public ProductType ProductType { get; set; } = ProductType.FinishedProduct;
    public string ProductTypeName => ProductType switch
    {
        ProductType.FinishedProduct => "منتج تام الصنع (Finished Product)",
        ProductType.SemiFinishedProduct => "منتج نصف مصنع / وسيط (Semi-Finished)",
        ProductType.PackagingItem => "مادة تعبئة / علبة مجمعة (Packaging Item)",
        ProductType.AssortedBox => "علبة مشكلة (Assorted Box)",
        _ => ProductType.ToString()
    };

    // Weight & Units
    public decimal Weight { get; set; }
    public string WeightUnit { get; set; } = "GRAM";
    public string Unit { get; set; } = "علبة";
    public decimal UnitWeightKg { get; set; }

    // Pricing & Standard Cost
    public decimal SellingPrice { get; set; }
    public decimal? WholesalePrice { get; set; }
    public decimal? DistributorPrice { get; set; }
    public decimal StandardCost { get; set; }
    public decimal MinimumStock { get; set; }

    // Shelf Life / Expiry
    public int ExpiryPeriod { get; set; }
    public string ExpiryUnit { get; set; } = "Days";
    public int ExpiryPeriodDays { get; set; }

    public bool IsActive { get; set; } = true;

    // Statistics & References
    public int WorkOrderCount { get; set; }
    public int ProductionRecordCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record CreateProductRequest(
    string Code,
    string SKU,
    string? Barcode,
    string Name,
    string? ArabicName,
    string? Description,
    int? ProductCategoryId,
    ProductType ProductType,
    decimal Weight,
    string? WeightUnit,
    string? Unit,
    decimal SellingPrice,
    decimal? WholesalePrice,
    decimal? DistributorPrice,
    decimal StandardCost,
    decimal MinimumStock,
    int ExpiryPeriod,
    string? ExpiryUnit);

public record UpdateProductRequest(
    int Id,
    string Code,
    string SKU,
    string? Barcode,
    string Name,
    string? ArabicName,
    string? Description,
    int? ProductCategoryId,
    ProductType ProductType,
    decimal Weight,
    string? WeightUnit,
    string? Unit,
    decimal SellingPrice,
    decimal? WholesalePrice,
    decimal? DistributorPrice,
    decimal StandardCost,
    decimal MinimumStock,
    int ExpiryPeriod,
    string? ExpiryUnit,
    bool IsActive);

public record ProductFilterRequest(
    string? Search,
    int? CategoryId,
    bool? IsActive,
    ProductType? ProductType);

public record ProductSummaryDto(
    int TotalProducts,
    int ActiveProducts,
    int InactiveProducts,
    int FinishedProductsCount,
    int AssortedBoxesCount,
    int CategoriesCount);