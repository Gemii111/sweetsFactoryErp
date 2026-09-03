using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IWasteRepository : IBaseRepository<Waste>
{
    Task<IEnumerable<Waste>> GetAllWastesWithDetailsAsync(
        WasteType? wasteType = null,
        WasteStatus? status = null,
        int? batchId = null,
        int? productId = null,
        int? materialId = null,
        int? reasonId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<Waste?> GetWasteWithDetailsAsync(int id, bool trackChanges = false);
    Task<Waste?> GetWasteByNumberAsync(string wasteNumber, bool trackChanges = false);
    Task<bool> IsWasteNumberUniqueAsync(string wasteNumber, int? excludeId = null);
    Task<int> GetCountForDateAsync(DateTime date);
}
