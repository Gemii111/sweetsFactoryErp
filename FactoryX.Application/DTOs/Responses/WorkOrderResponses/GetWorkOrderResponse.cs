namespace FactoryX.Application.DTOs.Responses.WorkOrderResponses;

public sealed record GetWorkOrderResponse(
	int Id,
	DateTime CreatedAt,
	DateTime UpdatedAt,
	int ProductId,
	int MachineId,
	int Quantity,
	DateTime StartDate,
	DateTime EndDate,
	string Status,
	string? ProductName,
	string? MachineName);
