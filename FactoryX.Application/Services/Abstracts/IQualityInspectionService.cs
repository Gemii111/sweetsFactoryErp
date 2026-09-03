using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IQualityInspectionService
{
    Task<IEnumerable<QualityInspectionDto>> GetAllInspectionsAsync(
        QualityInspectionStatus? status = null,
        QualityDecision? decision = null,
        int? batchId = null,
        int? orderId = null,
        int? productId = null,
        int? inspectorId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<QualityInspectionDto?> GetInspectionByIdAsync(int id);
    Task<QualityInspectionDto?> GetInspectionByNumberAsync(string inspectionNumber);
    Task<QualityInspectionSummaryDto> GetSummaryAsync();

    Task<QualityInspectionDto> CreateInspectionAsync(CreateQualityInspectionRequest request, int userId);
    Task<QualityInspectionDto> RecordMeasurementsAsync(RecordInspectionMeasurementsRequest request, int userId);
    Task<QualityInspectionDto> SubmitInspectionAsync(int id, int userId);
    Task<QualityInspectionDto> ApproveInspectionAsync(ApproveInspectionRequest request, int userId);
    Task<QualityInspectionDto> RejectInspectionAsync(RejectInspectionRequest request, int userId);
    Task<QualityInspectionDto> HoldInspectionAsync(HoldInspectionRequest request, int userId);
    Task<QualityInspectionDto> ReinspectAsync(ReinspectRequest request, int userId);
    Task<QualityInspectionDto> CancelInspectionAsync(int id, int userId, string? reason = null);

    Task<string> GenerateInspectionNumberAsync(DateTime date);
    Task<IEnumerable<QualityInspectionDto>> GetInspectionHistoryForBatchAsync(int batchId);
}
