namespace FactoryX.Application.DTOs.Requests.ProductionRecordRequests;

public sealed record InsertProductionRecordRequest(
	int WorkOrderId,
	int OperatorId,
	int QuantityProduced,
	DateTime Timestamp,
	string? WorkOrderName,
	string? OperatorName);
