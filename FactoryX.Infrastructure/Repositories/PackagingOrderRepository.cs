using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class PackagingOrderRepository : BaseRepository<PackagingOrder>, IPackagingOrderRepository
{
    public PackagingOrderRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<PackagingOrder>> GetAllWithDetailsAsync(
        PackagingOrderStatus? status = null,
        int? batchId = null,
        int? productId = null,
        int? bomId = null,
        int? operatorId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        IQueryable<PackagingOrder> query = _context.PackagingOrders
            .Include(o => o.ProductionBatch)
            .Include(o => o.Product)
            .Include(o => o.PackagingBOM)
            .Include(o => o.PackagingBOMVersion)
            .Include(o => o.Operator)
            .Include(o => o.CreatedByUser)
            .Include(o => o.Consumptions)
            .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (batchId.HasValue && batchId.Value > 0)
        {
            query = query.Where(o => o.ProductionBatchId == batchId.Value);
        }

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(o => o.ProductId == productId.Value);
        }

        if (bomId.HasValue && bomId.Value > 0)
        {
            query = query.Where(o => o.PackagingBOMId == bomId.Value);
        }

        if (operatorId.HasValue && operatorId.Value > 0)
        {
            query = query.Where(o => o.OperatorId == operatorId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt.Date >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt.Date <= toDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(o =>
                o.OrderNumber.ToLower().Contains(term) ||
                (o.ProductionBatch != null && o.ProductionBatch.BatchNumber.ToLower().Contains(term)) ||
                (o.Product != null && o.Product.Name.ToLower().Contains(term)) ||
                (o.Notes != null && o.Notes.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(o => o.CreatedAt).ThenByDescending(o => o.Id).ToListAsync();
    }

    public async Task<PackagingOrder?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        IQueryable<PackagingOrder> query = _context.PackagingOrders
            .Include(o => o.ProductionBatch)
                .ThenInclude(b => b!.Product)
            .Include(o => o.ProductionBatch)
                .ThenInclude(b => b!.WorkOrder)
            .Include(o => o.ProductionBatch)
                .ThenInclude(b => b!.QualityInspections)
            .Include(o => o.Product)
            .Include(o => o.PackagingBOM)
            .Include(o => o.PackagingBOMVersion)
                .ThenInclude(v => v!.Items)
                    .ThenInclude(i => i.Material)
            .Include(o => o.Operator)
            .Include(o => o.CreatedByUser)
            .Include(o => o.CompletedByUser)
            .Include(o => o.Consumptions)
                .ThenInclude(c => c.Material)
            .Include(o => o.Consumptions)
                .ThenInclude(c => c.Warehouse)
            .Include(o => o.Consumptions)
                .ThenInclude(c => c.Location);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<PackagingOrder?> GetByOrderNumberAsync(string orderNumber, bool trackChanges = false)
    {
        IQueryable<PackagingOrder> query = _context.PackagingOrders
            .Include(o => o.ProductionBatch)
            .Include(o => o.Product)
            .Include(o => o.PackagingBOM)
            .Include(o => o.PackagingBOMVersion)
            .Include(o => o.Consumptions);

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(o => o.OrderNumber.ToLower() == orderNumber.Trim().ToLower());
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var targetDate = date.Date;
        return await _context.PackagingOrders
            .CountAsync(o => o.CreatedAt.Date == targetDate || (o.StartTime.HasValue && o.StartTime.Value.Date == targetDate));
    }

    public async Task<bool> IsOrderNumberUniqueAsync(string orderNumber, int? excludeId = null)
    {
        var cleanNumber = orderNumber.Trim().ToLower();
        return !await _context.PackagingOrders
            .AnyAsync(o => o.OrderNumber.ToLower() == cleanNumber && (!excludeId.HasValue || o.Id != excludeId.Value));
    }

    public async Task<IEnumerable<PackagingOrder>> GetOrdersForBatchAsync(int batchId)
    {
        return await _context.PackagingOrders
            .Include(o => o.PackagingBOM)
            .Include(o => o.Consumptions)
            .AsNoTracking()
            .Where(o => o.ProductionBatchId == batchId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }
}
