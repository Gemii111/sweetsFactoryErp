namespace FactoryX.Application.DTOs.Responses.MachineResponses;

public record DeleteMachineResponse(
	int Id,
	string Name,
	string Status,
	int Capacity
);
