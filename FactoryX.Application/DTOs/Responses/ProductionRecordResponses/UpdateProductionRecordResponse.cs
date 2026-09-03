namespace FactoryX.Application.DTOs.Responses.ProductionRecord;

public sealed record UpdateProductionRecordResponse(
	int Id,
	int WorkOrderId,
	int OperatorId,
	string OperatorName,
	DateTime TimeStamp,
	int QuantityProduced
);