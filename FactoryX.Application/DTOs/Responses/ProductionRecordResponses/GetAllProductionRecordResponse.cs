namespace FactoryX.Application.DTOs.Responses.ProductionRecordResponses;

public sealed record GetAllProductionRecordResponse(
	int Id,
	int WorkOrderId,
	int OperatorId,
	string OperatorName,
	DateTime TimeStamp,
	int QuantityProduced
);
