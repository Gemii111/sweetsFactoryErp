using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IWarehouseAccessService
{
    Task<bool> HasWarehouseAccessAsync(int userId, int warehouseId);
    Task<List<int>> GetAllowedWarehouseIdsAsync(int userId);
    Task<bool> CanAccessAllWarehousesAsync(int userId);
    Task<UserWarehouseAssignmentDto> GetUserWarehouseAssignmentAsync(int userId);
    Task SaveUserWarehouseAssignmentAsync(int userId, bool isAllWarehouses, List<int> warehouseIds);
}
