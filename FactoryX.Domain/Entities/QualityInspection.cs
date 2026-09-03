using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum QualityInspectionType
{
    RawMaterial = 1,
    InProcess = 2,
    ProductionBatch = 3
}

public enum QualityInspectionStatus
{
    Draft = 1,
    Pending = 2,
    InProgress = 3,
    Approved = 4,
    Rejected = 5,
    Hold = 6,
    Cancelled = 7
}

public enum QualityDecision
{
    None = 0,
    Approved = 1,
    Rejected = 2,
    Hold = 3
}

public class QualityInspection : EntityBase
{
    public string InspectionNumber { get; set; } = string.Empty; // e.g. "QC-20260830-0001"
    public QualityInspectionType Type { get; set; } = QualityInspectionType.ProductionBatch;
    public QualityInspectionStatus Status { get; set; } = QualityInspectionStatus.Draft;
    public QualityDecision FinalDecision { get; set; } = QualityDecision.None;
    public QualityDecision RecommendedDecision { get; set; } = QualityDecision.None;

    // Linkages
    public int? ProductionBatchId { get; set; }
    public int? WorkOrderId { get; set; }
    public int? ProductId { get; set; }
    public int? MaterialId { get; set; }
    public int? SupplierId { get; set; }
    public int? QualityTemplateId { get; set; }

    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
    public int? InspectorId { get; set; }

    // Legacy and Raw Material inspection fields
    public string SupplierBatchNumber { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public bool WeightCheckPassed { get; set; } = true;
    public bool AppearanceCheckPassed { get; set; } = true;
    public bool TasteCheckPassed { get; set; } = true;
    public bool PackagingCheckPassed { get; set; } = true;

    // Notes & Decisions
    public string? Notes { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public string? ApprovalNotes { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
    public string? HoldReason { get; set; }

    // Reinspection Linkage
    public int? PreviousInspectionId { get; set; }
    public string? ReinspectionReason { get; set; }

    // Audit Trail
    public int? CreatedByUserId { get; set; }
    public int? SubmittedByUserId { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public int? CompletedByUserId { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? DecisionByUserId { get; set; }
    public DateTime? DecisionAt { get; set; }

    // Navigation properties
    public ProductionBatch? ProductionBatch { get; set; }
    public WorkOrder? WorkOrder { get; set; }
    public Product? Product { get; set; }
    public Material? Material { get; set; }
    public Supplier? Supplier { get; set; }
    public QualityTemplate? QualityTemplate { get; set; }
    public User? Inspector { get; set; }
    public User? CreatedByUser { get; set; }
    public User? SubmittedByUser { get; set; }
    public User? CompletedByUser { get; set; }
    public User? DecisionByUser { get; set; }
    public QualityInspection? PreviousInspection { get; set; }
    public ICollection<QualityInspection> Reinspections { get; set; } = new List<QualityInspection>();

    public ICollection<QualityInspectionItem> Items { get; set; } = new List<QualityInspectionItem>();
}

public class QualityInspectionItem : EntityBase
{
    public int QualityInspectionId { get; set; }
    public int? QualityTemplateItemId { get; set; }

    public string SpecificationName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Sequence { get; set; } = 1;
    public bool IsRequired { get; set; } = true;

    public InspectionDataType DataType { get; set; } = InspectionDataType.Number;

    // Standard Tolerances
    public decimal? MinValue { get; set; }
    public decimal? MaxValue { get; set; }
    public decimal? TargetValue { get; set; }
    public string? AllowedTextValues { get; set; }
    public string Unit { get; set; } = string.Empty;

    // Actual Measurement Results
    public string? ActualTextValue { get; set; }
    public decimal? ActualNumericValue { get; set; }
    public bool? ActualBooleanValue { get; set; }
    public string? ActualPassFailValue { get; set; } // "PASS" or "FAIL"

    public ItemEvaluationResult Result { get; set; } = ItemEvaluationResult.Pending;
    public string? InspectorNotes { get; set; }

    // Navigation properties
    public QualityInspection? QualityInspection { get; set; }
    public QualityTemplateItem? QualityTemplateItem { get; set; }
}
