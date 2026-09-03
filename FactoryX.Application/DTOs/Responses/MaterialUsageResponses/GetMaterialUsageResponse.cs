namespace FactoryX.Application.DTOs.Responses.MaterialUsageResponses;

public sealed record GetMaterialUsageResponse(
	int Id,
	int WorkOrderId,
	int MaterialId,
	int Quantity,
	string MaterialName);
