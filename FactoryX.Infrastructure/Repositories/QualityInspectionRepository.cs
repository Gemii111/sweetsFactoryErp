using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class QualityInspectionRepository : Repository<QualityInspection>, IQualityInspectionRepository
{
    public QualityInspectionRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<QualityInspection>> GetAllInspectionsWithDetailsAsync(
        QualityInspectionStatus? status = null,
        QualityDecision? decision = null,
        int? batchId = null,
        int? orderId = null,
        int? productId = null,
        int? inspectorId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        IQueryable<QualityInspection> query = _context.QualityInspections
            .Include(q => q.ProductionBatch)
                .ThenInclude(b => b!.Product)
            .Include(q => q.WorkOrder)
            .Include(q => q.Product)
            .Include(q => q.Material)
            .Include(q => q.Supplier)
            .Include(q => q.QualityTemplate)
            .Include(q => q.Inspector)
            .Include(q => q.CreatedByUser)
            .Include(q => q.Items)
            .AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(q => q.Status == status.Value);
        }

        if (decision.HasValue)
        {
            query = query.Where(q => q.FinalDecision == decision.Value);
        }

        if (batchId.HasValue && batchId.Value > 0)
        {
            query = query.Where(q => q.ProductionBatchId == batchId.Value);
        }

        if (orderId.HasValue && orderId.Value > 0)
        {
            query = query.Where(q => q.WorkOrderId == orderId.Value);
        }

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(q => q.ProductId == productId.Value || (q.ProductionBatch != null && q.ProductionBatch.ProductId == productId.Value));
        }

        if (inspectorId.HasValue && inspectorId.Value > 0)
        {
            query = query.Where(q => q.InspectorId == inspectorId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(q => q.InspectionDate.Date >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(q => q.InspectionDate.Date <= toDate.Value.Date);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(q =>
                (q.InspectionNumber != null && q.InspectionNumber.ToLower().Contains(term)) ||
                (q.ProductionBatch != null && q.ProductionBatch.BatchNumber.ToLower().Contains(term)) ||
                (q.Product != null && q.Product.Name.ToLower().Contains(term)) ||
                (q.Notes != null && q.Notes.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(q => q.InspectionDate).ThenByDescending(q => q.Id).ToListAsync();
    }

    public async Task<QualityInspection?> GetInspectionWithDetailsAsync(int id, bool trackChanges = false)
    {
        IQueryable<QualityInspection> query = _context.QualityInspections
            .Include(q => q.ProductionBatch)
                .ThenInclude(b => b!.Product)
            .Include(q => q.ProductionBatch)
                .ThenInclude(b => b!.WorkOrder)
            .Include(q => q.ProductionBatch)
                .ThenInclude(b => b!.RecipeVersion)
            .Include(q => q.ProductionBatch)
                .ThenInclude(b => b!.Consumptions!)
                    .ThenInclude(c => c.Material)
            .Include(q => q.ProductionBatch)
                .ThenInclude(b => b!.WasteRecords!)
                    .ThenInclude(w => w.WasteReason)
            .Include(q => q.WorkOrder)
            .Include(q => q.Product)
            .Include(q => q.Material)
            .Include(q => q.Supplier)
            .Include(q => q.QualityTemplate)
            .Include(q => q.Inspector)
            .Include(q => q.CreatedByUser)
            .Include(q => q.SubmittedByUser)
            .Include(q => q.CompletedByUser)
            .Include(q => q.DecisionByUser)
            .Include(q => q.PreviousInspection)
            .Include(q => q.Reinspections)
            .Include(q => q.Items.OrderBy(i => i.Sequence));

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<QualityInspection?> GetInspectionByNumberAsync(string inspectionNumber, bool trackChanges = false)
    {
        IQueryable<QualityInspection> query = _context.QualityInspections
            .Include(q => q.ProductionBatch)
                .ThenInclude(b => b!.Product)
            .Include(q => q.WorkOrder)
            .Include(q => q.Product)
            .Include(q => q.QualityTemplate)
            .Include(q => q.Inspector)
            .Include(q => q.Items.OrderBy(i => i.Sequence));

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(q => q.InspectionNumber.ToLower() == inspectionNumber.Trim().ToLower());
    }

    public async Task<QualityInspection?> GetLatestCompletedInspectionForBatchAsync(int batchId)
    {
        return await _context.QualityInspections
            .Include(q => q.Items)
            .AsNoTracking()
            .Where(q => q.ProductionBatchId == batchId &&
                        (q.Status == QualityInspectionStatus.Approved ||
                         q.Status == QualityInspectionStatus.Rejected ||
                         q.Status == QualityInspectionStatus.Hold))
            .OrderByDescending(q => q.InspectionDate)
            .ThenByDescending(q => q.Id)
            .FirstOrDefaultAsync();
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var targetDate = date.Date;
        return await _context.QualityInspections
            .CountAsync(q => q.InspectionDate.Date == targetDate || q.CreatedAt.Date == targetDate);
    }

    public async Task<bool> IsInspectionNumberUniqueAsync(string inspectionNumber, int? excludeId = null)
    {
        var cleanNumber = inspectionNumber.Trim().ToLower();
        return !await _context.QualityInspections
            .AnyAsync(q => q.InspectionNumber.ToLower() == cleanNumber && (!excludeId.HasValue || q.Id != excludeId.Value));
    }

    public async Task<IEnumerable<QualityInspection>> GetInspectionHistoryForBatchAsync(int batchId)
    {
        return await _context.QualityInspections
            .Include(q => q.QualityTemplate)
            .Include(q => q.Inspector)
            .Include(q => q.Items)
            .AsNoTracking()
            .Where(q => q.ProductionBatchId == batchId)
            .OrderByDescending(q => q.InspectionDate)
            .ThenByDescending(q => q.Id)
            .ToListAsync();
    }
}
