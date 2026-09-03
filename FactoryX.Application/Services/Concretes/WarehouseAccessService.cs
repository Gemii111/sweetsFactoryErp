using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class WarehouseAccessService : IWarehouseAccessService
{
    private readonly AppDbContext _context;

    public WarehouseAccessService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> HasWarehouseAccessAsync(int userId, int warehouseId)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserWarehouses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || !user.IsActive) return false;

        // Super Admin or Admin role has access to all warehouses
        if (user.Role == "Admin" || user.Role == "Super Admin" || user.Role == "SUPER_ADMIN" ||
            user.UserRoles.Any(ur => ur.Role != null && (ur.Role.Code == "SUPER_ADMIN" || ur.Role.Name == "Super Admin" || ur.Role.Name == "Admin")))
        {
            return true;
        }

        // Check if user has global warehouse access
        if (user.IsAllWarehousesAllowed) return true;

        // Check specific warehouse mapping
        return user.UserWarehouses.Any(uw => uw.WarehouseId == warehouseId);
    }

    public async Task<List<int>> GetAllowedWarehouseIdsAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .Include(u => u.UserWarehouses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || !user.IsActive) return new List<int>();

        if (user.Role == "Admin" || user.Role == "Super Admin" || user.Role == "SUPER_ADMIN" ||
            user.UserRoles.Any(ur => ur.Role != null && (ur.Role.Code == "SUPER_ADMIN" || ur.Role.Name == "Super Admin")) ||
            user.IsAllWarehousesAllowed)
        {
            return await _context.Warehouses.Where(w => w.IsActive).Select(w => w.Id).ToListAsync();
        }

        return user.UserWarehouses.Select(uw => uw.WarehouseId).ToList();
    }

    public async Task<bool> CanAccessAllWarehousesAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null || !user.IsActive) return false;

        if (user.Role == "Admin" || user.Role == "Super Admin" || user.Role == "SUPER_ADMIN" ||
            user.UserRoles.Any(ur => ur.Role != null && (ur.Role.Code == "SUPER_ADMIN" || ur.Role.Name == "Super Admin")))
        {
            return true;
        }

        return user.IsAllWarehousesAllowed;
    }

    public async Task<UserWarehouseAssignmentDto> GetUserWarehouseAssignmentAsync(int userId)
    {
        var user = await _context.Users.AsNoTracking()
            .Include(u => u.UserWarehouses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new InvalidOperationException("User not found");

        var allWarehouses = await _context.Warehouses.AsNoTracking()
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseLookupItemDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name
            }).ToListAsync();

        return new UserWarehouseAssignmentDto
        {
            UserId = user.Id,
            Username = user.Username,
            FullName = user.FullName ?? user.Username,
            IsAllWarehousesAllowed = user.IsAllWarehousesAllowed,
            SelectedWarehouseIds = user.UserWarehouses.Select(uw => uw.WarehouseId).ToList(),
            AvailableWarehouses = allWarehouses
        };
    }

    public async Task SaveUserWarehouseAssignmentAsync(int userId, bool isAllWarehouses, List<int> warehouseIds)
    {
        var user = await _context.Users
            .Include(u => u.UserWarehouses)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) throw new InvalidOperationException("User not found");

        user.IsAllWarehousesAllowed = isAllWarehouses;
        _context.UserWarehouses.RemoveRange(user.UserWarehouses);

        if (!isAllWarehouses && warehouseIds != null && warehouseIds.Any())
        {
            var validWarehouseIds = await _context.Warehouses
                .Where(w => warehouseIds.Contains(w.Id))
                .Select(w => w.Id)
                .ToListAsync();

            foreach (var wid in validWarehouseIds)
            {
                _context.UserWarehouses.Add(new UserWarehouse
                {
                    UserId = user.Id,
                    WarehouseId = wid,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
