namespace FactoryX.Application.DTOs;

public enum MaterialStockStatus
{
    OUT_OF_STOCK = 0,
    LOW_STOCK = 1,
    REORDER_REQUIRED = 2,
    NORMAL = 3,
    OVERSTOCKED = 4
}

public class MaterialDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? MaterialCategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    
    public string Unit { get; set; } = string.Empty;
    public string PurchaseUnit { get; set; } = string.Empty;
    public decimal ConversionFactor { get; set; } = 1.0m;
    
    public decimal MinimumStock { get; set; }
    public decimal ReorderLevel { get; set; }
    public decimal MaximumStock { get; set; }
    public decimal CurrentStock { get; set; }
    
    public decimal StandardCost { get; set; }
    public decimal CurrentCost { get; set; }
    public decimal LastPurchaseCost { get; set; }
    
    public int? WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string? BatchNumber { get; set; }
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public bool IsActive { get; set; } = true;
    
    public MaterialStockStatus StockStatus { get; set; }
    public string StockStatusName { get; set; } = string.Empty;
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public record CreateMaterialRequest(
    string Code,
    string SKU,
    string Name,
    string? ArabicName,
    string? Description,
    int? MaterialCategoryId,
    string Unit,
    string? PurchaseUnit,
    decimal ConversionFactor,
    decimal MinimumStock,
    decimal ReorderLevel,
    decimal MaximumStock,
    decimal StandardCost,
    decimal CurrentCost,
    decimal LastPurchaseCost,
    int? WarehouseId,
    string? BatchNumber,
    DateTime? ManufacturingDate,
    DateTime? ExpiryDate);

public record UpdateMaterialRequest(
    int Id,
    string Code,
    string SKU,
    string Name,
    string? ArabicName,
    string? Description,
    int? MaterialCategoryId,
    string Unit,
    string? PurchaseUnit,
    decimal ConversionFactor,
    decimal MinimumStock,
    decimal ReorderLevel,
    decimal MaximumStock,
    decimal StandardCost,
    decimal CurrentCost,
    decimal LastPurchaseCost,
    int? WarehouseId,
    string? BatchNumber,
    DateTime? ManufacturingDate,
    DateTime? ExpiryDate,
    bool IsActive);

public record MaterialFilterRequest(
    string? Search,
    int? CategoryId,
    bool? IsActive,
    MaterialStockStatus? StockStatus,
    string? ExpiryStatus);

public record MaterialStockSummaryDto(
    int TotalMaterials,
    int ActiveMaterials,
    int OutOfStockCount,
    int LowStockCount,
    int ReorderRequiredCount,
    int ExpiredCount,
    int ExpiringSoonCount);