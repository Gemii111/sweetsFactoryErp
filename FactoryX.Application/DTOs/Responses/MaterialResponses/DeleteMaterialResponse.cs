namespace FactoryX.Application.DTOs.Responses.MaterialResponses;

public sealed record DeleteMaterialResponse(
	int Id,
	string Name,
	string Code, 
	string Unit);
