using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class WasteRepository : BaseRepository<Waste>, IWasteRepository
{
    public WasteRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Waste>> GetAllWastesWithDetailsAsync(
        WasteType? wasteType = null,
        WasteStatus? status = null,
        int? batchId = null,
        int? productId = null,
        int? materialId = null,
        int? reasonId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var query = _context.Wastes
            .Include(w => w.ProductionBatch)
            .Include(w => w.WorkOrder)
            .Include(w => w.Material)
            .Include(w => w.Product)
            .Include(w => w.Warehouse)
            .Include(w => w.Location)
            .Include(w => w.WasteReason)
            .Include(w => w.CreatedByUser)
            .Include(w => w.ApprovedByUser)
            .Include(w => w.InventoryTransaction)
            .AsNoTracking()
            .AsQueryable();

        if (wasteType.HasValue)
        {
            query = query.Where(w => w.WasteType == wasteType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(w => w.Status == status.Value);
        }

        if (batchId.HasValue)
        {
            query = query.Where(w => w.ProductionBatchId == batchId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(w => w.ProductId == productId.Value);
        }

        if (materialId.HasValue)
        {
            query = query.Where(w => w.MaterialId == materialId.Value);
        }

        if (reasonId.HasValue)
        {
            query = query.Where(w => w.WasteReasonId == reasonId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(w => w.WasteDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(w => w.WasteDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(w =>
                w.WasteNumber.ToLower().Contains(term) ||
                (w.RawMaterialBatchNumber != null && w.RawMaterialBatchNumber.ToLower().Contains(term)) ||
                (w.Material != null && w.Material.Name.ToLower().Contains(term)) ||
                (w.Product != null && w.Product.Name.ToLower().Contains(term)) ||
                (w.ProductionBatch != null && w.ProductionBatch.BatchNumber.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(w => w.WasteDate).ThenByDescending(w => w.Id).ToListAsync();
    }

    public async Task<Waste?> GetWasteWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Wastes.AsQueryable() : _context.Wastes.AsNoTracking();

        return await query
            .Include(w => w.ProductionBatch)
                .ThenInclude(b => b!.WorkOrder)
            .Include(w => w.WorkOrder)
            .Include(w => w.Material)
            .Include(w => w.Product)
            .Include(w => w.Warehouse)
            .Include(w => w.Location)
            .Include(w => w.WasteReason)
            .Include(w => w.CreatedByUser)
            .Include(w => w.ApprovedByUser)
            .Include(w => w.InventoryTransaction)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<Waste?> GetWasteByNumberAsync(string wasteNumber, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Wastes.AsQueryable() : _context.Wastes.AsNoTracking();
        return await query
            .Include(w => w.ProductionBatch)
            .Include(w => w.Material)
            .Include(w => w.Product)
            .Include(w => w.WasteReason)
            .FirstOrDefaultAsync(w => w.WasteNumber == wasteNumber);
    }

    public async Task<bool> IsWasteNumberUniqueAsync(string wasteNumber, int? excludeId = null)
    {
        var clean = wasteNumber.Trim().ToLower();
        return !await _context.Wastes.AnyAsync(w =>
            w.WasteNumber.ToLower() == clean &&
            (!excludeId.HasValue || w.Id != excludeId.Value));
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1).AddTicks(-1);
        return await _context.Wastes.CountAsync(w => w.WasteDate >= start && w.WasteDate <= end);
    }
}
