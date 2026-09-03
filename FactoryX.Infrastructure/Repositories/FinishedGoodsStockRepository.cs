using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class FinishedGoodsStockRepository : BaseRepository<FinishedGoodsStock>, IFinishedGoodsStockRepository
{
    public FinishedGoodsStockRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<FinishedGoodsStock>> GetStockBalancesAsync(
        int? warehouseId = null,
        int? locationId = null,
        int? productId = null,
        string? batchNumber = null,
        bool trackChanges = false)
    {
        var query = trackChanges ? _context.FinishedGoodsStocks : _context.FinishedGoodsStocks.AsNoTracking();

        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            query = query.Where(f => f.WarehouseId == warehouseId.Value);
        }

        if (locationId.HasValue && locationId.Value > 0)
        {
            query = query.Where(f => f.LocationId == locationId.Value);
        }

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(f => f.ProductId == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(batchNumber))
        {
            var cleanBatch = batchNumber.Trim();
            query = query.Where(f => f.BatchNumber.Contains(cleanBatch));
        }

        return await query
            .Include(f => f.Product)
            .Include(f => f.ProductionBatch)
            .Include(f => f.Warehouse)
            .Include(f => f.Location)
            .Include(f => f.QCInspection)
            .Include(f => f.PackagingOrder)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync();
    }

    public async Task<FinishedGoodsStock?> FindStockAsync(
        int warehouseId,
        int? locationId,
        int productId,
        string batchNumber,
        bool trackChanges = false)
    {
        var query = trackChanges ? _context.FinishedGoodsStocks : _context.FinishedGoodsStocks.AsNoTracking();

        return await query
            .Include(f => f.Product)
            .Include(f => f.ProductionBatch)
            .Include(f => f.Warehouse)
            .Include(f => f.Location)
            .FirstOrDefaultAsync(f =>
                f.WarehouseId == warehouseId &&
                f.LocationId == locationId &&
                f.ProductId == productId &&
                f.BatchNumber == batchNumber);
    }

    public async Task<FinishedGoodsStock?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.FinishedGoodsStocks : _context.FinishedGoodsStocks.AsNoTracking();

        return await query
            .Include(f => f.Product)
                .ThenInclude(p => p!.ProductCategory)
            .Include(f => f.ProductionBatch)
                .ThenInclude(pb => pb!.WorkOrder)
            .Include(f => f.ProductionBatch)
                .ThenInclude(pb => pb!.RecipeVersion)
            .Include(f => f.Warehouse)
            .Include(f => f.Location)
            .Include(f => f.QCInspection)
            .Include(f => f.PackagingOrder)
                .ThenInclude(po => po!.PackagingBOM)
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<FinishedGoodsStock>> GetStockForBatchAsync(int batchId)
    {
        return await _context.FinishedGoodsStocks
            .AsNoTracking()
            .Where(f => f.ProductionBatchId == batchId)
            .Include(f => f.Warehouse)
            .Include(f => f.Location)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalStockQuantityAsync(int? productId = null)
    {
        var query = _context.FinishedGoodsStocks.AsNoTracking();
        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(f => f.ProductId == productId.Value);
        }

        return await query.SumAsync(f => f.Quantity);
    }

    public async Task<decimal> GetTotalStockValueAsync(int? productId = null)
    {
        var query = _context.FinishedGoodsStocks.AsNoTracking();
        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(f => f.ProductId == productId.Value);
        }

        return await query.SumAsync(f => f.TotalCost);
    }

    public async Task<IEnumerable<FinishedGoodsStock>> GetExpiringStockAsync(int days)
    {
        var targetDate = DateTime.UtcNow.AddDays(days);
        return await _context.FinishedGoodsStocks
            .AsNoTracking()
            .Where(f => f.Quantity > 0 && f.ExpiryDate <= targetDate)
            .Include(f => f.Product)
            .Include(f => f.Warehouse)
            .OrderBy(f => f.ExpiryDate)
            .ToListAsync();
    }
}
