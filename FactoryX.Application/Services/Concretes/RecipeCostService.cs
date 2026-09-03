using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class RecipeCostService : IRecipeCostService
{
    private readonly IRepositoryManager _repositoryManager;

    public RecipeCostService(IRepositoryManager repositoryManager)
    {
        _repositoryManager = repositoryManager;
    }

    public async Task<RecipeCostBreakdownDto> CalculateVersionCostAsync(RecipeVersion version)
    {
        decimal materialCost = 0m;

        if (version.Items != null && version.Items.Any())
        {
            // If items don't have Material loaded or CurrentCost, reload materials
            var materialIds = version.Items.Select(i => i.MaterialId).Distinct().ToList();
            var allMaterials = (await _repositoryManager.MaterialRepository.GetAllAsync())
                .Where(m => materialIds.Contains(m.Id))
                .ToDictionary(m => m.Id, m => m);

            foreach (var item in version.Items)
            {
                var cost = allMaterials.TryGetValue(item.MaterialId, out var mat) ? mat.CurrentCost : item.UnitCost;
                item.UnitCost = cost;
                item.TotalCost = item.Quantity * cost;
                materialCost += item.TotalCost;
            }
        }

        var wasteCost = materialCost * (version.ExpectedWastePercentage / 100m);
        var totalCost = materialCost + version.LaborCost + version.MachineCost + version.OverheadCost + wasteCost;
        var costPerUnit = version.ExpectedOutput > 0 ? (totalCost / version.ExpectedOutput) : 0m;

        return new RecipeCostBreakdownDto
        {
            MaterialCost = Math.Round(materialCost, 2),
            LaborCost = Math.Round(version.LaborCost, 2),
            MachineCost = Math.Round(version.MachineCost, 2),
            OverheadCost = Math.Round(version.OverheadCost, 2),
            WasteCost = Math.Round(wasteCost, 2),
            TotalCost = Math.Round(totalCost, 2),
            ExpectedOutput = version.ExpectedOutput,
            OutputUnit = string.IsNullOrWhiteSpace(version.OutputUnit) ? "KG" : version.OutputUnit,
            CostPerOutputUnit = Math.Round(costPerUnit, 2),
            CalculatedAt = DateTime.UtcNow,
            IsLiveEstimate = true
        };
    }

    public async Task<RecipeCostBreakdownDto> CalculateLiveCostAsync(
        IEnumerable<RecipeItemRequest> items,
        decimal expectedOutput,
        string? outputUnit,
        decimal wastePercentage,
        decimal laborCost,
        decimal machineCost,
        decimal overheadCost)
    {
        decimal materialCost = 0m;

        if (items != null && items.Any())
        {
            var materialIds = items.Select(i => i.MaterialId).Distinct().ToList();
            var allMaterials = (await _repositoryManager.MaterialRepository.GetAllAsync())
                .Where(m => materialIds.Contains(m.Id))
                .ToDictionary(m => m.Id, m => m);

            foreach (var item in items)
            {
                if (allMaterials.TryGetValue(item.MaterialId, out var mat))
                {
                    materialCost += item.Quantity * mat.CurrentCost;
                }
            }
        }

        var wasteCost = materialCost * (wastePercentage / 100m);
        var totalCost = materialCost + laborCost + machineCost + overheadCost + wasteCost;
        var costPerUnit = expectedOutput > 0 ? (totalCost / expectedOutput) : 0m;

        return new RecipeCostBreakdownDto
        {
            MaterialCost = Math.Round(materialCost, 2),
            LaborCost = Math.Round(laborCost, 2),
            MachineCost = Math.Round(machineCost, 2),
            OverheadCost = Math.Round(overheadCost, 2),
            WasteCost = Math.Round(wasteCost, 2),
            TotalCost = Math.Round(totalCost, 2),
            ExpectedOutput = expectedOutput,
            OutputUnit = string.IsNullOrWhiteSpace(outputUnit) ? "KG" : outputUnit,
            CostPerOutputUnit = Math.Round(costPerUnit, 2),
            CalculatedAt = DateTime.UtcNow,
            IsLiveEstimate = true
        };
    }
}
