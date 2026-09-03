namespace FactoryX.Application.DTOs.Responses.MaterialResponses;

public record InsertMaterialResponse(
	int Id,
	string Name,
	string Code,
	string Unit);
