namespace FactoryX.Application.DTOs.Responses.Operator;

public sealed record UpdateOperatorResponse(
	int Id,
	string Name,
	string EmployeeNumber);
