using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class RecipeItemRepository : BaseRepository<RecipeItem>, IRecipeItemRepository
{
    public RecipeItemRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<RecipeItem>> GetItemsByVersionIdAsync(int versionId)
    {
        return await _context.RecipeItems
            .Include(i => i.Material)
            .Where(i => i.RecipeVersionId == versionId)
            .OrderBy(i => i.Sequence)
            .ToListAsync();
    }
}
