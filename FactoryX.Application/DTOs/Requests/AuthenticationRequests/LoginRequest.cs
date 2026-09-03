namespace FactoryX.Application.DTOs.Requests.AuthenticationRequests;

public sealed record class LoginRequest(
	string Username,
	string Password);
