namespace FactoryX.Application.DTOs.Responses.Shift;

public sealed record DeleteShiftResponse(
	int Id,
	string Name,
	TimeSpan StartTime,
	TimeSpan EndTime);