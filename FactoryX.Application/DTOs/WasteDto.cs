using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class WasteDto
{
    public int Id { get; set; }
    public string WasteNumber { get; set; } = string.Empty;
    public WasteType WasteType { get; set; }
    public string WasteTypeName { get; set; } = string.Empty;
    public WasteStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;

    // Linkages
    public int? ProductionBatchId { get; set; }
    public string? ProductionBatchNumber { get; set; }
    public int? WorkOrderId { get; set; }
    public string? WorkOrderNumber { get; set; }
    public int? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public string? MaterialCode { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }

    // Physical Location
    public string? RawMaterialBatchNumber { get; set; }
    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }

    // Quantity & Cost
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    // Reason & Date
    public int? WasteReasonId { get; set; }
    public string? WasteReasonCode { get; set; }
    public string? WasteReasonName { get; set; }
    public string ReasonDescription { get; set; } = string.Empty;
    public DateTime WasteDate { get; set; }
    public string? Notes { get; set; }

    // Users & Audit
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovalNotes { get; set; }

    // Inventory Transaction Link
    public int? InventoryTransactionId { get; set; }
    public string? InventoryTransactionRef { get; set; }
}

public class CreateWasteRequest
{
    public WasteType WasteType { get; set; } = WasteType.RawMaterialWaste;

    public int? ProductionBatchId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }

    public string? RawMaterialBatchNumber { get; set; }
    public int? WarehouseId { get; set; }
    public int? LocationId { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";
    public decimal UnitCost { get; set; }

    public int? WasteReasonId { get; set; }
    public string? ReasonDescription { get; set; }
    public DateTime WasteDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public bool SubmitDirectly { get; set; } = false;
}

public class UpdateWasteRequest
{
    public int Id { get; set; }
    public WasteType WasteType { get; set; }

    public int? ProductionBatchId { get; set; }
    public int? MaterialId { get; set; }
    public int? ProductId { get; set; }

    public string? RawMaterialBatchNumber { get; set; }
    public int? WarehouseId { get; set; }
    public int? LocationId { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";
    public decimal UnitCost { get; set; }

    public int? WasteReasonId { get; set; }
    public string? ReasonDescription { get; set; }
    public DateTime WasteDate { get; set; }
    public string? Notes { get; set; }

    public bool SubmitDirectly { get; set; } = false;
}

public class ApproveWasteRequest
{
    public int WasteId { get; set; }
    public string? ApprovalNotes { get; set; }
}

public class RejectWasteRequest
{
    public int WasteId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

public class WasteSummaryDto
{
    public int TotalWastesCount { get; set; }
    public int PendingApprovalsCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public decimal TotalWasteCost { get; set; }
    public decimal RawMaterialWasteCost { get; set; }
    public decimal ProcessWasteCost { get; set; }
    public decimal OutputRejectionCost { get; set; }
}
