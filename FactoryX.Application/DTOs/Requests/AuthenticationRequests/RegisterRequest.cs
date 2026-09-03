namespace FactoryX.Application.DTOs.Requests.AuthenticationRequests;

public sealed record class RegisterRequest(
	string Username,
	string Password,
	string ConfirmPassword,
	string Role);
