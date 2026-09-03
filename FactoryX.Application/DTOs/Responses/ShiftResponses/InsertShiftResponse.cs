namespace FactoryX.Application.DTOs.Responses.Shift;

public sealed record InsertShiftResponse(
	int Id,
	string Name,
	TimeSpan StartTime,
	TimeSpan EndTime);
