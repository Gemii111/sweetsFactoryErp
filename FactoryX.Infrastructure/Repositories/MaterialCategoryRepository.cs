using Microsoft.EntityFrameworkCore;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Infrastructure.Repositories;

public class MaterialCategoryRepository : BaseRepository<MaterialCategory>, IMaterialCategoryRepository
{
    public MaterialCategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MaterialCategory>> GetAllWithMaterialsAsync(bool trackChanges = false)
    {
        var query = trackChanges ? _context.MaterialCategories : _context.MaterialCategories.AsNoTracking();
        return await query
            .Include(c => c.Materials)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<MaterialCategory?> GetByIdWithMaterialsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.MaterialCategories : _context.MaterialCategories.AsNoTracking();
        return await query
            .Include(c => c.Materials)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        var query = _context.MaterialCategories.AsNoTracking();
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return await query.AnyAsync(c => c.Name.ToLower() == name.Trim().ToLower());
    }

    public async Task<bool> ExistsByCodeAsync(string code, int? excludeId = null)
    {
        var query = _context.MaterialCategories.AsNoTracking();
        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }
        return await query.AnyAsync(c => c.Code.ToLower() == code.Trim().ToLower());
    }
}
