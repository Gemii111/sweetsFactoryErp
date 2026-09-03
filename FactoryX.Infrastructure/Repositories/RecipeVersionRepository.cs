using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class RecipeVersionRepository : BaseRepository<RecipeVersion>, IRecipeVersionRepository
{
    public RecipeVersionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<RecipeVersion?> GetVersionWithItemsAndCostsAsync(int versionId)
    {
        return await _context.RecipeVersions
            .Include(v => v.Recipe)
                .ThenInclude(r => r!.Product)
            .Include(v => v.Items!)
                .ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(v => v.Id == versionId);
    }

    public async Task<IEnumerable<RecipeVersion>> GetVersionsByRecipeIdAsync(int recipeId)
    {
        return await _context.RecipeVersions
            .Include(v => v.Items!)
                .ThenInclude(i => i.Material)
            .Where(v => v.RecipeId == recipeId)
            .OrderByDescending(v => v.EffectiveFrom)
            .ToListAsync();
    }

    public async Task<bool> IsVersionNumberUniqueAsync(int recipeId, string versionNumber, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(versionNumber)) return true;
        var query = _context.RecipeVersions.Where(v => v.RecipeId == recipeId);
        if (excludeId.HasValue)
        {
            query = query.Where(v => v.Id != excludeId.Value);
        }
        return !await query.AnyAsync(v => v.VersionNumber.ToLower() == versionNumber.Trim().ToLower());
    }

    public async Task<bool> HasOverlappingActiveVersionAsync(
        int recipeId,
        DateTime effectiveFrom,
        DateTime? effectiveTo,
        int? excludeVersionId = null)
    {
        var query = _context.RecipeVersions
            .Where(v => v.RecipeId == recipeId && v.Status == RecipeStatus.Active);

        if (excludeVersionId.HasValue)
        {
            query = query.Where(v => v.Id != excludeVersionId.Value);
        }

        var activeVersions = await query.ToListAsync();

        foreach (var existing in activeVersions)
        {
            var existingStart = existing.EffectiveFrom.Date;
            var existingEnd = existing.EffectiveTo?.Date ?? DateTime.MaxValue.Date;
            var newStart = effectiveFrom.Date;
            var newEnd = effectiveTo?.Date ?? DateTime.MaxValue.Date;

            // Two intervals [A, B] and [C, D] overlap if max(A, C) <= min(B, D)
            if (Math.Max(existingStart.Ticks, newStart.Ticks) <= Math.Min(existingEnd.Ticks, newEnd.Ticks))
            {
                return true;
            }
        }

        return false;
    }
}
