using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public enum MaterialAvailabilityStatus
{
    Available = 1,
    Shortage = 2,
    OutOfStock = 3
}

public class MaterialRequirementDto
{
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? MaterialArabicName { get; set; }
    public string StockUnit { get; set; } = "KG";

    public decimal RecipeQuantity { get; set; }
    public decimal ExpectedOutputQuantity { get; set; }
    public decimal PlannedProductionQuantity { get; set; }
    public decimal RequiredQuantity { get; set; }

    public decimal CurrentStock { get; set; }
    public decimal ShortageQuantity { get; set; }
    public MaterialAvailabilityStatus AvailabilityStatus { get; set; }

    public string AvailabilityStatusText => AvailabilityStatus switch
    {
        MaterialAvailabilityStatus.Available => "متوفر بالكامل (AVAILABLE)",
        MaterialAvailabilityStatus.Shortage => "عجز جزئي (SHORTAGE)",
        MaterialAvailabilityStatus.OutOfStock => "غير متوفر بالمخزن (OUT OF STOCK)",
        _ => AvailabilityStatus.ToString()
    };

    public string AvailabilityBadgeClass => AvailabilityStatus switch
    {
        MaterialAvailabilityStatus.Available => "bg-success",
        MaterialAvailabilityStatus.Shortage => "bg-warning text-dark",
        MaterialAvailabilityStatus.OutOfStock => "bg-danger",
        _ => "bg-secondary"
    };
}

public class WorkOrderMaterialRequirementDto
{
    public int Id { get; set; }
    public int WorkOrderId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? MaterialArabicName { get; set; }
    public string StockUnit { get; set; } = "KG";
    public decimal RecipeQuantity { get; set; }
    public decimal ExpectedOutputQuantity { get; set; }
    public decimal PlannedProductionQuantity { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal AllocatedQuantity { get; set; }
    public decimal CurrentStock { get; set; }
    public decimal ShortageQuantity => Math.Max(0, RequiredQuantity - CurrentStock);
    public MaterialAvailabilityStatus AvailabilityStatus =>
        CurrentStock >= RequiredQuantity ? MaterialAvailabilityStatus.Available :
        CurrentStock > 0 ? MaterialAvailabilityStatus.Shortage :
        MaterialAvailabilityStatus.OutOfStock;
    public string? Notes { get; set; }
}

public class ProductionOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductArabicName { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string ProductDisplayName => string.IsNullOrEmpty(ProductArabicName) ? ProductName : $"{ProductName} ({ProductArabicName})";

    public int? RecipeId { get; set; }
    public string? RecipeCode { get; set; }
    public string? RecipeName { get; set; }

    public int? RecipeVersionId { get; set; }
    public string? RecipeVersionNumber { get; set; }
    public string? RecipeVersionName { get; set; }
    public decimal RecipeExpectedOutput { get; set; }
    public string RecipeOutputUnit { get; set; } = "KG";

    public decimal PlannedQuantity { get; set; }
    public decimal ActualQuantity { get; set; }
    public string OutputUnit { get; set; } = "KG";

    public DateTime PlannedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ActualCompletionDate { get; set; }

    public ProductionOrderPriority Priority { get; set; } = ProductionOrderPriority.Normal;
    public string PriorityName => Priority switch
    {
        ProductionOrderPriority.Low => "منخفضة (Low)",
        ProductionOrderPriority.Normal => "عادية (Normal)",
        ProductionOrderPriority.High => "مرتفعة (High)",
        ProductionOrderPriority.Urgent => "طارئة / عاجلة (Urgent)",
        _ => Priority.ToString()
    };
    public string PriorityBadgeClass => Priority switch
    {
        ProductionOrderPriority.Low => "bg-secondary",
        ProductionOrderPriority.Normal => "bg-info text-dark",
        ProductionOrderPriority.High => "bg-warning text-dark",
        ProductionOrderPriority.Urgent => "bg-danger",
        _ => "bg-secondary"
    };

    public ProductionOrderStatus OrderStatus { get; set; } = ProductionOrderStatus.Draft;
    public string StatusName => OrderStatus switch
    {
        ProductionOrderStatus.Draft => "مسودة (Draft)",
        ProductionOrderStatus.Planned => "مخطط (Planned)",
        ProductionOrderStatus.Released => "مطلق وجاهز للتشغيل (Released)",
        ProductionOrderStatus.InProgress => "قيد الإنتاج (In Progress)",
        ProductionOrderStatus.Completed => "مكتمل (Completed)",
        ProductionOrderStatus.Cancelled => "ملغي (Cancelled)",
        _ => OrderStatus.ToString()
    };
    public string StatusBadgeClass => OrderStatus switch
    {
        ProductionOrderStatus.Draft => "bg-secondary",
        ProductionOrderStatus.Planned => "bg-primary",
        ProductionOrderStatus.Released => "bg-info text-dark",
        ProductionOrderStatus.InProgress => "bg-warning text-dark",
        ProductionOrderStatus.Completed => "bg-success",
        ProductionOrderStatus.Cancelled => "bg-dark",
        _ => "bg-secondary"
    };

    // Resources
    public int? ProductionAreaId { get; set; }
    public string? ProductionAreaName { get; set; }

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

    public string? Notes { get; set; }

    public List<WorkOrderMaterialRequirementDto> MaterialRequirements { get; set; } = new();
    public List<MaterialRequirementDto> LiveMaterialRequirements { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateProductionOrderRequest
{
    public string? OrderNumber { get; set; }
    public int ProductId { get; set; }
    public int RecipeVersionId { get; set; }
    public decimal PlannedQuantity { get; set; } = 100m;
    public string OutputUnit { get; set; } = "KG";
    public DateTime PlannedDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? DueDate { get; set; }
    public ProductionOrderPriority Priority { get; set; } = ProductionOrderPriority.Normal;
    public ProductionOrderStatus InitialStatus { get; set; } = ProductionOrderStatus.Draft;

    public int? ProductionAreaId { get; set; }
    public int? ProductionLineId { get; set; }
    public int? WorkCenterId { get; set; }
    public int? MachineId { get; set; }
    public int? OperatorId { get; set; }
    public int? ShiftId { get; set; }
    public string? Notes { get; set; }

    public CreateProductionOrderRequest() { }
}

public class UpdateProductionOrderRequest
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int RecipeVersionId { get; set; }
    public decimal PlannedQuantity { get; set; }
    public string OutputUnit { get; set; } = "KG";
    public DateTime PlannedDate { get; set; }
    public DateTime? DueDate { get; set; }
    public ProductionOrderPriority Priority { get; set; } = ProductionOrderPriority.Normal;

    public int? ProductionAreaId { get; set; }
    public int? ProductionLineId { get; set; }
    public int? WorkCenterId { get; set; }
    public int? MachineId { get; set; }
    public int? OperatorId { get; set; }
    public int? ShiftId { get; set; }
    public string? Notes { get; set; }

    public UpdateProductionOrderRequest() { }
}

public record ProductionOrderFilterRequest(
    string? Search,
    int? ProductId,
    ProductionOrderStatus? Status,
    ProductionOrderPriority? Priority,
    DateTime? FromDate,
    DateTime? ToDate);

public record ProductionOrderSummaryDto(
    int TotalOrders,
    int DraftOrders,
    int PlannedOrders,
    int ReleasedOrders,
    int InProgressOrders,
    int CompletedOrders,
    int CancelledOrders,
    decimal TotalPlannedQuantity);
