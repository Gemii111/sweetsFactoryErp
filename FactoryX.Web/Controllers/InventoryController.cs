using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;

namespace FactoryX.Web.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly IServiceManager _serviceManager;

    public InventoryController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Stock(int? warehouseId, int? locationId, string? batchNumber)
    {
        ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        if (warehouseId.HasValue)
        {
            ViewBag.Locations = await _serviceManager.WarehouseLocationService.GetByWarehouseIdAsync(warehouseId.Value);
        }

        ViewBag.SelectedWarehouseId = warehouseId;
        ViewBag.SelectedLocationId = locationId;
        ViewBag.BatchNumber = batchNumber;

        var stockBalances = await _serviceManager.InventoryService.GetStockAsync(
            warehouseId, locationId, materialId: null, productId: null, batchNumber);

        return View(stockBalances);
    }

    public async Task<IActionResult> Transactions(int? warehouseId, InventoryTransactionType? transactionType, DateTime? startDate, DateTime? endDate)
    {
        ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        ViewBag.SelectedWarehouseId = warehouseId;
        ViewBag.SelectedTransactionType = transactionType;
        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

        var transactions = await _serviceManager.InventoryService.GetStockMovementsAsync(
            warehouseId, materialId: null, productId: null, transactionType, startDate, endDate);

        return View(transactions);
    }

    public async Task<IActionResult> Transfer()
    {
        ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Transfer(StockTransferRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            return View(request);
        }

        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.TryParse(userIdString, out var parsedId) ? parsedId : 1;

            await _serviceManager.InventoryService.TransferStockAsync(request, userId);
            TempData["Success"] = "Stock transferred successfully.";
            return RedirectToAction(nameof(Stock), new { warehouseId = request.DestinationWarehouseId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            ViewBag.Materials = await _serviceManager.MaterialService.GetAllMaterialsAsync(new MaterialFilterRequest(null, null, true, null, null));
            return View(request);
        }
    }

    public async Task<IActionResult> Adjust(int? materialId = null)
    {
        ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        ViewBag.Materials = await _serviceManager.MaterialService.GetAllMaterialsAsync(new MaterialFilterRequest(null, null, true, null, null));
        ViewBag.SelectedMaterialId = materialId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Adjust(StockAdjustmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            ViewBag.Materials = await _serviceManager.MaterialService.GetAllMaterialsAsync(new MaterialFilterRequest(null, null, true, null, null));
            return View(request);
        }

        try
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int userId = int.TryParse(userIdString, out var parsedId) ? parsedId : 1;

            await _serviceManager.InventoryService.AdjustStockAsync(request, userId);
            TempData["Success"] = "تم حفظ تسوية وجرد المخزون بنجاح.";
            return RedirectToAction(nameof(Stock), new { warehouseId = request.WarehouseId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            ViewBag.Materials = await _serviceManager.MaterialService.GetAllMaterialsAsync(new MaterialFilterRequest(null, null, true, null, null));
            return View(request);
        }
    }
}
