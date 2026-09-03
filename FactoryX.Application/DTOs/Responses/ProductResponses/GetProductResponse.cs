namespace FactoryX.Application.DTOs.Responses.ProductResponses;

public sealed record GetProductResponse(
	int Id,
	string Name,
	string Code,
	string Description);
