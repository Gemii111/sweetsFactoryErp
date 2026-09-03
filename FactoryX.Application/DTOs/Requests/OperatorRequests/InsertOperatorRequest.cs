namespace FactoryX.Application.DTOs.Requests.OperatorRequests;

public sealed record InsertOperatorRequest(
    string Name,
    string EmployeeNumber
);