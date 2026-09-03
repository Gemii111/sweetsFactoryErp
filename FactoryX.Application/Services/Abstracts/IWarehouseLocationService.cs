using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IWarehouseLocationService
{
    Task<IEnumerable<WarehouseLocationDto>> GetAllLocationsAsync();
    Task<IEnumerable<WarehouseLocationDto>> GetByWarehouseIdAsync(int warehouseId);
    Task<WarehouseLocationDto?> GetByIdAsync(int id);
    Task<WarehouseLocationDto> CreateAsync(CreateWarehouseLocationRequest request);
    Task UpdateAsync(UpdateWarehouseLocationRequest request);
    Task ToggleActiveAsync(int id);
}
