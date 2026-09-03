using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IProductionExecutionService
{
    Task<BatchExecutionDetailsDto> GetExecutionDetailsAsync(int batchId);
    Task<ProductionBatchDto> StartBatchAsync(StartBatchRequest request, int userId);
    Task<ProductionBatchDto> PauseBatchAsync(int batchId, string? reason, int userId);
    Task<ProductionBatchDto> ResumeBatchAsync(int batchId, int userId);
    Task<ProductionBatchDto> CompleteBatchAsync(CompleteBatchRequest request, int userId);
    Task<ProductionBatchDto> CancelBatchAsync(CancelBatchRequest request, int userId);
}
