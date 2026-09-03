using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class Downtime : EntityBase
{
    public int MachineId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Machine? Machine { get; set; }
}