using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class PurchaseOrderRepository : BaseRepository<PurchaseOrder>, IPurchaseOrderRepository
{
    public PurchaseOrderRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<PurchaseOrder>> GetAllOrdersAsync(
        PurchaseOrderStatus? status = null,
        int? supplierId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var query = _context.PurchaseOrders.AsNoTracking()
            .Include(po => po.Supplier)
            .Include(po => po.Warehouse)
            .Include(po => po.ApprovedByUser)
            .Include(po => po.Items)
                .ThenInclude(i => i.Material)
            .Include(po => po.Receipts)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(po => po.Status == status.Value);
        }

        if (supplierId.HasValue && supplierId.Value > 0)
        {
            query = query.Where(po => po.SupplierId == supplierId.Value);
        }

        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            query = query.Where(po => po.WarehouseId == warehouseId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(po => po.OrderDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(po => po.OrderDate <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var cleanTerm = searchTerm.Trim().ToLower();
            query = query.Where(po =>
                po.OrderNumber.ToLower().Contains(cleanTerm) ||
                (po.Supplier != null && po.Supplier.Name.ToLower().Contains(cleanTerm)) ||
                (po.Notes != null && po.Notes.ToLower().Contains(cleanTerm)));
        }

        return await query.OrderByDescending(po => po.Id).ToListAsync();
    }

    public async Task<PurchaseOrder?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.PurchaseOrders : _context.PurchaseOrders.AsNoTracking();

        return await query
            .Include(po => po.Supplier)
                .ThenInclude(s => s!.Category)
            .Include(po => po.Warehouse)
            .Include(po => po.ApprovedByUser)
            .Include(po => po.PurchaseRequest)
            .Include(po => po.Items)
                .ThenInclude(i => i.Material)
            .Include(po => po.Receipts!)
                .ThenInclude(r => r.Items)
                    .ThenInclude(ri => ri.Material)
            .FirstOrDefaultAsync(po => po.Id == id);
    }

    public async Task<PurchaseOrder?> GetByOrderNumberAsync(string orderNumber, bool trackChanges = false)
    {
        var query = trackChanges ? _context.PurchaseOrders : _context.PurchaseOrders.AsNoTracking();

        return await query
            .Include(po => po.Supplier)
            .Include(po => po.Items)
                .ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(po => po.OrderNumber == orderNumber);
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1).AddTicks(-1);
        return await _context.PurchaseOrders.CountAsync(po => po.OrderDate >= start && po.OrderDate <= end);
    }

    public async Task<bool> IsOrderNumberUniqueAsync(string orderNumber, int? excludeId = null)
    {
        var query = _context.PurchaseOrders.Where(po => po.OrderNumber == orderNumber);
        if (excludeId.HasValue)
        {
            query = query.Where(po => po.Id != excludeId.Value);
        }
        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<PurchaseOrder>> GetReleasableOrdersForSupplierAsync(int supplierId)
    {
        return await _context.PurchaseOrders.AsNoTracking()
            .Where(po => po.SupplierId == supplierId &&
                        (po.Status == PurchaseOrderStatus.Approved || po.Status == PurchaseOrderStatus.PartiallyReceived))
            .Include(po => po.Items)
                .ThenInclude(i => i.Material)
            .OrderByDescending(po => po.OrderDate)
            .ToListAsync();
    }
}
