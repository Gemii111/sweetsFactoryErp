using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class Operator : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public ICollection<ProductionRecord>? ProductionRecords { get; set; }
}