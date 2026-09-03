namespace FactoryX.Application.DTOs.Requests.ProductRequests;

public sealed record UpdateProductRequest(
	int Id,
	string Name,
	string Code,
	string Description);
