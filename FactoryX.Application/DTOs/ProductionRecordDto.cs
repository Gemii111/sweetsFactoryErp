namespace FactoryX.Application.DTOs;

public class ProductionRecordDto
{
    public int Id { get; set; }
    public int WorkOrderId { get; set; }
    public int OperatorId { get; set; }
    public int QuantityProduced { get; set; }
    public DateTime Timestamp { get; set; }
    public string? WorkOrderName { get; set; }
    public string? OperatorName { get; set; }
}