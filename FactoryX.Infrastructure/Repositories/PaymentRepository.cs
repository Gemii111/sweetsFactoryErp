using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class PaymentRepository : BaseRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync(
        int? invoiceId = null,
        int? customerId = null,
        PaymentMethod? method = null,
        PaymentStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool trackChanges = false)
    {
        var query = trackChanges ? _context.Payments : _context.Payments.AsNoTracking();

        query = query
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.SalesOrder)
            .AsQueryable();

        if (invoiceId.HasValue && invoiceId.Value > 0)
        {
            query = query.Where(p => p.InvoiceId == invoiceId.Value);
        }

        if (customerId.HasValue && customerId.Value > 0)
        {
            query = query.Where(p => p.CustomerId == customerId.Value);
        }

        if (method.HasValue)
        {
            query = query.Where(p => p.PaymentMethod == method.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= fromDate.Value.Date);
        }

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(p => p.PaymentDate <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var cleanTerm = searchTerm.Trim().ToLower();
            query = query.Where(p =>
                p.PaymentNumber.ToLower().Contains(cleanTerm) ||
                (p.ReferenceNumber != null && p.ReferenceNumber.ToLower().Contains(cleanTerm)) ||
                (p.Customer != null && (p.Customer.Name.ToLower().Contains(cleanTerm) || p.Customer.Code.ToLower().Contains(cleanTerm))) ||
                (p.Invoice != null && p.Invoice.InvoiceNumber.ToLower().Contains(cleanTerm)) ||
                (p.Notes != null && p.Notes.ToLower().Contains(cleanTerm)));
        }

        return await query.OrderByDescending(p => p.Id).ToListAsync();
    }

    public async Task<Payment?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Payments : _context.Payments.AsNoTracking();

        return await query
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.SalesOrder)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Items)
                    .ThenInclude(it => it.Product)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetByPaymentNumberAsync(string paymentNumber, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Payments : _context.Payments.AsNoTracking();

        return await query
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.SalesOrder)
            .FirstOrDefaultAsync(p => p.PaymentNumber == paymentNumber);
    }

    public async Task<bool> IsPaymentNumberUniqueAsync(string paymentNumber, int? excludeId = null)
    {
        var query = _context.Payments.AsNoTracking().Where(p => p.PaymentNumber == paymentNumber);
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return !await query.AnyAsync();
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1).AddTicks(-1);
        return await _context.Payments.AsNoTracking()
            .CountAsync(p => p.CreatedAt >= start && p.CreatedAt <= end);
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByCustomerIdAsync(int customerId, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Payments : _context.Payments.AsNoTracking();

        return await query
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Payments : _context.Payments.AsNoTracking();

        return await query
            .Include(p => p.Customer)
            .Include(p => p.Invoice)
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
}
