namespace FactoryX.Application.DTOs.Requests.ShiftRequests;

public sealed record InsertShiftRequest(int Id, string Name, TimeSpan StartTime, TimeSpan EndTime);
