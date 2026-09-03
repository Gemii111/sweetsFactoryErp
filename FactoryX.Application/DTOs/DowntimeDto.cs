namespace FactoryX.Application.DTOs;

public class DowntimeDto
{
    public int Id { get; set; }
    public int MachineId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
}