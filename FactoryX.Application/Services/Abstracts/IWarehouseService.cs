using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IWarehouseService
{
    Task<IEnumerable<WarehouseDto>> GetAllAsync();
    Task<WarehouseDto?> GetByIdAsync(int id);
    Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request);
    Task UpdateAsync(UpdateWarehouseRequest request);
    Task ToggleActiveAsync(int id);
}
