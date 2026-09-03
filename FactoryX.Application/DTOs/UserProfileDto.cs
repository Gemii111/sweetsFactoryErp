namespace FactoryX.Application.DTOs;

public class UserProfileDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}