namespace FactoryX.Application.DTOs.Responses.Operator;

public sealed record InsertOperatorResponse(
	int Id,
	string Name,
	string EmployeeNumber);