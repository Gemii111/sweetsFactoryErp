using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class PackagingCostService : IPackagingCostService
{
    private readonly IRepositoryManager _repositoryManager;

    public PackagingCostService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public async Task<PackagingCostSummaryDto> CalculatePackagingCostAsync(int packagingBomId, int? versionId = null)
    {
        var bom = await _repositoryManager.PackagingBOMRepository.GetByIdWithDetailsAsync(packagingBomId);
        if (bom == null)
        {
            throw new KeyNotFoundException($"مواصفة التعبئة والتغليف بالمعرف #{packagingBomId} غير موجودة.");
        }

        PackagingBOMVersion? targetVersion = null;
        if (versionId.HasValue && versionId.Value > 0)
        {
            targetVersion = bom.Versions.FirstOrDefault(v => v.Id == versionId.Value);
        }
        else
        {
            targetVersion = bom.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault(v => v.Status == PackagingBOMStatus.Active)
                            ?? bom.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
        }

        var summary = new PackagingCostSummaryDto
        {
            PackagingBOMId = bom.Id,
            PackagingCode = bom.Code,
            PackagingName = bom.Name,
            PackSizeKg = bom.PackSizeKg > 0 ? bom.PackSizeKg : 1.0m
        };

        if (targetVersion != null && targetVersion.Items.Any())
        {
            decimal totalPackCost = 0;
            foreach (var item in targetVersion.Items.OrderBy(i => i.Sequence))
            {
                var material = item.Material ?? await _repositoryManager.MaterialRepository.GetByIdAsync(item.MaterialId);
                var unitCost = material != null ? (material.CurrentCost > 0 ? material.CurrentCost : (material.StandardCost > 0 ? material.StandardCost : material.UnitCost)) : 0m;
                var lineCost = Math.Round(item.QuantityRequired * unitCost, 4);
                totalPackCost += lineCost;

                summary.ItemBreakdown.Add(new PackagingCostItemDetailDto
                {
                    MaterialId = item.MaterialId,
                    MaterialName = material?.Name ?? $"Material #{item.MaterialId}",
                    Quantity = item.QuantityRequired,
                    Unit = item.Unit,
                    UnitCost = unitCost,
                    LineCost = lineCost
                });
            }

            summary.CostPerPack = Math.Round(totalPackCost, 4);
            summary.CostPerKg = summary.PackSizeKg > 0 ? Math.Round(totalPackCost / summary.PackSizeKg, 4) : totalPackCost;
        }
        else if (bom.Items.Any())
        {
            // Backward compatibility for legacy flat items
            decimal totalPackCost = 0;
            foreach (var item in bom.Items.OrderBy(i => i.Sequence))
            {
                var material = item.Material ?? await _repositoryManager.MaterialRepository.GetByIdAsync(item.MaterialId);
                var unitCost = material != null ? (material.CurrentCost > 0 ? material.CurrentCost : (material.StandardCost > 0 ? material.StandardCost : material.UnitCost)) : 0m;
                var lineCost = Math.Round(item.QuantityRequired * unitCost, 4);
                totalPackCost += lineCost;

                summary.ItemBreakdown.Add(new PackagingCostItemDetailDto
                {
                    MaterialId = item.MaterialId,
                    MaterialName = material?.Name ?? $"Material #{item.MaterialId}",
                    Quantity = item.QuantityRequired,
                    Unit = item.Unit,
                    UnitCost = unitCost,
                    LineCost = lineCost
                });
            }

            summary.CostPerPack = Math.Round(totalPackCost, 4);
            summary.CostPerKg = summary.PackSizeKg > 0 ? Math.Round(totalPackCost / summary.PackSizeKg, 4) : totalPackCost;
        }

        return summary;
    }

    public decimal CalculateVersionMaterialCost(PackagingBOMVersion version)
    {
        if (version.Items == null || !version.Items.Any()) return 0m;

        decimal totalCost = 0m;
        foreach (var item in version.Items)
        {
            var material = item.Material;
            var unitCost = material != null ? (material.CurrentCost > 0 ? material.CurrentCost : (material.StandardCost > 0 ? material.StandardCost : material.UnitCost)) : 0m;
            totalCost += item.QuantityRequired * unitCost;
        }

        return Math.Round(totalCost, 4);
    }

    public decimal CalculateOrderPackagingCost(IEnumerable<PackagingConsumption> consumptions)
    {
        if (consumptions == null) return 0m;
        return Math.Round(consumptions.Sum(c => c.TotalCost), 4);
    }
}
