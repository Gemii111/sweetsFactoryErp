using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class QualityTemplateDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int? ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
    public int ItemsCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public List<QualityTemplateItemDto> Items { get; set; } = new();
}

public class QualityTemplateItemDto
{
    public int Id { get; set; }
    public int QualityTemplateId { get; set; }
    public string SpecificationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Sequence { get; set; } = 1;
    public bool IsRequired { get; set; } = true;

    public InspectionDataType DataType { get; set; } = InspectionDataType.Number;
    public string DataTypeName { get; set; } = string.Empty;

    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string? AllowedTextValues { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class CreateQualityTemplateRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int? ProductCategoryId { get; set; }
    public int? ProductId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public List<CreateQualityTemplateItemRequest> Items { get; set; } = new();
}

public class CreateQualityTemplateItemRequest
{
    public string SpecificationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Sequence { get; set; } = 1;
    public bool IsRequired { get; set; } = true;

    public InspectionDataType DataType { get; set; } = InspectionDataType.Number;

    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string? AllowedTextValues { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class UpdateQualityTemplateRequest
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public int? ProductCategoryId { get; set; }
    public int? ProductId { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public List<CreateQualityTemplateItemRequest> Items { get; set; } = new();
}
