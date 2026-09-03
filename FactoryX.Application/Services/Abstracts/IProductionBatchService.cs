using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IProductionBatchService
{
    Task<IEnumerable<ProductionBatchDto>> GetBatchesAsync(ProductionBatchFilterRequest? filter = null);
    Task<ProductionBatchDto?> GetBatchByIdAsync(int id);
    Task<ProductionBatchDto> CreateBatchAsync(CreateProductionBatchRequest request);
    Task<ProductionBatchSummaryDto> GetSummaryAsync();
    Task<string> GenerateBatchNumberAsync();
}
