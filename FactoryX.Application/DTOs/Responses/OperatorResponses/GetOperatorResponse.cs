namespace FactoryX.Application.DTOs.Responses.OperatorResponses;

public sealed record GetOperatorResponse(
	int Id,
	string Name,
	string EmployeeNumber);
