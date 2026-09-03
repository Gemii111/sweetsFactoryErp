using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IWarehouseLocationRepository : IBaseRepository<WarehouseLocation>
{
    Task<IEnumerable<WarehouseLocation>> GetByWarehouseIdAsync(int warehouseId);
}
