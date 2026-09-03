using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;

namespace FactoryX.Web.Controllers;

[Authorize]
public class WarehouseLocationsController : Controller
{
    private readonly IServiceManager _serviceManager;

    public WarehouseLocationsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index(int? warehouseId)
    {
        ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        ViewBag.SelectedWarehouseId = warehouseId;

        if (warehouseId.HasValue)
        {
            var locations = await _serviceManager.WarehouseLocationService.GetByWarehouseIdAsync(warehouseId.Value);
            return View(locations);
        }

        return View(new List<WarehouseLocationDto>());
    }

    public async Task<IActionResult> Create(int? warehouseId)
    {
        ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        ViewBag.SelectedWarehouseId = warehouseId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWarehouseLocationRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            return View(request);
        }

        await _serviceManager.WarehouseLocationService.CreateAsync(request);
        TempData["Success"] = "Warehouse location created successfully.";
        return RedirectToAction(nameof(Index), new { warehouseId = request.WarehouseId });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var location = await _serviceManager.WarehouseLocationService.GetByIdAsync(id);
        if (location == null) return NotFound();

        ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();

        var request = new UpdateWarehouseLocationRequest(
            location.Id,
            location.WarehouseId,
            location.Code,
            location.Name,
            location.Section,
            location.Description,
            location.IsActive);

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateWarehouseLocationRequest request)
    {
        if (id != request.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            return View(request);
        }

        await _serviceManager.WarehouseLocationService.UpdateAsync(request);
        TempData["Success"] = "Warehouse location updated successfully.";
        return RedirectToAction(nameof(Index), new { warehouseId = request.WarehouseId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id, int warehouseId)
    {
        await _serviceManager.WarehouseLocationService.ToggleActiveAsync(id);
        TempData["Success"] = "Location status updated.";
        return RedirectToAction(nameof(Index), new { warehouseId });
    }
}
