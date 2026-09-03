namespace FactoryX.Application.DTOs.Responses.MaterialUsage;

public sealed record InsertMaterialUsageResponse(
	int Id,
	int WorkOrderId,
	int MaterialId,
	int Quantity,
	string MaterialName);
