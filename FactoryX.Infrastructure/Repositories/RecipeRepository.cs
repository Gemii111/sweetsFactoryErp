using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class RecipeRepository : BaseRepository<Recipe>, IRecipeRepository
{
    public RecipeRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Recipe>> GetFilteredRecipesAsync(string? search, int? productId, bool? isActive)
    {
        var query = _context.Recipes
            .Include(r => r.Product)
            .Include(r => r.Versions!)
                .ThenInclude(v => v.Items!)
                    .ThenInclude(i => i.Material)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(term) ||
                (r.ArabicName != null && r.ArabicName.ToLower().Contains(term)) ||
                r.Code.ToLower().Contains(term) ||
                (r.Product != null && r.Product.Name.ToLower().Contains(term)));
        }

        if (productId.HasValue)
        {
            query = query.Where(r => r.ProductId == productId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(r => r.IsActive == isActive.Value);
        }

        return await query.OrderBy(r => r.Name).ToListAsync();
    }

    public async Task<Recipe?> GetRecipeWithDetailsAsync(int id)
    {
        return await _context.Recipes
            .Include(r => r.Product)
            .Include(r => r.Versions!)
                .ThenInclude(v => v.Items!)
                    .ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code)) return true;
        var query = _context.Recipes.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }
        return !await query.AnyAsync(r => r.Code.ToLower() == code.Trim().ToLower());
    }

    public async Task<Recipe?> GetActiveRecipeForProductAsync(int productId, DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        return await _context.Recipes
            .Include(r => r.Product)
            .Include(r => r.Versions!)
                .ThenInclude(v => v.Items!)
                    .ThenInclude(i => i.Material)
            .Where(r => r.ProductId == productId && r.IsActive)
            .Where(r => r.Versions!.Any(v =>
                v.Status == RecipeStatus.Active &&
                v.EffectiveFrom <= targetDate &&
                (!v.EffectiveTo.HasValue || v.EffectiveTo.Value >= targetDate)))
            .FirstOrDefaultAsync();
    }
}
