namespace FactoryX.Application.DTOs.Responses.Product;

public sealed record InsertProductResponse(
	int Id,
	string Name,
	string Code,
	string Description);
