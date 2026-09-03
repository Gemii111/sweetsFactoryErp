using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class PackagingBOMRepository : BaseRepository<PackagingBOM>, IPackagingBOMRepository
{
    public PackagingBOMRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<PackagingBOM>> GetAllWithDetailsAsync(bool onlyActive = false, int? productId = null)
    {
        IQueryable<PackagingBOM> query = _context.PackagingBOMs
            .Include(b => b.Product)
            .Include(b => b.Versions)
                .ThenInclude(v => v.Items)
                    .ThenInclude(i => i.Material)
            .Include(b => b.Items)
                .ThenInclude(i => i.Material)
            .AsNoTracking();

        if (onlyActive)
        {
            query = query.Where(b => b.IsActive);
        }

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(b => b.ProductId == productId.Value);
        }

        return await query.OrderBy(b => b.Name).ToListAsync();
    }

    public async Task<PackagingBOM?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        IQueryable<PackagingBOM> query = _context.PackagingBOMs
            .Include(b => b.Product)
            .Include(b => b.Versions.OrderByDescending(v => v.VersionNumber))
                .ThenInclude(v => v.Items.OrderBy(i => i.Sequence))
                    .ThenInclude(i => i.Material)
            .Include(b => b.Items.OrderBy(i => i.Sequence))
                .ThenInclude(i => i.Material);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<PackagingBOM?> GetByCodeAsync(string code, bool trackChanges = false)
    {
        IQueryable<PackagingBOM> query = _context.PackagingBOMs
            .Include(b => b.Product)
            .Include(b => b.Versions)
                .ThenInclude(v => v.Items)
                    .ThenInclude(i => i.Material)
            .Include(b => b.Items)
                .ThenInclude(i => i.Material);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(b => b.Code.ToLower() == code.Trim().ToLower());
    }

    public async Task<PackagingBOMVersion?> GetActiveVersionForBOMAsync(int packagingBomId, DateTime? date = null)
    {
        var targetDate = (date ?? DateTime.UtcNow).Date;

        return await _context.PackagingBOMVersions
            .Include(v => v.Items.OrderBy(i => i.Sequence))
                .ThenInclude(i => i.Material)
            .Include(v => v.PackagingBOM)
                .ThenInclude(b => b!.Product)
            .AsNoTracking()
            .Where(v => v.PackagingBOMId == packagingBomId &&
                        v.Status == PackagingBOMStatus.Active &&
                        v.EffectiveFrom.Date <= targetDate &&
                        (!v.EffectiveTo.HasValue || v.EffectiveTo.Value.Date >= targetDate))
            .OrderByDescending(v => v.VersionNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<PackagingBOMVersion?> GetVersionWithItemsAsync(int versionId, bool trackChanges = false)
    {
        IQueryable<PackagingBOMVersion> query = _context.PackagingBOMVersions
            .Include(v => v.PackagingBOM)
                .ThenInclude(b => b!.Product)
            .Include(v => v.Items.OrderBy(i => i.Sequence))
                .ThenInclude(i => i.Material);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(v => v.Id == versionId);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        var cleanCode = code.Trim().ToLower();
        return !await _context.PackagingBOMs
            .AnyAsync(b => b.Code.ToLower() == cleanCode && (!excludeId.HasValue || b.Id != excludeId.Value));
    }

    public async Task<bool> HasOverlappingActiveVersionAsync(int packagingBomId, DateTime from, DateTime? to, int? excludeVersionId = null)
    {
        var fromDate = from.Date;
        var toDate = to?.Date ?? DateTime.MaxValue.Date;

        return await _context.PackagingBOMVersions
            .AnyAsync(v => v.PackagingBOMId == packagingBomId &&
                           v.Status == PackagingBOMStatus.Active &&
                           (!excludeVersionId.HasValue || v.Id != excludeVersionId.Value) &&
                           v.EffectiveFrom.Date <= toDate &&
                           (v.EffectiveTo == null || v.EffectiveTo.Value.Date >= fromDate));
    }
}
