namespace FactoryX.Application.DTOs.Requests.ShiftRequests;

public sealed record UpdateShiftRequest(int Id, string Name, TimeSpan StartTime, TimeSpan EndTime);
