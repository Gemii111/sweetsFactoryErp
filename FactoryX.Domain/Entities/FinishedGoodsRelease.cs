using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class FinishedGoodsRelease : EntityBase
{
    public string ReleaseNumber { get; set; } = string.Empty; // Format: FG-YYYYMMDD-XXXX

    public int ProductId { get; set; }
    public int ProductionBatchId { get; set; }
    public int? PackagingOrderId { get; set; }
    public int? QCInspectionId { get; set; }

    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";

    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public DateTime ProductionDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    public int ReleasedByUserId { get; set; }
    public DateTime ReleasedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public int? InventoryTransactionId { get; set; }

    // Navigation Properties
    public Product? Product { get; set; }
    public ProductionBatch? ProductionBatch { get; set; }
    public PackagingOrder? PackagingOrder { get; set; }
    public QualityInspection? QCInspection { get; set; }
    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? Location { get; set; }
    public User? ReleasedByUser { get; set; }
    public InventoryTransaction? InventoryTransaction { get; set; }
}
