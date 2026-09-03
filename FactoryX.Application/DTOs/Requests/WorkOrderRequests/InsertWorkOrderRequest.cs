namespace FactoryX.Application.DTOs.Requests.WorkOrderRequests;

public sealed record InsertWorkOrderRequest(
	int ProductId,
	int MachineId,
	int Quantity,
	DateTime StartDate,
	DateTime EndDate,
	string ProductName,
	string MachineName,
	string Status
);