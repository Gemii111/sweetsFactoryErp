using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class ProductionBatchRepository : BaseRepository<ProductionBatch>, IProductionBatchRepository
{
    public ProductionBatchRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductionBatch>> GetFilteredBatchesAsync(
        string? search = null,
        int? workOrderId = null,
        int? productId = null,
        ProductionBatchStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool trackChanges = false)
    {
        var query = trackChanges ? _context.ProductionBatches.AsQueryable() : _context.ProductionBatches.AsNoTracking();

        query = query
            .Include(b => b.WorkOrder)
            .Include(b => b.Product)
            .Include(b => b.RecipeVersion)
            .Include(b => b.Machine)
            .Include(b => b.Operator)
            .Include(b => b.Shift)
            .Include(b => b.WorkCenter)
            .Include(b => b.ProductionLine)
            .Include(b => b.Consumptions);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var clean = search.Trim().ToLower();
            query = query.Where(b =>
                b.BatchNumber.ToLower().Contains(clean) ||
                (b.WorkOrder != null && b.WorkOrder.OrderNumber.ToLower().Contains(clean)) ||
                (b.Product != null && (b.Product.Name.ToLower().Contains(clean) || (b.Product.ArabicName != null && b.Product.ArabicName.ToLower().Contains(clean)))));
        }

        if (workOrderId.HasValue && workOrderId.Value > 0)
        {
            query = query.Where(b => b.WorkOrderId == workOrderId.Value);
        }

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(b => b.ProductId == productId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(b => b.ProductionDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(b => b.ProductionDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
        }

        return await query.OrderByDescending(b => b.ProductionDate).ThenByDescending(b => b.Id).ToListAsync();
    }

    public async Task<ProductionBatch?> GetBatchWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.ProductionBatches.AsQueryable() : _context.ProductionBatches.AsNoTracking();

        return await query
            .Include(b => b.WorkOrder)
                .ThenInclude(w => w!.MaterialRequirements)
                    .ThenInclude(mr => mr.Material)
            .Include(b => b.Product)
            .Include(b => b.RecipeVersion)
            .Include(b => b.Machine)
            .Include(b => b.Operator)
            .Include(b => b.Shift)
            .Include(b => b.WorkCenter)
            .Include(b => b.ProductionLine)
            .Include(b => b.Consumptions!)
                .ThenInclude(c => c.Material)
            .Include(b => b.Consumptions!)
                .ThenInclude(c => c.Warehouse)
            .Include(b => b.Consumptions!)
                .ThenInclude(c => c.Location)
            .Include(b => b.ProductionRecords)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<ProductionBatch?> GetBatchWithConsumptionsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.ProductionBatches.AsQueryable() : _context.ProductionBatches.AsNoTracking();

        return await query
            .Include(b => b.Consumptions!)
                .ThenInclude(c => c.Material)
            .FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<bool> IsBatchNumberUniqueAsync(string batchNumber, int? excludeId = null)
    {
        var clean = batchNumber.Trim().ToLower();
        return !await _context.ProductionBatches.AnyAsync(b =>
            b.BatchNumber.ToLower() == clean &&
            (!excludeId.HasValue || b.Id != excludeId.Value));
    }

    public async Task<int> GetBatchCountForPrefixAsync(string prefix)
    {
        return await _context.ProductionBatches.CountAsync(b => b.BatchNumber.StartsWith(prefix));
    }
}
