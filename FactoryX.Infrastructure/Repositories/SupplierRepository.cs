using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class SupplierRepository : BaseRepository<Supplier>, ISupplierRepository
{
    public SupplierRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Supplier>> GetAllSuppliersAsync(
        string? searchTerm = null,
        int? categoryId = null,
        bool? isActive = null)
    {
        var query = _context.Suppliers.AsNoTracking()
            .Include(s => s.Category)
            .Include(s => s.PurchaseOrders)
            .Include(s => s.PurchaseReceipts)
            .AsQueryable();

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(s => s.CategoryId == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var cleanTerm = searchTerm.Trim().ToLower();
            query = query.Where(s =>
                s.Code.ToLower().Contains(cleanTerm) ||
                s.Name.ToLower().Contains(cleanTerm) ||
                (s.ArabicName != null && s.ArabicName.ToLower().Contains(cleanTerm)) ||
                (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(cleanTerm)) ||
                (s.Phone != null && s.Phone.ToLower().Contains(cleanTerm)) ||
                (s.Email != null && s.Email.ToLower().Contains(cleanTerm)));
        }

        return await query.OrderBy(s => s.Name).ToListAsync();
    }

    public async Task<Supplier?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Suppliers : _context.Suppliers.AsNoTracking();

        return await query
            .Include(s => s.Category)
            .Include(s => s.PurchaseOrders!)
                .ThenInclude(po => po.Items!)
                    .ThenInclude(poi => poi.Material)
            .Include(s => s.PurchaseReceipts!)
                .ThenInclude(pr => pr.Items!)
                    .ThenInclude(pri => pri.Material)
            .Include(s => s.PriceHistories!)
                .ThenInclude(ph => ph.Material)
            .FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Supplier?> GetByCodeAsync(string code, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Suppliers : _context.Suppliers.AsNoTracking();

        return await query
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Code == code);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        var query = _context.Suppliers.Where(s => s.Code == code);
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }
        return !await query.AnyAsync();
    }

    public async Task<bool> HasPurchasingHistoryAsync(int id)
    {
        var hasOrders = await _context.PurchaseOrders.AnyAsync(po => po.SupplierId == id);
        if (hasOrders) return true;

        var hasReceipts = await _context.PurchaseReceipts.AnyAsync(pr => pr.SupplierId == id);
        return hasReceipts;
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Suppliers.CountAsync();
    }
}
