namespace FactoryX.Application.DTOs.Requests.DowntimeRequests;

public sealed record UpdateDowntimeRequest(DateTime StartTime, DateTime EndTime, string Reason);
