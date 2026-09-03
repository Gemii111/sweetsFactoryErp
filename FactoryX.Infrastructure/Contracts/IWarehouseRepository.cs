using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IWarehouseRepository : IBaseRepository<Warehouse>
{
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync();
    Task<Warehouse?> GetWithLocationsAsync(int warehouseId);
}
