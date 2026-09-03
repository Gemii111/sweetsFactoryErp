using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IPackagingCostService
{
    Task<PackagingCostSummaryDto> CalculatePackagingCostAsync(int packagingBomId, int? versionId = null);
    decimal CalculateVersionMaterialCost(PackagingBOMVersion version);
    decimal CalculateOrderPackagingCost(IEnumerable<PackagingConsumption> consumptions);
}
