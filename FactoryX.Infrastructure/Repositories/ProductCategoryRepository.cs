using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class ProductCategoryRepository : BaseRepository<ProductCategory>, IProductCategoryRepository
{
    public ProductCategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductCategory>> GetAllWithProductsAsync(bool trackChanges = false)
    {
        var query = _context.ProductCategories.Include(c => c.Products).AsQueryable();
        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }
        return await query.OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IEnumerable<ProductCategory>> GetActiveCategoriesAsync()
    {
        return await _context.ProductCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<ProductCategory?> GetCategoryWithProductsAsync(int id)
    {
        return await _context.ProductCategories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code)) return true;
        var query = _context.ProductCategories.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return !await query.AnyAsync(c => c.Code.ToLower() == code.Trim().ToLower());
    }

    public async Task<bool> IsNameUniqueAsync(string name, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(name)) return true;
        var query = _context.ProductCategories.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return !await query.AnyAsync(c => c.Name.ToLower() == name.Trim().ToLower());
    }
}
