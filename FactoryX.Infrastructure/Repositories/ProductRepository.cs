using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetFilteredProductsAsync(
        string? search,
        int? categoryId,
        bool? isActive,
        ProductType? productType)
    {
        var query = _context.Products
            .Include(p => p.ProductCategory)
            .Include(p => p.WorkOrders)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p =>
                p.Name.ToLower().Contains(term) ||
                (p.ArabicName != null && p.ArabicName.ToLower().Contains(term)) ||
                p.Code.ToLower().Contains(term) ||
                p.SKU.ToLower().Contains(term) ||
                (p.Barcode != null && p.Barcode.ToLower().Contains(term)));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.ProductCategoryId == categoryId.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(p => p.IsActive == isActive.Value);
        }

        if (productType.HasValue)
        {
            query = query.Where(p => p.ProductType == productType.Value);
        }

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<Product?> GetProductWithDetailsAsync(int id)
    {
        return await _context.Products
            .Include(p => p.ProductCategory)
            .Include(p => p.WorkOrders!)
                .ThenInclude(w => w.ProductionRecords)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _context.Products
            .Where(p => p.IsActive)
            .Include(p => p.ProductCategory)
            .OrderBy(p => p.Name)
            .ToListAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(code)) return true;
        var query = _context.Products.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        return !await query.AnyAsync(p => p.Code.ToLower() == code.Trim().ToLower());
    }

    public async Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(sku)) return true;
        var query = _context.Products.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        return !await query.AnyAsync(p => p.SKU.ToLower() == sku.Trim().ToLower());
    }

    public async Task<bool> IsBarcodeUniqueAsync(string barcode, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(barcode)) return true;
        var query = _context.Products.AsQueryable();
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }
        return !await query.AnyAsync(p => p.Barcode != null && p.Barcode.ToLower() == barcode.Trim().ToLower());
    }

    public async Task<bool> HasWorkOrdersOrRecordsAsync(int productId)
    {
        var hasWorkOrders = await _context.WorkOrders.AnyAsync(w => w.ProductId == productId);
        return hasWorkOrders;
    }
}
