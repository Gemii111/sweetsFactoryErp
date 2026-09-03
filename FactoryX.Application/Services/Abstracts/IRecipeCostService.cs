using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IRecipeCostService
{
    Task<RecipeCostBreakdownDto> CalculateVersionCostAsync(RecipeVersion version);
    Task<RecipeCostBreakdownDto> CalculateLiveCostAsync(
        IEnumerable<RecipeItemRequest> items,
        decimal expectedOutput,
        string? outputUnit,
        decimal wastePercentage,
        decimal laborCost,
        decimal machineCost,
        decimal overheadCost);
}
