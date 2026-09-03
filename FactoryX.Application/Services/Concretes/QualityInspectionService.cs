using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;

namespace FactoryX.Application.Services.Concretes;

public class QualityInspectionService : IQualityInspectionService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IQualityTemplateService _templateService;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateQualityInspectionRequest> _createValidator;
    private readonly IValidator<RejectInspectionRequest> _rejectValidator;
    private readonly IValidator<HoldInspectionRequest> _holdValidator;
    private readonly IValidator<ReinspectRequest> _reinspectValidator;

    public QualityInspectionService(
        IRepositoryManager repositoryManager,
        IQualityTemplateService templateService,
        IMapper mapper,
        IValidator<CreateQualityInspectionRequest> createValidator,
        IValidator<RejectInspectionRequest> rejectValidator,
        IValidator<HoldInspectionRequest> holdValidator,
        IValidator<ReinspectRequest> reinspectValidator)
    {
        _repositoryManager = repositoryManager;
        _templateService = templateService;
        _mapper = mapper;
        _createValidator = createValidator;
        _rejectValidator = rejectValidator;
        _holdValidator = holdValidator;
        _reinspectValidator = reinspectValidator;
    }

    public async Task<IEnumerable<QualityInspectionDto>> GetAllInspectionsAsync(
        QualityInspectionStatus? status = null,
        QualityDecision? decision = null,
        int? batchId = null,
        int? orderId = null,
        int? productId = null,
        int? inspectorId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var inspections = await _repositoryManager.QualityInspectionRepository.GetAllInspectionsWithDetailsAsync(
            status, decision, batchId, orderId, productId, inspectorId, fromDate, toDate, searchTerm);

        return _mapper.Map<IEnumerable<QualityInspectionDto>>(inspections);
    }

    public async Task<QualityInspectionDto?> GetInspectionByIdAsync(int id)
    {
        var inspection = await _repositoryManager.QualityInspectionRepository.GetInspectionWithDetailsAsync(id);
        return inspection == null ? null : _mapper.Map<QualityInspectionDto>(inspection);
    }

    public async Task<QualityInspectionDto?> GetInspectionByNumberAsync(string inspectionNumber)
    {
        var inspection = await _repositoryManager.QualityInspectionRepository.GetInspectionByNumberAsync(inspectionNumber);
        return inspection == null ? null : _mapper.Map<QualityInspectionDto>(inspection);
    }

    public async Task<QualityInspectionSummaryDto> GetSummaryAsync()
    {
        var all = await _repositoryManager.QualityInspectionRepository.GetAllInspectionsWithDetailsAsync();
        var list = all.ToList();

        return new QualityInspectionSummaryDto
        {
            TotalInspectionsCount = list.Count,
            PendingCount = list.Count(q => q.Status == QualityInspectionStatus.Pending),
            InProgressCount = list.Count(q => q.Status == QualityInspectionStatus.InProgress),
            ApprovedCount = list.Count(q => q.Status == QualityInspectionStatus.Approved),
            RejectedCount = list.Count(q => q.Status == QualityInspectionStatus.Rejected),
            HoldCount = list.Count(q => q.Status == QualityInspectionStatus.Hold)
        };
    }

    public async Task<string> GenerateInspectionNumberAsync(DateTime date)
    {
        var prefix = $"QC-{date:yyyyMMdd}-";
        var countToday = await _repositoryManager.QualityInspectionRepository.GetCountForDateAsync(date);
        var sequence = countToday + 1;

        var number = $"{prefix}{sequence:D4}";
        while (!await _repositoryManager.QualityInspectionRepository.IsInspectionNumberUniqueAsync(number))
        {
            sequence++;
            number = $"{prefix}{sequence:D4}";
        }

        return number;
    }

    public async Task<QualityInspectionDto> CreateInspectionAsync(CreateQualityInspectionRequest request, int userId)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(request.ProductionBatchId);
        if (batch == null)
        {
            throw new InvalidOperationException($"دفعة الإنتاج برقم #{request.ProductionBatchId} غير موجودة.");
        }

        // Determine template: explicitly supplied or product precedence lookup
        QualityTemplateDto? template = null;
        if (request.QualityTemplateId.HasValue && request.QualityTemplateId.Value > 0)
        {
            template = await _templateService.GetTemplateByIdAsync(request.QualityTemplateId.Value);
        }
        else
        {
            template = await _templateService.GetApplicableTemplateForProductAsync(batch.ProductId, batch.Product?.ProductCategoryId);
        }

        var inspectionNumber = await GenerateInspectionNumberAsync(request.InspectionDate);

        var inspection = new QualityInspection
        {
            InspectionNumber = inspectionNumber,
            Type = QualityInspectionType.ProductionBatch,
            Status = QualityInspectionStatus.InProgress,
            FinalDecision = QualityDecision.None,
            RecommendedDecision = QualityDecision.None,
            ProductionBatchId = batch.Id,
            WorkOrderId = batch.WorkOrderId,
            ProductId = batch.ProductId,
            QualityTemplateId = template?.Id,
            InspectorId = request.InspectorId > 0 ? request.InspectorId : userId,
            InspectionDate = request.InspectionDate,
            Notes = request.Notes?.Trim(),
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Populate items from template
        if (template != null && template.Items.Any())
        {
            foreach (var tItem in template.Items.OrderBy(i => i.Sequence))
            {
                inspection.Items.Add(new QualityInspectionItem
                {
                    QualityTemplateItemId = tItem.Id,
                    SpecificationName = tItem.SpecificationName,
                    Description = tItem.Description,
                    Sequence = tItem.Sequence,
                    IsRequired = tItem.IsRequired,
                    DataType = tItem.DataType,
                    MinValue = tItem.MinValue,
                    MaxValue = tItem.MaxValue,
                    TargetValue = tItem.TargetValue,
                    AllowedTextValues = tItem.AllowedTextValues,
                    Unit = tItem.Unit,
                    Result = ItemEvaluationResult.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _repositoryManager.QualityInspectionRepository.AddAsync(inspection);
        await _repositoryManager.SaveAsync();

        return (await GetInspectionByIdAsync(inspection.Id))!;
    }

    public async Task<QualityInspectionDto> RecordMeasurementsAsync(RecordInspectionMeasurementsRequest request, int userId)
    {
        var inspection = await _repositoryManager.QualityInspectionRepository.GetInspectionWithDetailsAsync(request.InspectionId, trackChanges: true);
        if (inspection == null)
        {
            throw new KeyNotFoundException($"سجل فحص الجودة برقم #{request.InspectionId} غير موجود.");
        }

        if (inspection.Status == QualityInspectionStatus.Approved ||
            inspection.Status == QualityInspectionStatus.Rejected ||
            inspection.Status == QualityInspectionStatus.Cancelled)
        {
            throw new InvalidOperationException($"لا يمكن تعديل قياسات فحص مكتمل أو ملغى في حالة '{inspection.Status}'.");
        }

        foreach (var input in request.Measurements)
        {
            var item = inspection.Items.FirstOrDefault(i => i.Id == input.ItemId);
            if (item == null) continue;

            item.ActualTextValue = input.ActualTextValue?.Trim();
            item.ActualNumericValue = input.ActualNumericValue;
            item.ActualBooleanValue = input.ActualBooleanValue;
            item.ActualPassFailValue = input.ActualPassFailValue?.Trim().ToUpper();
            item.InspectorNotes = input.InspectorNotes?.Trim();
            item.UpdatedAt = DateTime.UtcNow;

            // Auto evaluate this item
            item.Result = EvaluateItem(item);
        }

        // Calculate overall recommended decision
        inspection.RecommendedDecision = CalculateRecommendedDecision(inspection.Items);
        inspection.Status = QualityInspectionStatus.InProgress;
        inspection.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.QualityInspectionRepository.Update(inspection);
        await _repositoryManager.SaveAsync();

        return (await GetInspectionByIdAsync(inspection.Id))!;
    }

    private ItemEvaluationResult EvaluateItem(QualityInspectionItem item)
    {
        switch (item.DataType)
        {
            case InspectionDataType.Number:
                if (!item.ActualNumericValue.HasValue)
                {
                    return ItemEvaluationResult.Pending;
                }

                var val = item.ActualNumericValue.Value;
                if (item.MinValue.HasValue && item.MaxValue.HasValue)
                {
                    return (val >= item.MinValue.Value && val <= item.MaxValue.Value)
                        ? ItemEvaluationResult.Pass
                        : ItemEvaluationResult.Fail;
                }
                else if (item.MinValue.HasValue)
                {
                    return (val >= item.MinValue.Value)
                        ? ItemEvaluationResult.Pass
                        : ItemEvaluationResult.Fail;
                }
                else if (item.MaxValue.HasValue)
                {
                    return (val <= item.MaxValue.Value)
                        ? ItemEvaluationResult.Pass
                        : ItemEvaluationResult.Fail;
                }
                else if (item.TargetValue.HasValue)
                {
                    return (val == item.TargetValue.Value)
                        ? ItemEvaluationResult.Pass
                        : ItemEvaluationResult.Fail;
                }
                return ItemEvaluationResult.Pass;

            case InspectionDataType.PassFail:
                if (string.IsNullOrWhiteSpace(item.ActualPassFailValue))
                {
                    return ItemEvaluationResult.Pending;
                }
                return item.ActualPassFailValue.Equals("PASS", StringComparison.OrdinalIgnoreCase)
                    ? ItemEvaluationResult.Pass
                    : ItemEvaluationResult.Fail;

            case InspectionDataType.Boolean:
                if (!item.ActualBooleanValue.HasValue)
                {
                    return ItemEvaluationResult.Pending;
                }
                return item.ActualBooleanValue.Value
                    ? ItemEvaluationResult.Pass
                    : ItemEvaluationResult.Fail;

            case InspectionDataType.Text:
                if (string.IsNullOrWhiteSpace(item.ActualTextValue))
                {
                    return ItemEvaluationResult.Pending;
                }
                if (!string.IsNullOrWhiteSpace(item.AllowedTextValues))
                {
                    var allowed = item.AllowedTextValues.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim().ToLower())
                        .ToList();

                    return allowed.Contains(item.ActualTextValue.ToLower())
                        ? ItemEvaluationResult.Pass
                        : ItemEvaluationResult.Fail;
                }
                return ItemEvaluationResult.Pass;

            default:
                return ItemEvaluationResult.Pending;
        }
    }

    private QualityDecision CalculateRecommendedDecision(IEnumerable<QualityInspectionItem> items)
    {
        var itemList = items.ToList();
        if (!itemList.Any()) return QualityDecision.None;

        var requiredItems = itemList.Where(i => i.IsRequired).ToList();
        if (!requiredItems.Any())
        {
            requiredItems = itemList;
        }

        // If any required item failed -> Recommend Rejected
        if (requiredItems.Any(i => i.Result == ItemEvaluationResult.Fail))
        {
            return QualityDecision.Rejected;
        }

        // If any required item is still pending -> Cannot recommend Approved yet
        if (requiredItems.Any(i => i.Result == ItemEvaluationResult.Pending))
        {
            return QualityDecision.None;
        }

        // If all required items passed -> Recommend Approved
        if (requiredItems.All(i => i.Result == ItemEvaluationResult.Pass))
        {
            return QualityDecision.Approved;
        }

        return QualityDecision.None;
    }

    public async Task<QualityInspectionDto> SubmitInspectionAsync(int id, int userId)
    {
        var inspection = await _repositoryManager.QualityInspectionRepository.GetInspectionWithDetailsAsync(id, trackChanges: true);
        if (inspection == null)
        {
            throw new KeyNotFoundException($"سجل فحص الجودة برقم #{id} غير موجود.");
        }

        if (inspection.Status != QualityInspectionStatus.Draft && inspection.Status != QualityInspectionStatus.InProgress)
        {
            throw new InvalidOperationException($"لا يمكن تقديم الفحص للاعتماد وهو في حالة '{inspection.Status}'.");
        }

        inspection.Status = QualityInspectionStatus.Pending;
        inspection.SubmittedByUserId = userId;
        inspection.SubmittedAt = DateTime.UtcNow;
        inspection.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.QualityInspectionRepository.Update(inspection);
        await _repositoryManager.SaveAsync();

        return (await GetInspectionByIdAsync(inspection.Id))!;
    }

    public async Task<QualityInspectionDto> ApproveInspectionAsync(ApproveInspectionRequest request, int userId)
    {
        var inspection = await _repositoryManager.QualityInspectionRepository.GetInspectionWithDetailsAsync(request.InspectionId, trackChanges: true);
        if (inspection == null)
        {
            throw new KeyNotFoundException($"سجل فحص الجودة برقم #{request.InspectionId} غير موجود.");
        }

        if (inspection.Status == QualityInspectionStatus.Approved)
        {
            throw new InvalidOperationException("تم اعتماد هذا الفحص مسبقاً.");
        }

        if (inspection.Status == QualityInspectionStatus.Cancelled)
        {
            throw new InvalidOperationException("لا يمكن اعتماد فحص ملغى.");
        }

        // Validate required items
        var requiredItems = inspection.Items.Where(i => i.IsRequired).ToList();
        if (requiredItems.Any(i => i.Result == ItemEvaluationResult.Fail))
        {
            var failedNames = string.Join(", ", requiredItems.Where(i => i.Result == ItemEvaluationResult.Fail).Select(i => i.SpecificationName));
            throw new InvalidOperationException($"لا يمكن اعتماد دفعة الإنتاج لوجود معايير جودة إلزامية راسبة (FAIL): [{failedNames}].");
        }

        if (requiredItems.Any(i => i.Result == ItemEvaluationResult.Pending))
        {
            var pendingNames = string.Join(", ", requiredItems.Where(i => i.Result == ItemEvaluationResult.Pending).Select(i => i.SpecificationName));
            throw new InvalidOperationException($"يجب تسجيل نتائج جميع المعايير الإلزامية قبل الاعتماد. المعايير المعلقة: [{pendingNames}].");
        }

        inspection.Status = QualityInspectionStatus.Approved;
        inspection.FinalDecision = QualityDecision.Approved;
        inspection.ApprovalNotes = request.ApprovalNotes?.Trim();
        inspection.CompletedByUserId = userId;
        inspection.CompletedAt = DateTime.UtcNow;
        inspection.DecisionByUserId = userId;
        inspection.DecisionAt = DateTime.UtcNow;
        inspection.UpdatedAt = DateTime.UtcNow;

        // Update ProductionBatch QualityStatus
        if (inspection.ProductionBatchId.HasValue)
        {
            var batch = await _repositoryManager.ProductionBatchRepository.GetByIdAsync(inspection.ProductionBatchId.Value, trackChanges: true);
            if (batch != null)
            {
                batch.QualityStatus = "Approved";
                batch.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.ProductionBatchRepository.Update(batch);
            }
        }

        _repositoryManager.QualityInspectionRepository.Update(inspection);
        await _repositoryManager.SaveAsync();

        return (await GetInspectionByIdAsync(inspection.Id))!;
    }

    public async Task<QualityInspectionDto> RejectInspectionAsync(RejectInspectionRequest request, int userId)
    {
        var validationResult = await _rejectValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var inspection = await _repositoryManager.QualityInspectionRepository.GetInspectionWithDetailsAsync(request.InspectionId, trackChanges: true);
        if (inspection == null)
        {
            throw new KeyNotFoundException($"سجل فحص الجودة برقم #{request.InspectionId} غير موجود.");
        }

        if (inspection.Status == QualityInspectionStatus.Approved)
        {
            throw new InvalidOperationException("لا يمكن تغيير قرار فحص معتمد بالفعل إلى رفض. يتطلب ذلك عملية إعادة فحص (Re-inspection).");
        }

        if (inspection.Status == QualityInspectionStatus.Cancelled)
        {
            throw new InvalidOperationException("لا يمكن رفض فحص ملغى.");
        }

        inspection.Status = QualityInspectionStatus.Rejected;
        inspection.FinalDecision = QualityDecision.Rejected;
        inspection.RejectionReason = request.RejectionReason.Trim();
        inspection.CompletedByUserId = userId;
        inspection.CompletedAt = DateTime.UtcNow;
        inspection.DecisionByUserId = userId;
        inspection.DecisionAt = DateTime.UtcNow;
        inspection.UpdatedAt = DateTime.UtcNow;

        // Update ProductionBatch QualityStatus
        if (inspection.ProductionBatchId.HasValue)
        {
            var batch = await _repositoryManager.ProductionBatchRepository.GetByIdAsync(inspection.ProductionBatchId.Value, trackChanges: true);
            if (batch != null)
            {
                batch.QualityStatus = "Rejected";
                batch.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.ProductionBatchRepository.Update(batch);
            }
        }

        _repositoryManager.QualityInspectionRepository.Update(inspection);
        await _repositoryManager.SaveAsync();

        return (await GetInspectionByIdAsync(inspection.Id))!;
    }

    public async Task<QualityInspectionDto> HoldInspectionAsync(HoldInspectionRequest request, int userId)
    {
        var validationResult = await _holdValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var inspection = await _repositoryManager.QualityInspectionRepository.GetInspectionWithDetailsAsync(request.InspectionId, trackChanges: true);
        if (inspection == null)
        {
            throw new KeyNotFoundException($"سجل فحص الجودة برقم #{request.InspectionId} غير موجود.");
        }

        if (inspection.Status == QualityInspectionStatus.Approved)
        {
            throw new InvalidOperationException("لا يمكن تعليق فحص معتمد بالفعل.");
        }

        inspection.Status = QualityInspectionStatus.Hold;
        inspection.FinalDecision = QualityDecision.Hold;
        inspection.HoldReason = request.HoldReason.Trim();
        inspection.DecisionByUserId = userId;
        inspection.DecisionAt = DateTime.UtcNow;
        inspection.UpdatedAt = DateTime.UtcNow;

        // Update ProductionBatch QualityStatus
        if (inspection.ProductionBatchId.HasValue)
        {
            var batch = await _repositoryManager.ProductionBatchRepository.GetByIdAsync(inspection.ProductionBatchId.Value, trackChanges: true);
            if (batch != null)
            {
                batch.QualityStatus = "Hold";
                batch.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.ProductionBatchRepository.Update(batch);
            }
        }

        _repositoryManager.QualityInspectionRepository.Update(inspection);
        await _repositoryManager.SaveAsync();

        return (await GetInspectionByIdAsync(inspection.Id))!;
    }

    public async Task<QualityInspectionDto> ReinspectAsync(ReinspectRequest request, int userId)
    {
        var validationResult = await _reinspectValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var prevInspection = await _repositoryManager.QualityInspectionRepository.GetInspectionWithDetailsAsync(request.PreviousInspectionId);
        if (prevInspection == null)
        {
            throw new KeyNotFoundException($"الفحص السابق برقم #{request.PreviousInspectionId} غير موجود.");
        }

        if (!prevInspection.ProductionBatchId.HasValue)
        {
            throw new InvalidOperationException("الفحص السابق غير مرتبط بدفعة إنتاج.");
        }

        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(prevInspection.ProductionBatchId.Value);
        if (batch == null)
        {
            throw new InvalidOperationException($"دفعة الإنتاج #{prevInspection.ProductionBatchId} غير موجودة.");
        }

        QualityTemplateDto? template = null;
        if (request.QualityTemplateId.HasValue && request.QualityTemplateId.Value > 0)
        {
            template = await _templateService.GetTemplateByIdAsync(request.QualityTemplateId.Value);
        }
        else if (prevInspection.QualityTemplateId.HasValue)
        {
            template = await _templateService.GetTemplateByIdAsync(prevInspection.QualityTemplateId.Value);
        }
        else
        {
            template = await _templateService.GetApplicableTemplateForProductAsync(batch.ProductId, batch.Product?.ProductCategoryId);
        }

        var inspectionNumber = await GenerateInspectionNumberAsync(DateTime.UtcNow);

        var newInspection = new QualityInspection
        {
            InspectionNumber = inspectionNumber,
            Type = QualityInspectionType.ProductionBatch,
            Status = QualityInspectionStatus.InProgress,
            FinalDecision = QualityDecision.None,
            RecommendedDecision = QualityDecision.None,
            ProductionBatchId = batch.Id,
            WorkOrderId = batch.WorkOrderId,
            ProductId = batch.ProductId,
            QualityTemplateId = template?.Id,
            InspectorId = request.InspectorId > 0 ? request.InspectorId : userId,
            InspectionDate = DateTime.UtcNow,
            PreviousInspectionId = prevInspection.Id,
            ReinspectionReason = request.ReinspectionReason.Trim(),
            Notes = request.Notes?.Trim(),
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (template != null && template.Items.Any())
        {
            foreach (var tItem in template.Items.OrderBy(i => i.Sequence))
            {
                newInspection.Items.Add(new QualityInspectionItem
                {
                    QualityTemplateItemId = tItem.Id,
                    SpecificationName = tItem.SpecificationName,
                    Description = tItem.Description,
                    Sequence = tItem.Sequence,
                    IsRequired = tItem.IsRequired,
                    DataType = tItem.DataType,
                    MinValue = tItem.MinValue,
                    MaxValue = tItem.MaxValue,
                    TargetValue = tItem.TargetValue,
                    AllowedTextValues = tItem.AllowedTextValues,
                    Unit = tItem.Unit,
                    Result = ItemEvaluationResult.Pending,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        await _repositoryManager.QualityInspectionRepository.AddAsync(newInspection);
        await _repositoryManager.SaveAsync();

        return (await GetInspectionByIdAsync(newInspection.Id))!;
    }

    public async Task<QualityInspectionDto> CancelInspectionAsync(int id, int userId, string? reason = null)
    {
        var inspection = await _repositoryManager.QualityInspectionRepository.GetInspectionWithDetailsAsync(id, trackChanges: true);
        if (inspection == null)
        {
            throw new KeyNotFoundException($"سجل فحص الجودة برقم #{id} غير موجود.");
        }

        if (inspection.Status == QualityInspectionStatus.Approved)
        {
            throw new InvalidOperationException("لا يمكن إلغاء فحص معتمد.");
        }

        if (inspection.Status == QualityInspectionStatus.Cancelled)
        {
            throw new InvalidOperationException("الفحص ملغى بالفعل.");
        }

        inspection.Status = QualityInspectionStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            inspection.Notes = string.IsNullOrWhiteSpace(inspection.Notes)
                ? $"سبب الإلغاء: {reason}"
                : $"{inspection.Notes} | سبب الإلغاء: {reason}";
        }
        inspection.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.QualityInspectionRepository.Update(inspection);
        await _repositoryManager.SaveAsync();

        return (await GetInspectionByIdAsync(inspection.Id))!;
    }

    public async Task<IEnumerable<QualityInspectionDto>> GetInspectionHistoryForBatchAsync(int batchId)
    {
        var list = await _repositoryManager.QualityInspectionRepository.GetInspectionHistoryForBatchAsync(batchId);
        return _mapper.Map<IEnumerable<QualityInspectionDto>>(list);
    }
}
