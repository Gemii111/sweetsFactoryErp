namespace FactoryX.Application.DTOs.Responses.Product;

public sealed record UpdateProductResponse(
	int Id,
	string Name,
	string Code,
	string Description);