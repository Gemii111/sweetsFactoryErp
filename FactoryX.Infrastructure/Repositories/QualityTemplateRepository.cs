using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class QualityTemplateRepository : Repository<QualityTemplate>, IQualityTemplateRepository
{
    public QualityTemplateRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<QualityTemplate>> GetAllTemplatesWithDetailsAsync(
        bool onlyActive = false, int? categoryId = null, int? productId = null)
    {
        IQueryable<QualityTemplate> query = _context.QualityTemplates
            .Include(t => t.ProductCategory)
            .Include(t => t.Product)
            .Include(t => t.Items)
            .AsNoTracking();

        if (onlyActive)
        {
            query = query.Where(t => t.IsActive);
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(t => t.ProductCategoryId == categoryId.Value);
        }

        if (productId.HasValue && productId.Value > 0)
        {
            query = query.Where(t => t.ProductId == productId.Value);
        }

        return await query.OrderBy(t => t.Name).ToListAsync();
    }

    public async Task<QualityTemplate?> GetTemplateWithItemsAsync(int id, bool trackChanges = false)
    {
        IQueryable<QualityTemplate> query = _context.QualityTemplates
            .Include(t => t.ProductCategory)
            .Include(t => t.Product)
            .Include(t => t.Items.OrderBy(i => i.Sequence));

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<QualityTemplate?> GetTemplateByCodeAsync(string code, bool trackChanges = false)
    {
        IQueryable<QualityTemplate> query = _context.QualityTemplates
            .Include(t => t.ProductCategory)
            .Include(t => t.Product)
            .Include(t => t.Items.OrderBy(i => i.Sequence));

        if (!trackChanges)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(t => t.Code.ToLower() == code.Trim().ToLower());
    }

    public async Task<QualityTemplate?> GetApplicableTemplateForProductAsync(int productId, int? categoryId = null)
    {
        // 1. Specific Product Template takes first precedence
        var productTemplate = await _context.QualityTemplates
            .Include(t => t.Items.OrderBy(i => i.Sequence))
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IsActive && t.ProductId == productId);

        if (productTemplate != null)
        {
            return productTemplate;
        }

        // 2. If no categoryId supplied, lookup category from product
        if (!categoryId.HasValue || categoryId.Value <= 0)
        {
            var prod = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            categoryId = prod?.ProductCategoryId;
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            var categoryTemplate = await _context.QualityTemplates
                .Include(t => t.Items.OrderBy(i => i.Sequence))
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.IsActive && t.ProductCategoryId == categoryId.Value && t.ProductId == null);

            if (categoryTemplate != null)
            {
                return categoryTemplate;
            }
        }

        // 3. Fallback: general active template
        return await _context.QualityTemplates
            .Include(t => t.Items.OrderBy(i => i.Sequence))
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.IsActive && t.ProductId == null && t.ProductCategoryId == null);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        var cleanCode = code.Trim().ToLower();
        return !await _context.QualityTemplates
            .AnyAsync(t => t.Code.ToLower() == cleanCode && (!excludeId.HasValue || t.Id != excludeId.Value));
    }
}
