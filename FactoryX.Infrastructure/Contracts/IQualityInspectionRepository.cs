using FactoryX.Domain.Entities;
using FactoryX.Domain.Interfaces;

namespace FactoryX.Infrastructure.Contracts;

public interface IQualityInspectionRepository : IRepository<QualityInspection>
{
    Task<IEnumerable<QualityInspection>> GetAllInspectionsWithDetailsAsync(
        QualityInspectionStatus? status = null,
        QualityDecision? decision = null,
        int? batchId = null,
        int? orderId = null,
        int? productId = null,
        int? inspectorId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<QualityInspection?> GetInspectionWithDetailsAsync(int id, bool trackChanges = false);
    Task<QualityInspection?> GetInspectionByNumberAsync(string inspectionNumber, bool trackChanges = false);
    Task<QualityInspection?> GetLatestCompletedInspectionForBatchAsync(int batchId);
    Task<int> GetCountForDateAsync(DateTime date);
    Task<bool> IsInspectionNumberUniqueAsync(string inspectionNumber, int? excludeId = null);
    Task<IEnumerable<QualityInspection>> GetInspectionHistoryForBatchAsync(int batchId);
}
