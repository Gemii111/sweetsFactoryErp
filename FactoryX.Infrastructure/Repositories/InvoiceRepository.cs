using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync(
        InvoiceStatus? status = null,
        int? customerId = null,
        int? salesOrderId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool trackChanges = false)
    {
        var query = trackChanges ? _context.Invoices : _context.Invoices.AsNoTracking();

        query = query
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .Include(i => i.SalesFulfillment)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .Include(i => i.Payments)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        if (customerId.HasValue && customerId.Value > 0)
        {
            query = query.Where(i => i.CustomerId == customerId.Value);
        }

        if (salesOrderId.HasValue && salesOrderId.Value > 0)
        {
            query = query.Where(i => i.SalesOrderId == salesOrderId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(i => i.InvoiceDate <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var cleanTerm = searchTerm.Trim().ToLower();
            query = query.Where(i =>
                i.InvoiceNumber.ToLower().Contains(cleanTerm) ||
                (i.Customer != null && (i.Customer.Name.ToLower().Contains(cleanTerm) || i.Customer.Code.ToLower().Contains(cleanTerm))) ||
                (i.SalesOrder != null && i.SalesOrder.OrderNumber.ToLower().Contains(cleanTerm)) ||
                (i.Notes != null && i.Notes.ToLower().Contains(cleanTerm)));
        }

        return await query.OrderByDescending(i => i.Id).ToListAsync();
    }

    public async Task<Invoice?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Invoices : _context.Invoices.AsNoTracking();

        return await query
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
                .ThenInclude(so => so!.Warehouse)
            .Include(i => i.SalesFulfillment)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Invoices : _context.Invoices.AsNoTracking();

        return await query
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .Include(i => i.SalesFulfillment)
            .Include(i => i.Items)
                .ThenInclude(item => item.Product)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.InvoiceNumber == invoiceNumber);
    }

    public async Task<bool> IsInvoiceNumberUniqueAsync(string invoiceNumber, int? excludeId = null)
    {
        var query = _context.Invoices.AsNoTracking().Where(i => i.InvoiceNumber == invoiceNumber);
        if (excludeId.HasValue)
        {
            query = query.Where(i => i.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1).AddTicks(-1);
        return await _context.Invoices.AsNoTracking()
            .CountAsync(i => i.CreatedAt >= start && i.CreatedAt <= end);
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByCustomerIdAsync(int customerId, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Invoices : _context.Invoices.AsNoTracking();

        return await query
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .Where(i => i.CustomerId == customerId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesBySalesOrderIdAsync(int salesOrderId, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Invoices : _context.Invoices.AsNoTracking();

        return await query
            .Include(i => i.Customer)
            .Include(i => i.Items)
            .Include(i => i.Payments)
            .Where(i => i.SalesOrderId == salesOrderId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }
}
