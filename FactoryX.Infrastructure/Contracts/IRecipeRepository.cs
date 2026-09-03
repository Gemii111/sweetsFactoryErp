using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IRecipeRepository : IBaseRepository<Recipe>
{
    Task<IEnumerable<Recipe>> GetFilteredRecipesAsync(string? search, int? productId, bool? isActive);
    Task<Recipe?> GetRecipeWithDetailsAsync(int id);
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    Task<Recipe?> GetActiveRecipeForProductAsync(int productId, DateTime? date = null);
}
