using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IProductionBatchRepository : IBaseRepository<ProductionBatch>
{
    Task<IEnumerable<ProductionBatch>> GetFilteredBatchesAsync(
        string? search = null,
        int? workOrderId = null,
        int? productId = null,
        ProductionBatchStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool trackChanges = false);

    Task<ProductionBatch?> GetBatchWithDetailsAsync(int id, bool trackChanges = false);
    Task<ProductionBatch?> GetBatchWithConsumptionsAsync(int id, bool trackChanges = false);
    Task<bool> IsBatchNumberUniqueAsync(string batchNumber, int? excludeId = null);
    Task<int> GetBatchCountForPrefixAsync(string prefix);
}
