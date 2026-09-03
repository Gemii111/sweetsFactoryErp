namespace FactoryX.Application.DTOs.Responses.ShiftResponses;

public sealed record GetShiftResponse(
	int Id,
	string Name,
	TimeSpan StartTime,
	TimeSpan EndTime);
