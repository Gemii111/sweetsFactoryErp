using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class SalesFulfillmentRepository : BaseRepository<SalesFulfillment>, ISalesFulfillmentRepository
{
    public SalesFulfillmentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SalesFulfillment>> GetAllFulfillmentsAsync(
        SalesFulfillmentStatus? status = null,
        int? salesOrderId = null,
        int? customerId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool trackChanges = false)
    {
        var query = trackChanges ? _context.SalesFulfillments : _context.SalesFulfillments.AsNoTracking();

        query = query
            .Include(sf => sf.Customer)
            .Include(sf => sf.Warehouse)
            .Include(sf => sf.SalesOrder)
            .Include(sf => sf.ShippedByUser)
            .Include(sf => sf.Items)
                .ThenInclude(i => i.Product)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(sf => sf.Status == status.Value);
        }

        if (salesOrderId.HasValue && salesOrderId.Value > 0)
        {
            query = query.Where(sf => sf.SalesOrderId == salesOrderId.Value);
        }

        if (customerId.HasValue && customerId.Value > 0)
        {
            query = query.Where(sf => sf.CustomerId == customerId.Value);
        }

        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            query = query.Where(sf => sf.WarehouseId == warehouseId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(sf => sf.FulfillmentDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(sf => sf.FulfillmentDate <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var clean = searchTerm.Trim().ToLower();
            query = query.Where(sf =>
                sf.FulfillmentNumber.ToLower().Contains(clean) ||
                (sf.Customer != null && sf.Customer.Name.ToLower().Contains(clean)) ||
                (sf.SalesOrder != null && sf.SalesOrder.OrderNumber.ToLower().Contains(clean)) ||
                (sf.Notes != null && sf.Notes.ToLower().Contains(clean)));
        }

        return await query.OrderByDescending(sf => sf.Id).ToListAsync();
    }

    public async Task<SalesFulfillment?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.SalesFulfillments : _context.SalesFulfillments.AsNoTracking();

        return await query
            .Include(sf => sf.Customer)
            .Include(sf => sf.Warehouse)
            .Include(sf => sf.SalesOrder!)
                .ThenInclude(so => so.Items!)
                    .ThenInclude(soi => soi.Product)
            .Include(sf => sf.ShippedByUser)
            .Include(sf => sf.Items)
                .ThenInclude(i => i.Product)
            .Include(sf => sf.Items)
                .ThenInclude(i => i.Location)
            .Include(sf => sf.Items)
                .ThenInclude(i => i.InventoryTransaction)
            .FirstOrDefaultAsync(sf => sf.Id == id);
    }

    public async Task<SalesFulfillment?> GetByFulfillmentNumberAsync(string fulfillmentNumber, bool trackChanges = false)
    {
        var query = trackChanges ? _context.SalesFulfillments : _context.SalesFulfillments.AsNoTracking();

        return await query
            .Include(sf => sf.Customer)
            .Include(sf => sf.Items)
            .FirstOrDefaultAsync(sf => sf.FulfillmentNumber.ToLower() == fulfillmentNumber.Trim().ToLower());
    }

    public async Task<bool> IsFulfillmentNumberUniqueAsync(string fulfillmentNumber, int? excludeId = null)
    {
        var clean = fulfillmentNumber.Trim().ToLower();
        return !await _context.SalesFulfillments.AnyAsync(sf =>
            sf.FulfillmentNumber.ToLower() == clean &&
            (!excludeId.HasValue || sf.Id != excludeId.Value));
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1).AddTicks(-1);

        return await _context.SalesFulfillments.CountAsync(sf =>
            sf.FulfillmentDate >= start && sf.FulfillmentDate <= end);
    }

    public async Task<decimal> GetTotalFulfilledQuantityForOrderItemAsync(int salesOrderItemId)
    {
        return await _context.SalesFulfillmentItems
            .Where(sfi => sfi.SalesOrderItemId == salesOrderItemId && sfi.SalesFulfillment!.Status == SalesFulfillmentStatus.Shipped)
            .SumAsync(sfi => sfi.ShippedQuantity);
    }
}
