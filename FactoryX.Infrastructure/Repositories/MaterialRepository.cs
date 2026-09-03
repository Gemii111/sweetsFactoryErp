using Microsoft.EntityFrameworkCore;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Infrastructure.Repositories;

public class MaterialRepository : BaseRepository<Material>, IMaterialRepository
{
    public MaterialRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Material>> GetAllWithDetailsAsync(bool trackChanges = false)
    {
        var query = trackChanges ? _context.Materials : _context.Materials.AsNoTracking();
        return await query
            .Include(m => m.MaterialCategory)
            .Include(m => m.Warehouse)
            .OrderBy(m => m.Name)
            .ToListAsync();
    }

    public async Task<Material?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Materials : _context.Materials.AsNoTracking();
        return await query
            .Include(m => m.MaterialCategory)
            .Include(m => m.Warehouse)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
    {
        var query = _context.Materials.AsNoTracking();
        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }
        return await query.AnyAsync(m => m.Code.ToLower() == code.Trim().ToLower());
    }

    public async Task<bool> ExistsBySKUAsync(string sku, int? excludeId = null)
    {
        var query = _context.Materials.AsNoTracking();
        if (excludeId.HasValue)
        {
            query = query.Where(m => m.Id != excludeId.Value);
        }
        return await query.AnyAsync(m => m.SKU.ToLower() == sku.Trim().ToLower());
    }

    public async Task<IEnumerable<Material>> GetLowStockMaterialsAsync(bool trackChanges = false)
    {
        var query = trackChanges ? _context.Materials : _context.Materials.AsNoTracking();
        return await query
            .Include(m => m.MaterialCategory)
            .Include(m => m.Warehouse)
            .Where(m => m.IsActive && m.CurrentStock < m.MinimumStock)
            .OrderBy(m => m.CurrentStock)
            .ToListAsync();
    }

    public async Task<IEnumerable<Material>> GetMaterialsBelowReorderLevelAsync(bool trackChanges = false)
    {
        var query = trackChanges ? _context.Materials : _context.Materials.AsNoTracking();
        return await query
            .Include(m => m.MaterialCategory)
            .Include(m => m.Warehouse)
            .Where(m => m.IsActive && m.CurrentStock <= m.ReorderLevel)
            .OrderBy(m => m.CurrentStock)
            .ToListAsync();
    }

    public async Task<IEnumerable<Material>> GetExpiredMaterialsAsync(bool trackChanges = false)
    {
        var today = DateTime.UtcNow.Date;
        var query = trackChanges ? _context.Materials : _context.Materials.AsNoTracking();
        return await query
            .Include(m => m.MaterialCategory)
            .Include(m => m.Warehouse)
            .Where(m => m.IsActive && m.ExpiryDate.HasValue && m.ExpiryDate.Value.Date < today)
            .OrderBy(m => m.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Material>> GetMaterialsExpiringSoonAsync(int days, bool trackChanges = false)
    {
        var today = DateTime.UtcNow.Date;
        var threshold = today.AddDays(days);
        var query = trackChanges ? _context.Materials : _context.Materials.AsNoTracking();
        return await query
            .Include(m => m.MaterialCategory)
            .Include(m => m.Warehouse)
            .Where(m => m.IsActive && m.ExpiryDate.HasValue && m.ExpiryDate.Value.Date >= today && m.ExpiryDate.Value.Date <= threshold)
            .OrderBy(m => m.ExpiryDate)
            .ToListAsync();
    }
}
