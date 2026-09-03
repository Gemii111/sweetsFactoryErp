using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class SupplierPaymentRepository : BaseRepository<SupplierPayment>, ISupplierPaymentRepository
{
    public SupplierPaymentRepository(AppDbContext context) : base(context) { }

    public async Task<SupplierPayment?> GetWithDetailsAsync(int id)
    {
        return await _context.SupplierPayments
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseReceipt)
            .Include(p => p.PurchaseOrder)
            .Include(p => p.CreatedByUser)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<string> GenerateNextPaymentNumberAsync(DateTime date)
    {
        var datePrefix = $"SPAY-{date:yyyyMMdd}-";
        var count = await _context.SupplierPayments
            .CountAsync(p => p.PaymentNumber.StartsWith(datePrefix));

        return $"{datePrefix}{(count + 1):D4}";
    }

    public async Task<IEnumerable<SupplierPayment>> GetBySupplierIdAsync(int supplierId)
    {
        return await _context.SupplierPayments
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseReceipt)
            .Include(p => p.PurchaseOrder)
            .Where(p => p.SupplierId == supplierId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<SupplierPayment>> GetAllWithDetailsAsync()
    {
        return await _context.SupplierPayments
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Include(p => p.PurchaseReceipt)
            .Include(p => p.PurchaseOrder)
            .Include(p => p.CreatedByUser)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }
}
