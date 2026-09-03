namespace FactoryX.Application.DTOs.Requests.WorkOrderRequests;

public sealed record UpdateWorkOrderRequest(
	int Id,
	int ProductId,
	int MachineId,
	int Quantity,
	DateTime StartDate,
	DateTime EndDate,
	string Status,
	string? ProductName,
	string? MachineName
);
