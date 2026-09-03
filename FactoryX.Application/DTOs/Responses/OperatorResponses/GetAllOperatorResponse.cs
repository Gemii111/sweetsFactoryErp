namespace FactoryX.Application.DTOs.Responses.OperatorResponses;

public sealed record GetAllOperatorResponse(
	int Id,
	string Name,
	string EmployeeNumber
);
