using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class WorkOrderRepository : BaseRepository<WorkOrder>, IWorkOrderRepository
{
    public WorkOrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WorkOrder>> GetFilteredOrdersAsync(
        string? search = null,
        int? productId = null,
        ProductionOrderStatus? status = null,
        ProductionOrderPriority? priority = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool trackChanges = false)
    {
        var query = trackChanges ? _context.WorkOrders.AsQueryable() : _context.WorkOrders.AsNoTracking();

        query = query
            .Include(w => w.Product)
            .Include(w => w.Recipe)
            .Include(w => w.RecipeVersion)
            .Include(w => w.Machine)
            .Include(w => w.Operator)
            .Include(w => w.Shift)
            .Include(w => w.WorkCenter)
            .Include(w => w.ProductionLine)
            .Include(w => w.ProductionArea);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var cleanSearch = search.Trim().ToLower();
            query = query.Where(w =>
                w.OrderNumber.ToLower().Contains(cleanSearch) ||
                (w.Product != null && (w.Product.Name.ToLower().Contains(cleanSearch) || (w.Product.ArabicName != null && w.Product.ArabicName.ToLower().Contains(cleanSearch)))) ||
                (w.RecipeVersion != null && (w.RecipeVersion.VersionNumber.ToLower().Contains(cleanSearch) || (w.RecipeVersion.VersionName != null && w.RecipeVersion.VersionName.ToLower().Contains(cleanSearch)))));
        }

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(w => w.ProductId == productId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(w => w.OrderStatus == status.Value);
        }

        if (priority.HasValue)
        {
            query = query.Where(w => w.Priority == priority.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(w => w.PlannedDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            query = query.Where(w => w.PlannedDate <= toDate.Value.Date.AddDays(1).AddTicks(-1));
        }

        return await query.OrderByDescending(w => w.PlannedDate).ThenByDescending(w => w.Id).ToListAsync();
    }

    public async Task<WorkOrder?> GetOrderWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.WorkOrders.AsQueryable() : _context.WorkOrders.AsNoTracking();

        return await query
            .Include(w => w.Product)
            .Include(w => w.Recipe)
            .Include(w => w.RecipeVersion)
                .ThenInclude(rv => rv!.Items)
                    .ThenInclude(i => i.Material)
            .Include(w => w.MaterialRequirements)
                .ThenInclude(mr => mr.Material)
            .Include(w => w.Machine)
            .Include(w => w.Operator)
            .Include(w => w.Shift)
            .Include(w => w.WorkCenter)
            .Include(w => w.ProductionLine)
            .Include(w => w.ProductionArea)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<WorkOrder?> GetOrderWithRequirementsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.WorkOrders.AsQueryable() : _context.WorkOrders.AsNoTracking();

        return await query
            .Include(w => w.MaterialRequirements)
            .FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task<bool> IsOrderNumberUniqueAsync(string orderNumber, int? excludeId = null)
    {
        var clean = orderNumber.Trim().ToLower();
        return !await _context.WorkOrders.AnyAsync(w =>
            w.OrderNumber.ToLower() == clean &&
            (!excludeId.HasValue || w.Id != excludeId.Value));
    }

    public async Task<bool> HasActiveOrdersForRecipeVersionAsync(int recipeVersionId)
    {
        return await _context.WorkOrders.AnyAsync(w =>
            w.RecipeVersionId == recipeVersionId &&
            w.OrderStatus != ProductionOrderStatus.Completed &&
            w.OrderStatus != ProductionOrderStatus.Cancelled);
    }

    public async Task<bool> HasActiveOrdersForProductAsync(int productId)
    {
        return await _context.WorkOrders.AnyAsync(w =>
            w.ProductId == productId &&
            w.OrderStatus != ProductionOrderStatus.Completed &&
            w.OrderStatus != ProductionOrderStatus.Cancelled);
    }
}
