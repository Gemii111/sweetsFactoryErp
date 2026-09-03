using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum WasteType
{
    RawMaterialWaste = 1,
    ProductionProcessWaste = 2,
    OutputRejection = 3
}

public enum WasteStatus
{
    Draft = 1,
    PendingApproval = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}

public class WasteReason : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Waste> Wastes { get; set; } = new List<Waste>();
}

public class Waste : EntityBase
{
    public string WasteNumber { get; set; } = string.Empty; // Format: W-yyyyMMdd-XXXX
    public WasteType WasteType { get; set; } = WasteType.RawMaterialWaste;
    public WasteStatus Status { get; set; } = WasteStatus.Draft;

    // Linkages
    public int? ProductionBatchId { get; set; }
    public int? WorkOrderId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }

    // Physical Storage Linkage (where applicable for Raw Material Waste)
    public string? RawMaterialBatchNumber { get; set; }
    public int? WarehouseId { get; set; }
    public int? LocationId { get; set; }

    // Quantities & Costs
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    // Reason & Metadata
    public int? WasteReasonId { get; set; }
    public string ReasonDescription { get; set; } = string.Empty;
    public DateTime WasteDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    // User Auditing
    public int? CreatedByUserId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }

    // Legacy employee relation support
    public int? EmployeeId { get; set; }
    public string ApprovalStatus { get; set; } = "Draft";

    // Linked Inventory Deduction Transaction (for Approved Raw Material Waste)
    public int? InventoryTransactionId { get; set; }

    // Navigation Properties
    public ProductionBatch? ProductionBatch { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public Material? Material { get; set; }
    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? Location { get; set; }
    public WasteReason? WasteReason { get; set; }
    public Employee? Employee { get; set; }
    public User? CreatedByUser { get; set; }
    public User? ApprovedByUser { get; set; }
    public InventoryTransaction? InventoryTransaction { get; set; }
}
