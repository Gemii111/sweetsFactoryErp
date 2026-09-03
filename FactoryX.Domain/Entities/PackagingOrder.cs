using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum PackagingOrderStatus
{
    Draft = 1,
    Planned = 2,
    InProgress = 3,
    Paused = 4,
    Completed = 5,
    Cancelled = 6
}

public class PackagingOrder : EntityBase
{
    public string OrderNumber { get; set; } = string.Empty; // e.g. PKG-20260830-0001
    public int ProductionBatchId { get; set; }
    public int ProductId { get; set; }
    public int PackagingBOMId { get; set; }
    public int? PackagingBOMVersionId { get; set; }

    public decimal PlannedQuantity { get; set; } // Planned number of finished packs
    public decimal ActualQuantity { get; set; } // Actual packaged quantity
    public decimal TheoreticalMaxPacks { get; set; } // Based on batch actual output / pack size in KG

    public PackagingOrderStatus Status { get; set; } = PackagingOrderStatus.Draft;
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public int? OperatorId { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }

    public decimal PackagingMaterialCost { get; set; }

    // Audit Trail
    public int? CreatedByUserId { get; set; }
    public int? CompletedByUserId { get; set; }

    // Navigation Properties
    public ProductionBatch? ProductionBatch { get; set; }
    public Product? Product { get; set; }
    public PackagingBOM? PackagingBOM { get; set; }
    public PackagingBOMVersion? PackagingBOMVersion { get; set; }
    public Operator? Operator { get; set; }
    public User? CreatedByUser { get; set; }
    public User? CompletedByUser { get; set; }

    public ICollection<PackagingConsumption> Consumptions { get; set; } = new List<PackagingConsumption>();
}

public class PackagingConsumption : EntityBase
{
    public int PackagingOrderId { get; set; }
    public int MaterialId { get; set; }

    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public string Unit { get; set; } = "Pcs";

    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public string? BatchNumber { get; set; }

    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public int? InventoryTransactionId { get; set; }
    public string? Notes { get; set; }

    public PackagingOrder? PackagingOrder { get; set; }
    public Material? Material { get; set; }
    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? Location { get; set; }
    public InventoryTransaction? InventoryTransaction { get; set; }
}
