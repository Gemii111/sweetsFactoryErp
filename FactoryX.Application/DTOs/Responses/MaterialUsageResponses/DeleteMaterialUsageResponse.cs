namespace FactoryX.Application.DTOs.Responses.MaterialUsage;

public sealed record DeleteMaterialUsageResponse(
	int Id,
	int WorkOrderId,
	int MaterialId,
	int Quantity);
