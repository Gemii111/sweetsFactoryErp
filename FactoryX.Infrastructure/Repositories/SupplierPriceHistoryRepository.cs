using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class SupplierPriceHistoryRepository : BaseRepository<SupplierPriceHistory>, ISupplierPriceHistoryRepository
{
    public SupplierPriceHistoryRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SupplierPriceHistory>> GetHistoryAsync(int? supplierId = null, int? materialId = null)
    {
        var query = _context.SupplierPriceHistories
            .Include(ph => ph.Supplier)
            .Include(ph => ph.Material)
            .Include(ph => ph.PurchaseOrder)
            .Include(ph => ph.PurchaseReceipt)
            .AsQueryable();

        if (supplierId.HasValue && supplierId.Value > 0)
        {
            query = query.Where(ph => ph.SupplierId == supplierId.Value);
        }

        if (materialId.HasValue && materialId.Value > 0)
        {
            query = query.Where(ph => ph.MaterialId == materialId.Value);
        }

        return await query.OrderByDescending(ph => ph.PurchaseDate).ToListAsync();
    }

    public async Task<SupplierPriceHistory?> GetLatestPriceAsync(int supplierId, int materialId)
    {
        return await _context.SupplierPriceHistories
            .Where(ph => ph.SupplierId == supplierId && ph.MaterialId == materialId)
            .OrderByDescending(ph => ph.PurchaseDate)
            .FirstOrDefaultAsync();
    }
}
