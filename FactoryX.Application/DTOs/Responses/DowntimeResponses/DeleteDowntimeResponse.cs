namespace FactoryX.Application.DTOs.Responses.DowntimeResponses;

public sealed record DeleteDowntimeResponse(
    int Id,
	int MachineId,
	DateTime StartTime,
	DateTime EndTime,
	string Reason
);
