using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class ProductionRecord : EntityBase
{
    public int WorkOrderId { get; set; }
    public int? ProductionBatchId { get; set; }
    public int OperatorId { get; set; }
    public int QuantityProduced { get; set; }
    public DateTime Timestamp { get; set; }

    public WorkOrder? WorkOrder { get; set; }
    public ProductionBatch? ProductionBatch { get; set; }
    public Operator? Operator { get; set; }
}