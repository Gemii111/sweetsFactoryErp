using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class SalesOrderRepository : BaseRepository<SalesOrder>, ISalesOrderRepository
{
    public SalesOrderRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SalesOrder>> GetAllOrdersAsync(
        SalesOrderStatus? status = null,
        int? customerId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool trackChanges = false)
    {
        var query = trackChanges ? _context.SalesOrders : _context.SalesOrders.AsNoTracking();

        query = query
            .Include(so => so.Customer)
            .Include(so => so.Warehouse)
            .Include(so => so.ConfirmedByUser)
            .Include(so => so.Items)
                .ThenInclude(i => i.Product)
            .Include(so => so.Fulfillments)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(so => so.Status == status.Value);
        }

        if (customerId.HasValue && customerId.Value > 0)
        {
            query = query.Where(so => so.CustomerId == customerId.Value);
        }

        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            query = query.Where(so => so.WarehouseId == warehouseId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(so => so.OrderDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(so => so.OrderDate <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var cleanTerm = searchTerm.Trim().ToLower();
            query = query.Where(so =>
                so.OrderNumber.ToLower().Contains(cleanTerm) ||
                (so.Customer != null && so.Customer.Name.ToLower().Contains(cleanTerm)) ||
                (so.Notes != null && so.Notes.ToLower().Contains(cleanTerm)));
        }

        return await query.OrderByDescending(so => so.Id).ToListAsync();
    }

    public async Task<SalesOrder?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.SalesOrders : _context.SalesOrders.AsNoTracking();

        return await query
            .Include(so => so.Customer)
            .Include(so => so.Warehouse)
            .Include(so => so.ConfirmedByUser)
            .Include(so => so.Items)
                .ThenInclude(i => i.Product)
            .Include(so => so.Fulfillments!)
                .ThenInclude(f => f.Items)
                    .ThenInclude(fi => fi.Product)
            .FirstOrDefaultAsync(so => so.Id == id);
    }

    public async Task<SalesOrder?> GetByOrderNumberAsync(string orderNumber, bool trackChanges = false)
    {
        var query = trackChanges ? _context.SalesOrders : _context.SalesOrders.AsNoTracking();

        return await query
            .Include(so => so.Customer)
            .Include(so => so.Warehouse)
            .Include(so => so.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(so => so.OrderNumber.ToLower() == orderNumber.Trim().ToLower());
    }

    public async Task<bool> IsOrderNumberUniqueAsync(string orderNumber, int? excludeId = null)
    {
        var clean = orderNumber.Trim().ToLower();
        return !await _context.SalesOrders.AnyAsync(so =>
            so.OrderNumber.ToLower() == clean &&
            (!excludeId.HasValue || so.Id != excludeId.Value));
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1).AddTicks(-1);

        return await _context.SalesOrders.CountAsync(so =>
            so.OrderDate >= start && so.OrderDate <= end);
    }
}
