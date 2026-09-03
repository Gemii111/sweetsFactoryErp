using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class FinishedGoodsStock : EntityBase
{
    public int ProductId { get; set; }
    public int ProductionBatchId { get; set; }
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";

    public DateTime ProductionDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }

    // Compatibility aliases
    public decimal CostPerUnit
    {
        get => UnitCost;
        set => UnitCost = value;
    }

    public decimal TotalValue
    {
        get => TotalCost;
        set => TotalCost = value;
    }

    // Linkages
    public int? QCInspectionId { get; set; }
    public int? PackagingOrderId { get; set; }

    // Navigation Properties
    public Product? Product { get; set; }
    public ProductionBatch? ProductionBatch { get; set; }
    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? Location { get; set; }
    public QualityInspection? QCInspection { get; set; }
    public PackagingOrder? PackagingOrder { get; set; }
}
