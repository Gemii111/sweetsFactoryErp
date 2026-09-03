using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class MaterialUsage : EntityBase
{
    public int WorkOrderId { get; set; }
    public int MaterialId { get; set; }
    public int Quantity { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public Material? Material { get; set; }
}