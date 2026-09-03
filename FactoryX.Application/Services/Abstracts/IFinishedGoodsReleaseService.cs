using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IFinishedGoodsReleaseService
{
    Task<ReleaseAvailabilityDto> GetReleaseAvailabilityAsync(int batchId);
    Task<FinishedGoodsReleaseDto> ReleaseFinishedGoodsAsync(CreateFinishedGoodsReleaseRequest request, int userId);
    Task<IEnumerable<FinishedGoodsReleaseDto>> GetAllReleasesAsync(
        int? productId = null,
        int? batchId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);
    Task<FinishedGoodsReleaseDto> GetReleaseByIdAsync(int id);
    Task<IEnumerable<FinishedGoodsReleaseDto>> GetReleasesForBatchAsync(int batchId);
}
