namespace FactoryX.Application.DTOs.Responses.MachineResponses;

public record GetMachineResponse
{
	public int Id { get; init; }
	public string Name { get; init; } = string.Empty;
	public string Status { get; init; } = string.Empty;
	public int Capacity { get; init; }
}
