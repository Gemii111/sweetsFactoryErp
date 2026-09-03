namespace FactoryX.Application.DTOs.Responses.Shift;

public sealed record UpdateShiftResponse(
	int Id,
	string Name,
	TimeSpan StartTime,
	TimeSpan EndTime);
