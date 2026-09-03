using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class SupplierCategoryRepository : BaseRepository<SupplierCategory>, ISupplierCategoryRepository
{
    public SupplierCategoryRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<SupplierCategory>> GetAllCategoriesAsync(bool onlyActive = false)
    {
        var query = _context.SupplierCategories.AsNoTracking();
        if (onlyActive)
        {
            query = query.Where(c => c.IsActive);
        }
        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<SupplierCategory?> GetByCodeAsync(string code, bool trackChanges = false)
    {
        var query = trackChanges ? _context.SupplierCategories : _context.SupplierCategories.AsNoTracking();
        return await query.FirstOrDefaultAsync(c => c.Code == code);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        var query = _context.SupplierCategories.Where(c => c.Code == code);
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return !await query.AnyAsync();
    }
}
