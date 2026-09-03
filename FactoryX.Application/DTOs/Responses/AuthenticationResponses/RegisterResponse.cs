namespace FactoryX.Application.DTOs.Responses.AuthenticationResponses;

public sealed record RegisterResponse(
	int Id, 
	string Username, 
	string Role);
