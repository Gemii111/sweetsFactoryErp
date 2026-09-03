namespace FactoryX.Application.DTOs.Responses.DowntimeResponses;

public sealed record InsertDowntimeResponse(
    int Id,
    int MachineId,
    DateTime StartTime,
    DateTime EndTime,
    string Reason
);
