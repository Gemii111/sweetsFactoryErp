using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IQualityGateService
{
    Task<ReleaseGateResultDto> CanReleaseBatchAsync(int batchId);
}
