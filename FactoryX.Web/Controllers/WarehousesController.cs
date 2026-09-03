using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;

namespace FactoryX.Web.Controllers;

[Authorize]
public class WarehousesController : Controller
{
    private readonly IServiceManager _serviceManager;

    public WarehousesController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index()
    {
        var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        return View(warehouses);
    }

    public async Task<IActionResult> Details(int id)
    {
        var warehouse = await _serviceManager.WarehouseService.GetByIdAsync(id);
        if (warehouse == null) return NotFound();

        return View(warehouse);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWarehouseRequest request)
    {
        if (!ModelState.IsValid) return View(request);

        await _serviceManager.WarehouseService.CreateAsync(request);
        TempData["Success"] = "Warehouse created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var warehouse = await _serviceManager.WarehouseService.GetByIdAsync(id);
        if (warehouse == null) return NotFound();

        var request = new UpdateWarehouseRequest(
            warehouse.Id,
            warehouse.Code,
            warehouse.Name,
            warehouse.Description,
            warehouse.Type,
            warehouse.IsActive,
            warehouse.BranchId);

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateWarehouseRequest request)
    {
        if (id != request.Id) return BadRequest();
        if (!ModelState.IsValid) return View(request);

        await _serviceManager.WarehouseService.UpdateAsync(request);
        TempData["Success"] = "Warehouse updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        await _serviceManager.WarehouseService.ToggleActiveAsync(id);
        TempData["Success"] = "Warehouse status updated.";
        return RedirectToAction(nameof(Index));
    }
}
