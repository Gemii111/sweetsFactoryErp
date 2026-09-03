namespace FactoryX.Application.DTOs;

public class WorkOrderDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int MachineId { get; set; }
    public int Quantity { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ProductName { get; set; }
    public string? MachineName { get; set; }
}