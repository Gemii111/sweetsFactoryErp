using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class QualityGateService : IQualityGateService
{
    private readonly IRepositoryManager _repositoryManager;

    public QualityGateService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public async Task<ReleaseGateResultDto> CanReleaseBatchAsync(int batchId)
    {
        var batch = await _repositoryManager.ProductionBatchRepository.GetByIdAsync(batchId);
        if (batch == null)
        {
            return new ReleaseGateResultDto
            {
                BatchId = batchId,
                IsAllowed = false,
                Status = "BLOCKED",
                Reason = $"دفعة الإنتاج برقم #{batchId} غير موجودة."
            };
        }

        var inspections = await _repositoryManager.QualityInspectionRepository.GetInspectionHistoryForBatchAsync(batchId);
        var latestInspection = inspections.FirstOrDefault();

        if (latestInspection == null)
        {
            return new ReleaseGateResultDto
            {
                BatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                IsAllowed = false,
                Status = "BLOCKED",
                Reason = "لا يوجد أي فحص جودة مسجل لدفعة الإنتاج (No QC Inspection Recorded)."
            };
        }

        if (latestInspection.Status == QualityInspectionStatus.Draft ||
            latestInspection.Status == QualityInspectionStatus.InProgress ||
            latestInspection.Status == QualityInspectionStatus.Pending)
        {
            return new ReleaseGateResultDto
            {
                BatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                IsAllowed = false,
                Status = "BLOCKED",
                Reason = $"فحص الجودة برقم [{latestInspection.InspectionNumber}] قيد التنفيذ أو معلق للمراجعة ولم يتم اتخاذ القرار النهائي بعد.",
                InspectionNumber = latestInspection.InspectionNumber,
                DecisionDate = null,
                InspectorName = latestInspection.Inspector?.FullName ?? latestInspection.Inspector?.Username
            };
        }

        if (latestInspection.Status == QualityInspectionStatus.Hold || latestInspection.FinalDecision == QualityDecision.Hold)
        {
            return new ReleaseGateResultDto
            {
                BatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                IsAllowed = false,
                Status = "BLOCKED",
                Reason = $"دفعة الإنتاج محتجزة (HOLD) بقرار الجودة رقم [{latestInspection.InspectionNumber}]. سبب الاحتجاز: {latestInspection.HoldReason}",
                InspectionNumber = latestInspection.InspectionNumber,
                DecisionDate = latestInspection.DecisionAt,
                InspectorName = latestInspection.DecisionByUser?.FullName ?? latestInspection.Inspector?.FullName
            };
        }

        if (latestInspection.Status == QualityInspectionStatus.Rejected || latestInspection.FinalDecision == QualityDecision.Rejected)
        {
            return new ReleaseGateResultDto
            {
                BatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                IsAllowed = false,
                Status = "BLOCKED",
                Reason = $"دفعة الإنتاج مرفوضة (REJECTED) بقرار الجودة رقم [{latestInspection.InspectionNumber}]. سبب الرفض: {latestInspection.RejectionReason}",
                InspectionNumber = latestInspection.InspectionNumber,
                DecisionDate = latestInspection.DecisionAt,
                InspectorName = latestInspection.DecisionByUser?.FullName ?? latestInspection.Inspector?.FullName
            };
        }

        if (latestInspection.Status == QualityInspectionStatus.Approved && latestInspection.FinalDecision == QualityDecision.Approved)
        {
            // Verify no required item is failed or pending
            var hasFailingRequiredItem = latestInspection.Items.Any(i => i.IsRequired && i.Result == ItemEvaluationResult.Fail);
            if (hasFailingRequiredItem)
            {
                return new ReleaseGateResultDto
                {
                    BatchId = batch.Id,
                    BatchNumber = batch.BatchNumber,
                    IsAllowed = false,
                    Status = "BLOCKED",
                    Reason = $"يوجد معيار جودة إلزامي راسب (FAIL) في فحص الجودة [{latestInspection.InspectionNumber}].",
                    InspectionNumber = latestInspection.InspectionNumber
                };
            }

            return new ReleaseGateResultDto
            {
                BatchId = batch.Id,
                BatchNumber = batch.BatchNumber,
                IsAllowed = true,
                Status = "ALLOWED",
                Reason = $"دفعة الإنتاج مطابقة ومعتمدة للإفراج بموجب فحص الجودة المعتمد [{latestInspection.InspectionNumber}].",
                InspectionNumber = latestInspection.InspectionNumber,
                DecisionDate = latestInspection.DecisionAt,
                InspectorName = latestInspection.DecisionByUser?.FullName ?? latestInspection.Inspector?.FullName
            };
        }

        return new ReleaseGateResultDto
        {
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            IsAllowed = false,
            Status = "BLOCKED",
            Reason = $"حالة فحص الجودة غير مطابقة للإفراج ({latestInspection.Status}).",
            InspectionNumber = latestInspection.InspectionNumber
        };
    }
}
