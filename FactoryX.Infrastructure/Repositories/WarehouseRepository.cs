using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class WarehouseRepository : BaseRepository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync()
    {
        return await _context.Warehouses
            .Include(w => w.Locations)
            .Where(w => w.IsActive)
            .ToListAsync();
    }

    public async Task<Warehouse?> GetWithLocationsAsync(int warehouseId)
    {
        return await _context.Warehouses
            .Include(w => w.Locations)
            .FirstOrDefaultAsync(w => w.Id == warehouseId);
    }
}
