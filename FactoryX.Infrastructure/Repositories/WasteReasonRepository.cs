using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class WasteReasonRepository : BaseRepository<WasteReason>, IWasteReasonRepository
{
    public WasteReasonRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WasteReason>> GetAllReasonsAsync(bool onlyActive = false)
    {
        var query = _context.WasteReasons.AsNoTracking().AsQueryable();

        if (onlyActive)
        {
            query = query.Where(r => r.IsActive);
        }

        return await query.OrderBy(r => r.Code).ToListAsync();
    }

    public async Task<WasteReason?> GetByCodeAsync(string code)
    {
        var clean = code.Trim().ToUpper();
        return await _context.WasteReasons
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Code.ToUpper() == clean);
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        var clean = code.Trim().ToUpper();
        return !await _context.WasteReasons.AnyAsync(r =>
            r.Code.ToUpper() == clean &&
            (!excludeId.HasValue || r.Id != excludeId.Value));
    }

    public async Task<bool> HasAssociatedWastesAsync(int id)
    {
        return await _context.Wastes.AnyAsync(w => w.WasteReasonId == id);
    }
}
