using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum InspectionDataType
{
    Text = 1,
    Number = 2,
    Boolean = 3,
    PassFail = 4
}

public enum ItemEvaluationResult
{
    Pending = 0,
    Pass = 1,
    Fail = 2,
    Warning = 3
}

public class QualityTemplate : EntityBase
{
    public string Code { get; set; } = string.Empty; // e.g. "SESAME-QC-01"
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int? ProductCategoryId { get; set; }
    public int? ProductId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    // Navigation properties
    public ProductCategory? ProductCategory { get; set; }
    public Product? Product { get; set; }
    public ICollection<QualityTemplateItem> Items { get; set; } = new List<QualityTemplateItem>();
    public ICollection<QualityInspection> Inspections { get; set; } = new List<QualityInspection>();
}

public class QualityTemplateItem : EntityBase
{
    public int QualityTemplateId { get; set; }
    public string SpecificationName { get; set; } = string.Empty; // e.g. "وزن القطعة", "القوام والملمس", "المظهر واللون", "الطعم والرائحة"
    public string Description { get; set; } = string.Empty;
    public int Sequence { get; set; } = 1;
    public bool IsRequired { get; set; } = true;

    public InspectionDataType DataType { get; set; } = InspectionDataType.Number;

    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string? AllowedTextValues { get; set; } // Comma-separated allowed values (e.g. "طبيعي,ممتاز,ذهبي")
    public string Unit { get; set; } = string.Empty; // e.g. "G", "%", "C"
    public string? Notes { get; set; }

    // Navigation properties
    public QualityTemplate? QualityTemplate { get; set; }
}
