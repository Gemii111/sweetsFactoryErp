using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class PackagingOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int ProductionBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductCode { get; set; }

    public int PackagingBOMId { get; set; }
    public string PackagingBOMName { get; set; } = string.Empty;
    public string PackagingBOMCode { get; set; } = string.Empty;
    public int? PackagingBOMVersionId { get; set; }
    public int VersionNumber { get; set; }

    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal TheoreticalMaxPacks { get; set; }
    public decimal PackSizeKg { get; set; }
    public string PackUnit { get; set; } = "Box";

    public PackagingOrderStatus Status { get; set; }
    public string StatusText => Status switch
    {
        PackagingOrderStatus.Draft => "مسودة (Draft)",
        PackagingOrderStatus.Planned => "مخطط (Planned)",
        PackagingOrderStatus.InProgress => "قيد التنفيذ (In Progress)",
        PackagingOrderStatus.Paused => "متوقف مؤقتاً (Paused)",
        PackagingOrderStatus.Completed => "مكتمل (Completed)",
        PackagingOrderStatus.Cancelled => "ملغي (Cancelled)",
        _ => Status.ToString()
    };

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int? OperatorId { get; set; }
    public string? OperatorName { get; set; }

    public decimal PackagingMaterialCost { get; set; }
    public string? Notes { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedAt { get; set; }

    public ReleaseGateResultDto? QCGateStatus { get; set; }

    public List<PackagingConsumptionDto> Consumptions { get; set; } = new();
    public List<PackagingRequirementDto> Requirements { get; set; } = new();
}

public class PackagingConsumptionDto
{
    public int Id { get; set; }
    public int PackagingOrderId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string? MaterialCode { get; set; }

    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public string Unit { get; set; } = "Pcs";

    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? BatchNumber { get; set; }

    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public int? InventoryTransactionId { get; set; }
    public string? Notes { get; set; }
}

public class PackagingRequirementDto
{
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public decimal QuantityPerPack { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal ShortageQuantity => Math.Max(0m, RequiredQuantity - AvailableQuantity);
    public string Unit { get; set; } = "Pcs";
    public bool IsSufficient => AvailableQuantity >= RequiredQuantity;
    public string AvailabilityStatus => IsSufficient ? "متوفر (AVAILABLE)" : "عجز (SHORTAGE)";
}

public class CreatePackagingOrderRequest
{
    public int ProductionBatchId { get; set; }
    public int PackagingBOMId { get; set; }
    public int? PackagingBOMVersionId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public int? OperatorId { get; set; }
    public string? Notes { get; set; }
}

public class ExecutePackagingOrderRequest
{
    public int PackagingOrderId { get; set; }
    public decimal ActualPackagedQuantity { get; set; }
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public string? Notes { get; set; }
    public List<PackagingConsumptionItemRequest> Consumptions { get; set; } = new();
}

public class PackagingConsumptionItemRequest
{
    public int MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "Pcs";
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public string? BatchNumber { get; set; }
    public string? Notes { get; set; }
}

public class PausePackagingOrderRequest
{
    public int PackagingOrderId { get; set; }
    public string? Notes { get; set; }
}

public class CancelPackagingOrderRequest
{
    public int PackagingOrderId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
}
