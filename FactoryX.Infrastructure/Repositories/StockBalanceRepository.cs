using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class StockBalanceRepository : BaseRepository<StockBalance>, IStockBalanceRepository
{
    public StockBalanceRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<StockBalance?> FindStockAsync(int warehouseId, int? locationId, int? materialId, int? productId, string batchNumber)
    {
        var query = _context.StockBalances.AsQueryable();

        query = query.Where(s => s.WarehouseId == warehouseId);
        
        if (locationId.HasValue)
            query = query.Where(s => s.LocationId == locationId);
        else
            query = query.Where(s => s.LocationId == null);

        if (materialId.HasValue)
            query = query.Where(s => s.MaterialId == materialId);
        
        if (productId.HasValue)
            query = query.Where(s => s.ProductId == productId);

        batchNumber = batchNumber ?? string.Empty;
        query = query.Where(s => s.BatchNumber == batchNumber);

        return await query.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<StockBalance>> GetStockBalancesAsync(
        int? warehouseId, int? locationId, int? materialId, int? productId, string? batchNumber)
    {
        var query = _context.StockBalances
            .Include(s => s.Warehouse)
            .Include(s => s.Location)
            .Include(s => s.Material)
            .Include(s => s.Product)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(s => s.WarehouseId == warehouseId.Value);

        if (locationId.HasValue)
            query = query.Where(s => s.LocationId == locationId.Value);

        if (materialId.HasValue)
            query = query.Where(s => s.MaterialId == materialId.Value);

        if (productId.HasValue)
            query = query.Where(s => s.ProductId == productId.Value);

        if (!string.IsNullOrWhiteSpace(batchNumber))
            query = query.Where(s => s.BatchNumber.Contains(batchNumber));

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<StockBalance>> GetExpiringStockAsync(int daysUntilExpiry)
    {
        var targetDate = DateTime.UtcNow.AddDays(daysUntilExpiry);

        return await _context.StockBalances
            .Include(s => s.Warehouse)
            .Include(s => s.Location)
            .Include(s => s.Material)
            .Include(s => s.Product)
            .Where(s => s.ExpiryDate.HasValue && s.ExpiryDate.Value <= targetDate && s.Quantity > 0)
            .OrderBy(s => s.ExpiryDate)
            .ToListAsync();
    }
}
