namespace FactoryX.Application.DTOs.Responses.MaterialResponses;

public sealed record GetAllMaterialResponse(
	int Id,
	string Name,
	string Code,
	string Unit);
