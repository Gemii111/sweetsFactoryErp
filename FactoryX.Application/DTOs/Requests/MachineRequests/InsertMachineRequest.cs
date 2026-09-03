namespace FactoryX.Application.DTOs.Requests.MachineRequests;

public sealed record InsertMachineRequest(
    string Name,
    string Status,
    int Capacity
);