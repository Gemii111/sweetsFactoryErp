using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class ProductionBatchDto
{
    public int Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;

    public int WorkOrderId { get; set; }
    public string WorkOrderNumber { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductArabicName { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;

    public int? RecipeVersionId { get; set; }
    public string? RecipeVersionNumber { get; set; }
    public string? RecipeVersionName { get; set; }

    public decimal PlannedQuantity { get; set; }
    public decimal ActualOutputQuantity { get; set; }
    public string OutputUnit { get; set; } = "KG";

    public DateTime ProductionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public DateTime? PauseTime { get; set; }

    public ProductionBatchStatus Status { get; set; } = ProductionBatchStatus.Planned;
    public string StatusName => Status switch
    {
        ProductionBatchStatus.Planned => "مخطط (Planned)",
        ProductionBatchStatus.InProgress => "قيد التشغيل (In Progress)",
        ProductionBatchStatus.Paused => "متوقف مؤقتاً (Paused)",
        ProductionBatchStatus.Completed => "مكتمل (Completed)",
        ProductionBatchStatus.Cancelled => "ملغي (Cancelled)",
        _ => Status.ToString()
    };
    public string StatusBadgeClass => Status switch
    {
        ProductionBatchStatus.Planned => "bg-primary",
        ProductionBatchStatus.InProgress => "bg-warning text-dark",
        ProductionBatchStatus.Paused => "bg-info text-dark",
        ProductionBatchStatus.Completed => "bg-success",
        ProductionBatchStatus.Cancelled => "bg-dark",
        _ => "bg-secondary"
    };

    public string QualityStatus { get; set; } = "Pending";
    public string? CancellationReason { get; set; }

    public int? ProductionLineId { get; set; }
    public string? ProductionLineName { get; set; }

    public int? WorkCenterId { get; set; }
    public string? WorkCenterName { get; set; }

    public int? MachineId { get; set; }
    public string? MachineName { get; set; }

    public int? OperatorId { get; set; }
    public string? OperatorName { get; set; }

    public int? ShiftId { get; set; }
    public string? ShiftName { get; set; }

    public int? TargetWarehouseId { get; set; }
    public string? TargetWarehouseName { get; set; }

    public string? Notes { get; set; }

    public List<ProductionConsumptionDto> Consumptions { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductionConsumptionDto
{
    public int Id { get; set; }
    public int ProductionBatchId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? MaterialArabicName { get; set; }

    public int? WarehouseId { get; set; }
    public string? WarehouseName { get; set; }

    public int? LocationId { get; set; }
    public string? LocationName { get; set; }

    public string RawMaterialBatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }

    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public decimal Variance { get; set; }
    public string Unit { get; set; } = "KG";

    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    public int? InventoryTransactionId { get; set; }
    public string? Notes { get; set; }
}

public class CreateProductionBatchRequest
{
    public string? BatchNumber { get; set; }
    public int WorkOrderId { get; set; }
    public decimal PlannedQuantity { get; set; } = 100m;
    public string OutputUnit { get; set; } = "KG";
    public DateTime ProductionDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? ExpiryDate { get; set; }

    public int? ProductionLineId { get; set; }
    public int? WorkCenterId { get; set; }
    public int? MachineId { get; set; }
    public int? OperatorId { get; set; }
    public int? ShiftId { get; set; }
    public int? TargetWarehouseId { get; set; }
    public string? Notes { get; set; }
}

public class AvailableMaterialBatchDto
{
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public decimal AvailableQuantity { get; set; }
    public string Unit { get; set; } = "KG";
    public DateTime? ExpiryDate { get; set; }
    public bool IsExpired => ExpiryDate.HasValue && ExpiryDate.Value.Date < DateTime.UtcNow.Date;
    public string ExpiryDateText => ExpiryDate.HasValue ? ExpiryDate.Value.ToString("yyyy-MM-dd") : "غير محدد";
    public string DisplayText => $"{WarehouseName} - دفعة: {(string.IsNullOrEmpty(BatchNumber) ? "عام" : BatchNumber)} | رصيد: {AvailableQuantity:N2} {Unit} | صلاحية: {ExpiryDateText}{(IsExpired ? " [منتهية الصلاحية]" : "")}";
}

public class BatchMaterialRequirementItemDto
{
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? MaterialArabicName { get; set; }
    public string Unit { get; set; } = "KG";

    public decimal RecipeQuantity { get; set; }
    public decimal PlannedQuantity { get; set; }

    public decimal TotalAvailableStock { get; set; }
    public decimal ShortageQuantity => Math.Max(0, PlannedQuantity - TotalAvailableStock);
    public bool HasShortage => TotalAvailableStock < PlannedQuantity;

    public List<AvailableMaterialBatchDto> AvailableBatches { get; set; } = new();

    // Selection for execution
    public int SelectedWarehouseId { get; set; }
    public int? SelectedLocationId { get; set; }
    public string SelectedBatchNumber { get; set; } = string.Empty;
    public decimal ConsumedQuantity { get; set; }
}

public class BatchExecutionDetailsDto
{
    public ProductionBatchDto Batch { get; set; } = null!;
    public List<BatchMaterialRequirementItemDto> MaterialRequirements { get; set; } = new();
    public bool CanStart => Batch.Status == ProductionBatchStatus.Planned && MaterialRequirements.All(m => !m.HasShortage);
    public bool CanPause => Batch.Status == ProductionBatchStatus.InProgress;
    public bool CanResume => Batch.Status == ProductionBatchStatus.Paused;
    public bool CanComplete => Batch.Status == ProductionBatchStatus.InProgress || Batch.Status == ProductionBatchStatus.Paused;
    public bool CanCancel => Batch.Status != ProductionBatchStatus.Completed && Batch.Status != ProductionBatchStatus.Cancelled;
}

public class StartBatchConsumptionItemRequest
{
    public int MaterialId { get; set; }
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public string RawMaterialBatchNumber { get; set; } = string.Empty;
    public decimal ActualQuantity { get; set; }
}

public class StartBatchRequest
{
    public int BatchId { get; set; }
    public List<StartBatchConsumptionItemRequest> Consumptions { get; set; } = new();
    public string? Notes { get; set; }
}

public class CompleteBatchRequest
{
    public int BatchId { get; set; }
    public decimal ActualOutputQuantity { get; set; }
    public string? Notes { get; set; }
}

public class CancelBatchRequest
{
    public int BatchId { get; set; }
    public string CancellationReason { get; set; } = string.Empty;
}

public record ProductionBatchFilterRequest(
    string? Search,
    int? WorkOrderId,
    int? ProductId,
    ProductionBatchStatus? Status,
    DateTime? FromDate,
    DateTime? ToDate);

public record ProductionBatchSummaryDto(
    int TotalBatches,
    int PlannedBatches,
    int InProgressBatches,
    int PausedBatches,
    int CompletedBatches,
    int CancelledBatches,
    decimal TotalPlannedQuantity,
    decimal TotalActualOutputQuantity);
