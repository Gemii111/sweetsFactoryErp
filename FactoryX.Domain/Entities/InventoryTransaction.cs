using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum InventoryTransactionType
{
    PurchaseReceipt = 1,
    MaterialIssue = 2,
    ProductionConsumption = 3,
    ProductionOutput = 4,
    PackagingConsumption = 5,
    Sales = 6,
    SalesShipment = 6,
    SalesReturn = 7,
    PurchaseReturn = 8,
    StockAdjustment = 9,
    StockTransfer = 10,
    Waste = 11,
    Damage = 12,
    Expiry = 13,
    FinishedGoodsReceipt = 14,
    FinishedGoodsAdjustment = 15,
    FinishedGoodsTransfer = 16
}

public class InventoryTransaction : EntityBase
{
    public InventoryTransactionType TransactionType { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;

    public int WarehouseId { get; set; }
    public int? SourceLocationId { get; set; }
    public int? DestinationLocationId { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public string ReferenceDocumentNumber { get; set; } = string.Empty; // e.g. PO-001, SO-001, BATCH-001, FG-001
    public int? UserId { get; set; }
    public string Notes { get; set; } = string.Empty;

    public Material? Material { get; set; }
    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? SourceLocation { get; set; }
    public WarehouseLocation? DestinationLocation { get; set; }
    public User? User { get; set; }
}

public class StockBalance : EntityBase
{
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }
    
    public string BatchNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? Location { get; set; }
    public Material? Material { get; set; }
    public Product? Product { get; set; }
}
