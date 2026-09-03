namespace FactoryX.Application.DTOs.Requests.OperatorRequests;

public sealed record UpdateOperatorRequest(
    int Id,
    string Name,
    string EmployeeNumber
);