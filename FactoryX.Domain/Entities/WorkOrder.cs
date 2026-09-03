using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum ProductionOrderStatus
{
    Draft = 1,
    Planned = 2,
    Released = 3,
    InProgress = 4,
    Completed = 5,
    Cancelled = 6
}

public enum ProductionOrderPriority
{
    Low = 1,
    Normal = 2,
    High = 3,
    Urgent = 4
}

public class WorkOrder : EntityBase
{
    public string OrderNumber { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public int? RecipeId { get; set; }
    public int? RecipeVersionId { get; set; }

    public decimal PlannedQuantity { get; set; } = 100m;
    public decimal ActualQuantityDecimal { get; set; }
    public string OutputUnit { get; set; } = "KG";

    // Legacy int property support
    public int Quantity
    {
        get => (int)PlannedQuantity;
        set => PlannedQuantity = value;
    }
    public int ActualQuantity
    {
        get => (int)ActualQuantityDecimal;
        set => ActualQuantityDecimal = value;
    }

    public DateTime PlannedDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? DueDate { get; set; }

    // Legacy date aliases
    public DateTime StartDate
    {
        get => PlannedDate;
        set => PlannedDate = value;
    }
    public DateTime EndDate
    {
        get => DueDate ?? PlannedDate;
        set => DueDate = value;
    }
    public DateTime? ActualCompletionDate { get; set; }

    public ProductionOrderPriority Priority { get; set; } = ProductionOrderPriority.Normal;
    public ProductionOrderStatus OrderStatus { get; set; } = ProductionOrderStatus.Draft;

    public string Status
    {
        get => OrderStatus switch
        {
            ProductionOrderStatus.Draft => "Draft",
            ProductionOrderStatus.Planned => "Planned",
            ProductionOrderStatus.Released => "Released",
            ProductionOrderStatus.InProgress => "InProgress",
            ProductionOrderStatus.Completed => "Completed",
            ProductionOrderStatus.Cancelled => "Cancelled",
            _ => "Draft"
        };
        set
        {
            if (Enum.TryParse<ProductionOrderStatus>(value, true, out var parsed))
            {
                OrderStatus = parsed;
            }
            else if (string.Equals(value, "InProduction", StringComparison.OrdinalIgnoreCase))
            {
                OrderStatus = ProductionOrderStatus.InProgress;
            }
        }
    }

    // Resource assignments
    public int? ProductionAreaId { get; set; }
    public int? ProductionLineId { get; set; }
    public int? WorkCenterId { get; set; }
    public int? MachineId { get; set; }
    public int? OperatorId { get; set; }
    public int? ShiftId { get; set; }

    public string? Notes { get; set; }

    // Navigations
    public Product? Product { get; set; }
    public Recipe? Recipe { get; set; }
    public RecipeVersion? RecipeVersion { get; set; }
    public ProductionArea? ProductionArea { get; set; }
    public ProductionLine? ProductionLine { get; set; }
    public WorkCenter? WorkCenter { get; set; }
    public Machine? Machine { get; set; }
    public Operator? Operator { get; set; }
    public Shift? Shift { get; set; }

    public ICollection<WorkOrderMaterialRequirement>? MaterialRequirements { get; set; }
    public ICollection<ProductionRecord>? ProductionRecords { get; set; }
    public ICollection<MaterialUsage>? MaterialUsages { get; set; }
    public ICollection<ProductionBatch>? ProductionBatches { get; set; }
}

public class WorkOrderMaterialRequirement : EntityBase
{
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
    public string? Notes { get; set; }

    public WorkOrder? WorkOrder { get; set; }
    public Material? Material { get; set; }
}