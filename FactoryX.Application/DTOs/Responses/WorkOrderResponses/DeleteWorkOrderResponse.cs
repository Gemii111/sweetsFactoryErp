namespace FactoryX.Application.DTOs.Responses.WorkOrder;

public sealed record DeleteWorkOrderResponse(
	int Id,
	int ProductId,
	int MachineId,
	int Quantity,
	DateTime StartDate,
	DateTime EndDate,
	string Status,
	string? ProductName,
	string? MachineName);
