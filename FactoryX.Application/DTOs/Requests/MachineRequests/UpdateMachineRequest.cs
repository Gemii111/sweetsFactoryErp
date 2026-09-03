namespace FactoryX.Application.DTOs.Requests.MachineRequests;

public sealed record UpdateMachineRequest(
    int Id,
    string Name,
    string Status,
    int Capacity
);