namespace FactoryX.Application.DTOs.Responses.AuthenticationResponses;

public sealed record LoginResponse(
	int Id,
	string Username,
	string Role
);
