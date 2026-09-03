using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class Machine : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public ICollection<WorkOrder>? WorkOrders { get; set; }
    public ICollection<Downtime>? Downtimes { get; set; }
}