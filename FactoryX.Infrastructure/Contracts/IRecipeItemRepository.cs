using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IRecipeItemRepository : IBaseRepository<RecipeItem>
{
    Task<IEnumerable<RecipeItem>> GetItemsByVersionIdAsync(int versionId);
}
