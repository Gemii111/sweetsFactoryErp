namespace FactoryX.Application.DTOs.Responses.MaterialResponses;

public sealed record GetMaterialResponse(
	int Id,
	string Name,
	string Code, 
	string Unit);
