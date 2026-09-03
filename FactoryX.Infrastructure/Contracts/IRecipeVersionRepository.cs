using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IRecipeVersionRepository : IBaseRepository<RecipeVersion>
{
    Task<RecipeVersion?> GetVersionWithItemsAndCostsAsync(int versionId);
    Task<IEnumerable<RecipeVersion>> GetVersionsByRecipeIdAsync(int recipeId);
    Task<bool> IsVersionNumberUniqueAsync(int recipeId, string versionNumber, int? excludeId = null);
    Task<bool> HasOverlappingActiveVersionAsync(int recipeId, DateTime effectiveFrom, DateTime? effectiveTo, int? excludeVersionId = null);
}
