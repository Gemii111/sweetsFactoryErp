namespace FactoryX.Application.DTOs.Responses.ProductResponses;

public sealed record GetAllProductResponse(
	int Id,
	string Name,
	string Code,
	string Description);
