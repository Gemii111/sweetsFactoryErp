using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IFinishedGoodsReleaseRepository : IBaseRepository<FinishedGoodsRelease>
{
    Task<IEnumerable<FinishedGoodsRelease>> GetAllWithDetailsAsync(
        int? productId = null,
        int? batchId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool trackChanges = false);

    Task<FinishedGoodsRelease?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<FinishedGoodsRelease?> GetByReleaseNumberAsync(string releaseNumber, bool trackChanges = false);
    Task<IEnumerable<FinishedGoodsRelease>> GetReleasesForBatchAsync(int batchId);
    Task<decimal> GetTotalReleasedQuantityForBatchAsync(int batchId);
    Task<int> GetCountForDateAsync(DateTime date);
    Task<bool> IsReleaseNumberUniqueAsync(string releaseNumber, int? excludeId = null);
}
