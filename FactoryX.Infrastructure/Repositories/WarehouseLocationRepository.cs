using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class WarehouseLocationRepository : BaseRepository<WarehouseLocation>, IWarehouseLocationRepository
{
    public WarehouseLocationRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<WarehouseLocation>> GetByWarehouseIdAsync(int warehouseId)
    {
        return await _context.WarehouseLocations
            .Include(l => l.Warehouse)
            .Where(l => l.WarehouseId == warehouseId)
            .ToListAsync();
    }
}
