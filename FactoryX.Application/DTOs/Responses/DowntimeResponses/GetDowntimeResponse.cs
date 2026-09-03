namespace FactoryX.Application.DTOs.Responses.DowntimeResponses;

public sealed record GetDowntimeResponse(
	int Id,
	int MachineId,
	DateTime StartTime,
	DateTime EndTime,
	string Reason
);
