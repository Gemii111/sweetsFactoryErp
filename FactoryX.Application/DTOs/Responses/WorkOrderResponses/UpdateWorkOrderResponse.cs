namespace FactoryX.Application.DTOs.Responses.WorkOrder;

public sealed record UpdateWorkOrderResponse(
	int Id,
	DateTime UpdatedAt,
	int ProductId,
	int MachineId,
	int Quantity,
	DateTime StartDate,
	DateTime EndDate,
	string Status,
	string? ProductName,
	string? MachineName);