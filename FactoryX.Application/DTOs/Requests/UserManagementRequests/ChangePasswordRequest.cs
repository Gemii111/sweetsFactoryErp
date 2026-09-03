namespace FactoryX.Application.DTOs.Requests.UserManagementRequests;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword, string ConfirmPassword);
