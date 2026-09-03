namespace FactoryX.Application.DTOs.Responses.MachineResponses;

public record InsertMachineResponse(
	int Id,
	string Name,
	string Status,
	int Capacity);
