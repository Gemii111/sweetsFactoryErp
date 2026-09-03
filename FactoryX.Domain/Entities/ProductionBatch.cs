using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum ProductionBatchStatus
{
    Planned = 1,
    InProgress = 2,
    Paused = 3,
    Completed = 4,
    Cancelled = 5
}

public class ProductionBatch : EntityBase
{
    public string BatchNumber { get; set; } = string.Empty; // e.g., B-20260830-0001
    public int WorkOrderId { get; set; }
    public int ProductId { get; set; }
    public int? RecipeVersionId { get; set; }

    public decimal PlannedQuantity { get; set; }
    public decimal ActualOutputQuantity { get; set; }
    public string OutputUnit { get; set; } = "KG";

    // Legacy property aliases
    public decimal ExpectedOutput
    {
        get => PlannedQuantity;
        set => PlannedQuantity = value;
    }
    public decimal ActualOutput
    {
        get => ActualOutputQuantity;
        set => ActualOutputQuantity = value;
    }
    public decimal WasteQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }

    public DateTime ProductionDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ExpiryDate { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? PauseTime { get; set; }

    public ProductionBatchStatus Status { get; set; } = ProductionBatchStatus.Planned;
    public string QualityStatus { get; set; } = "Pending"; // Pending, Approved, Rejected, Hold
    public string? CancellationReason { get; set; }

    public int? ProductionLineId { get; set; }
    public int? WorkCenterId { get; set; }
    public int? MachineId { get; set; }
    public int? OperatorId { get; set; }
    public int? ShiftId { get; set; }
    public int? TargetWarehouseId { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public WorkOrder? WorkOrder { get; set; }
    public Product? Product { get; set; }
    public RecipeVersion? RecipeVersion { get; set; }
    public ProductionLine? ProductionLine { get; set; }
    public WorkCenter? WorkCenter { get; set; }
    public Machine? Machine { get; set; }
    public Operator? Operator { get; set; }
    public Shift? Shift { get; set; }
    public Warehouse? TargetWarehouse { get; set; }

    public ICollection<ProductionConsumption>? Consumptions { get; set; }
    public ICollection<ProductionRecord>? ProductionRecords { get; set; }
    public ICollection<QualityInspection>? QualityInspections { get; set; }
    public ICollection<Waste>? WasteRecords { get; set; }
}

public class ProductionConsumption : EntityBase
{
    public int ProductionBatchId { get; set; }
    public int MaterialId { get; set; }
    public int? WarehouseId { get; set; }
    public int? LocationId { get; set; }

    public string RawMaterialBatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }

    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Variance { get; set; } // ActualQuantity - PlannedQuantity

    // Legacy property aliases
    public decimal IssuedQuantity
    {
        get => ActualQuantity;
        set => ActualQuantity = value;
    }
    public decimal ActualConsumption
    {
        get => ActualQuantity;
        set => ActualQuantity = value;
    }
    public decimal Difference
    {
        get => Variance;
        set => Variance = value;
    }
    public decimal WasteQuantity { get; set; }

    public string Unit { get; set; } = "KG";
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public int? InventoryTransactionId { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public ProductionBatch? ProductionBatch { get; set; }
    public Material? Material { get; set; }
    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? Location { get; set; }
    public InventoryTransaction? InventoryTransaction { get; set; }
}
