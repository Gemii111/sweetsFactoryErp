namespace FactoryX.Application.DTOs.Responses.MaterialResponses;

public sealed record UpdateMaterialResponse(
	int Id,
	string Name,
	string Code,
	string Unit);
