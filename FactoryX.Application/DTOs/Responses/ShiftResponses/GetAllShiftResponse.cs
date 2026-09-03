namespace FactoryX.Application.DTOs.Responses.ShiftResponses;

public sealed record GetAllShiftResponse(
	int Id,
	string Name,
	TimeSpan StartTime,
	TimeSpan EndTime);
