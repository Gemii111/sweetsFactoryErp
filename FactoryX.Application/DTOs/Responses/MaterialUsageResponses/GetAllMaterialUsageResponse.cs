namespace FactoryX.Application.DTOs.Responses.MaterialUsageResponses;

public sealed record GetAllMaterialUsageResponse(
	int Id,
	int WorkOrderId,
	int MaterialId,
	int Quantity,
	string MaterialName);
