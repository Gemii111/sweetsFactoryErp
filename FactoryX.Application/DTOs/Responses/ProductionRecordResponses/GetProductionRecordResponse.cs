namespace FactoryX.Application.DTOs.Responses.ProductionRecordResponses;

public sealed record GetProductionRecordResponse(
	int Id,
	int WorkOrderId,
	int OperatorId,
	string OperatorName,
	DateTime TimeStamp,
	int QuantityProduced
);
