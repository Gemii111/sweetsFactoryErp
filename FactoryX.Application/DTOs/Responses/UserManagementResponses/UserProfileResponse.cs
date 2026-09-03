namespace FactoryX.Application.DTOs.Responses.UserManagementResponses;

public sealed record UserProfileResponse(
	int Id,
	string UserName = "",
	string? Email = "",
	string? FullName = "");
