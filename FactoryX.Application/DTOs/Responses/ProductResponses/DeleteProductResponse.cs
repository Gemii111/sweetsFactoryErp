namespace FactoryX.Application.DTOs.Responses.Product;

public sealed record DeleteProductResponse(
	int Id,
	string Name,
	string Code,
	string Description);
