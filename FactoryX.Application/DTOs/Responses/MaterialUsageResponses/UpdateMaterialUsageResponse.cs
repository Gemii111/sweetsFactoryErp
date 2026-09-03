namespace FactoryX.Application.DTOs.Responses.MaterialUsage;

public sealed record UpdateMaterialUsageResponse(
	int Id,
	int WorkOrderId,
	int MaterialId,
	int Quantity,
	string MaterialName);
