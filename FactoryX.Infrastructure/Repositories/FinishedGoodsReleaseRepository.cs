using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class FinishedGoodsReleaseRepository : BaseRepository<FinishedGoodsRelease>, IFinishedGoodsReleaseRepository
{
    public FinishedGoodsReleaseRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<FinishedGoodsRelease>> GetAllWithDetailsAsync(
        int? productId = null,
        int? batchId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool trackChanges = false)
    {
        var query = trackChanges ? _context.FinishedGoodsReleases : _context.FinishedGoodsReleases.AsNoTracking();

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(r => r.ProductId == productId.Value);
        }

        if (batchId.HasValue && batchId.Value > 0)
        {
            query = query.Where(r => r.ProductionBatchId == batchId.Value);
        }

        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            query = query.Where(r => r.WarehouseId == warehouseId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.ReleasedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(r => r.ReleasedAt <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var cleanTerm = searchTerm.Trim();
            query = query.Where(r =>
                r.ReleaseNumber.Contains(cleanTerm) ||
                r.BatchNumber.Contains(cleanTerm) ||
                (r.Product != null && r.Product.Name.Contains(cleanTerm)));
        }

        return await query
            .Include(r => r.Product)
            .Include(r => r.ProductionBatch)
            .Include(r => r.PackagingOrder)
            .Include(r => r.QCInspection)
            .Include(r => r.Warehouse)
            .Include(r => r.Location)
            .Include(r => r.ReleasedByUser)
            .Include(r => r.InventoryTransaction)
            .OrderByDescending(r => r.ReleasedAt)
            .ToListAsync();
    }

    public async Task<FinishedGoodsRelease?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.FinishedGoodsReleases : _context.FinishedGoodsReleases.AsNoTracking();

        return await query
            .Include(r => r.Product)
                .ThenInclude(p => p!.ProductCategory)
            .Include(r => r.ProductionBatch)
                .ThenInclude(pb => pb!.WorkOrder)
            .Include(r => r.ProductionBatch)
                .ThenInclude(pb => pb!.RecipeVersion)
            .Include(r => r.PackagingOrder)
                .ThenInclude(po => po!.PackagingBOM)
            .Include(r => r.QCInspection)
            .Include(r => r.Warehouse)
            .Include(r => r.Location)
            .Include(r => r.ReleasedByUser)
            .Include(r => r.InventoryTransaction)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<FinishedGoodsRelease?> GetByReleaseNumberAsync(string releaseNumber, bool trackChanges = false)
    {
        var query = trackChanges ? _context.FinishedGoodsReleases : _context.FinishedGoodsReleases.AsNoTracking();
        return await query
            .Include(r => r.Product)
            .Include(r => r.ProductionBatch)
            .Include(r => r.Warehouse)
            .Include(r => r.Location)
            .Include(r => r.ReleasedByUser)
            .FirstOrDefaultAsync(r => r.ReleaseNumber == releaseNumber);
    }

    public async Task<IEnumerable<FinishedGoodsRelease>> GetReleasesForBatchAsync(int batchId)
    {
        return await _context.FinishedGoodsReleases
            .AsNoTracking()
            .Where(r => r.ProductionBatchId == batchId)
            .Include(r => r.Warehouse)
            .Include(r => r.Location)
            .Include(r => r.ReleasedByUser)
            .OrderByDescending(r => r.ReleasedAt)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalReleasedQuantityForBatchAsync(int batchId)
    {
        return await _context.FinishedGoodsReleases
            .AsNoTracking()
            .Where(r => r.ProductionBatchId == batchId)
            .SumAsync(r => r.Quantity);
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var startOfDay = date.Date;
        var endOfDay = startOfDay.AddDays(1);

        return await _context.FinishedGoodsReleases
            .CountAsync(r => r.ReleasedAt >= startOfDay && r.ReleasedAt < endOfDay);
    }

    public async Task<bool> IsReleaseNumberUniqueAsync(string releaseNumber, int? excludeId = null)
    {
        var query = _context.FinishedGoodsReleases.AsNoTracking().Where(r => r.ReleaseNumber == releaseNumber);
        if (excludeId.HasValue && excludeId.Value > 0)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }
}
