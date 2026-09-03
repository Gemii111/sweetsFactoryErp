using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IProductionPlanningService
{
    Task<List<MaterialRequirementDto>> CalculateMaterialRequirementsAsync(int recipeVersionId, decimal plannedQuantity);
    Task<IEnumerable<RecipeVersionDto>> GetActiveRecipeVersionsForProductAsync(int productId, DateTime plannedDate);
    Task ValidateRecipeVersionForPlanningAsync(int productId, int recipeVersionId, DateTime plannedDate);
}
