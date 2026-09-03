namespace FactoryX.Application.DTOs.Responses.MachineResponses;

public record UpdateMachineResponse(
	int Id,
	string Name,
	string Status, 
	int Capacity);

