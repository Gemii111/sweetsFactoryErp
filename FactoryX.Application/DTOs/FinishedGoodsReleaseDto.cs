using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class FinishedGoodsReleaseDto
{
    public int Id { get; set; }
    public string ReleaseNumber { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string? ProductArabicName { get; set; }

    public int ProductionBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal BatchActualOutputQuantity { get; set; }

    public int? PackagingOrderId { get; set; }
    public string? PackagingOrderNumber { get; set; }
    public string? PackagingBOMName { get; set; }
    public decimal? ActualPackagedQuantity { get; set; }

    public int? QCInspectionId { get; set; }
    public string? QCInspectionNumber { get; set; }
    public string? QCInspectionStatus { get; set; }

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string WarehouseCode { get; set; } = string.Empty;

    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? LocationCode { get; set; }

    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";

    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public DateTime ProductionDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    public int ReleasedByUserId { get; set; }
    public string ReleasedByUserName { get; set; } = string.Empty;
    public DateTime ReleasedAt { get; set; }
    public string? Notes { get; set; }

    public int? InventoryTransactionId { get; set; }

    // Traceability links
    public int? WorkOrderId { get; set; }
    public string? WorkOrderNumber { get; set; }
}

public class CreateFinishedGoodsReleaseRequest
{
    public int ProductionBatchId { get; set; }
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public decimal Quantity { get; set; }
    public string? Notes { get; set; }
}

public class ReleaseAvailabilityDto
{
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string OutputUnit { get; set; } = "KG";

    public decimal PlannedQuantity { get; set; }
    public decimal ActualOutputQuantity { get; set; }
    public decimal RejectedOutputQuantity { get; set; }
    public decimal AlreadyReleasedQuantity { get; set; }
    public decimal RemainingReleaseableQuantity { get; set; }

    public DateTime ProductionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Cost Breakdown
    public decimal ProductionUnitCost { get; set; }
    public decimal PackagingUnitCost { get; set; }
    public decimal TotalUnitCost { get; set; }

    // QC Gate Status
    public ReleaseGateResultDto QCGate { get; set; } = new();

    // Packaging Gate Status
    public bool PackagingRequired { get; set; }
    public bool PackagingCompleted { get; set; }
    public int? PackagingOrderId { get; set; }
    public string? PackagingOrderNumber { get; set; }
    public string? PackagingOrderStatus { get; set; }
    public decimal? ActualPackagedQuantity { get; set; }
    public string? PackagingGateReason { get; set; }

    // Overall Gate Status
    public bool CanRelease => QCGate.IsAllowed &&
                              (!PackagingRequired || PackagingCompleted) &&
                              RemainingReleaseableQuantity > 0;

    public string? BlockingReason { get; set; }
}
