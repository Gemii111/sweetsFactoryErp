using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class Shift : EntityBase
{
    public string Name { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}