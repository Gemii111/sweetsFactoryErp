namespace FactoryX.Application.DTOs.Requests.DowntimeRequests;

public sealed record InsertDowntimeRequest(int MachineId, DateTime StartTime, DateTime EndTime, string Reason);
