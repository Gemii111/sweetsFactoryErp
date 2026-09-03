namespace FactoryX.Application.DTOs.Responses.Operator;

public sealed record DeleteOperatorResponse(
	int Id,
	string Name,
	string EmployeeNumber);
