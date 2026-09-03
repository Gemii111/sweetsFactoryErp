using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class QualityInspectionDto
{
    public int Id { get; set; }
    public string InspectionNumber { get; set; } = string.Empty;
    public QualityInspectionType Type { get; set; }
    public string TypeName { get; set; } = string.Empty;

    public QualityInspectionStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;

    public QualityDecision FinalDecision { get; set; }
    public string FinalDecisionName { get; set; } = string.Empty;

    public QualityDecision RecommendedDecision { get; set; }
    public string RecommendedDecisionName { get; set; } = string.Empty;

    // Linkages
    public int? ProductionBatchId { get; set; }
    public string? ProductionBatchNumber { get; set; }
    public decimal BatchPlannedQuantity { get; set; }
    public decimal BatchActualQuantity { get; set; }
    public string? BatchOutputUnit { get; set; }

    public int? WorkOrderId { get; set; }
    public string? WorkOrderNumber { get; set; }

    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }

    public int? QualityTemplateId { get; set; }
    public string? QualityTemplateCode { get; set; }
    public string? QualityTemplateName { get; set; }

    public int? InspectorId { get; set; }
    public string? InspectorName { get; set; }
    public DateTime InspectionDate { get; set; }

    // Decisions & Notes
    public string? Notes { get; set; }
    public string? ApprovalNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? HoldReason { get; set; }

    // Reinspection
    public int? PreviousInspectionId { get; set; }
    public string? PreviousInspectionNumber { get; set; }
    public string? ReinspectionReason { get; set; }
    public int ReinspectionsCount { get; set; }

    // Audit
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? SubmittedByUserId { get; set; }
    public string? SubmittedByUserName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? CompletedByUserId { get; set; }
    public string? CompletedByUserName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DecisionByUserId { get; set; }
    public string? DecisionByUserName { get; set; }
    public DateTime? DecisionAt { get; set; }

    public List<QualityInspectionItemDto> Items { get; set; } = new();
}

public class QualityInspectionItemDto
{
    public int Id { get; set; }
    public int QualityInspectionId { get; set; }
    public int? QualityTemplateItemId { get; set; }

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

    public string? ActualTextValue { get; set; }
    public decimal? ActualNumericValue { get; set; }
    public bool? ActualBooleanValue { get; set; }
    public string? ActualPassFailValue { get; set; }

    public ItemEvaluationResult Result { get; set; } = ItemEvaluationResult.Pending;
    public string ResultName { get; set; } = string.Empty;
    public string? InspectorNotes { get; set; }
}

public class CreateQualityInspectionRequest
{
    public int ProductionBatchId { get; set; }
    public int? QualityTemplateId { get; set; }
    public int? InspectorId { get; set; }
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

public class UpdateQualityInspectionRequest
{
    public int Id { get; set; }
    public int? InspectorId { get; set; }
    public DateTime InspectionDate { get; set; }
    public string? Notes { get; set; }
}

public class RecordInspectionMeasurementsRequest
{
    public int InspectionId { get; set; }
    public List<InspectionMeasurementItemInput> Measurements { get; set; } = new();
}

public class InspectionMeasurementItemInput
{
    public int ItemId { get; set; }
    public string? ActualTextValue { get; set; }
    public decimal? ActualNumericValue { get; set; }
    public bool? ActualBooleanValue { get; set; }
    public string? ActualPassFailValue { get; set; }
    public string? InspectorNotes { get; set; }
}

public class ApproveInspectionRequest
{
    public int InspectionId { get; set; }
    public string? ApprovalNotes { get; set; }
}

public class RejectInspectionRequest
{
    public int InspectionId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

public class HoldInspectionRequest
{
    public int InspectionId { get; set; }
    public string HoldReason { get; set; } = string.Empty;
}

public class ReinspectRequest
{
    public int PreviousInspectionId { get; set; }
    public string ReinspectionReason { get; set; } = string.Empty;
    public int? QualityTemplateId { get; set; }
    public int? InspectorId { get; set; }
    public string? Notes { get; set; }
}

public class QualityInspectionSummaryDto
{
    public int TotalInspectionsCount { get; set; }
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int ApprovedCount { get; set; }
    public int RejectedCount { get; set; }
    public int HoldCount { get; set; }
}

public class ReleaseGateResultDto
{
    public int BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public bool IsAllowed { get; set; }
    public string Status { get; set; } = string.Empty; // "ALLOWED" or "BLOCKED"
    public string Reason { get; set; } = string.Empty;
    public string? InspectionNumber { get; set; }
    public DateTime? DecisionDate { get; set; }
    public string? InspectorName { get; set; }
}
