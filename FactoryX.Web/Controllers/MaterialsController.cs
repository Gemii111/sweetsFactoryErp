using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FluentValidation;

namespace FactoryX.Web.Controllers;

[Authorize]
public class MaterialsController : Controller
{
    private readonly IServiceManager _serviceManager;

    public MaterialsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        bool? isActive,
        MaterialStockStatus? stockStatus,
        string? expiryStatus)
    {
        var filter = new MaterialFilterRequest(search, categoryId, isActive, stockStatus, expiryStatus);
        var materials = await _serviceManager.MaterialService.GetAllMaterialsAsync(filter);
        var summary = await _serviceManager.MaterialService.GetStockSummaryAsync();
        var categories = await _serviceManager.MaterialCategoryService.GetAllCategoriesAsync();

        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.IsActive = isActive;
        ViewBag.StockStatus = stockStatus;
        ViewBag.ExpiryStatus = expiryStatus;
        ViewBag.Summary = summary;
        ViewBag.Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name", categoryId);

        return View(materials);
    }

    public async Task<IActionResult> Details(int id)
    {
        var material = await _serviceManager.MaterialService.GetMaterialByIdAsync(id);
        if (material == null) return NotFound();

        var stockBalances = await _serviceManager.MaterialService.GetMaterialStockBalancesAsync(id);
        var recentTransactions = await _serviceManager.MaterialService.GetMaterialRecentTransactionsAsync(id, 15);

        ViewBag.StockBalances = stockBalances;
        ViewBag.RecentTransactions = recentTransactions;

        return View(material);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMaterialRequest request)
    {
        try
        {
            var created = await _serviceManager.MaterialService.CreateMaterialAsync(request);
            TempData["Success"] = $"تم إضافة مادة الخام '{created.Name}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await PopulateDropdownsAsync(request.MaterialCategoryId, request.WarehouseId);
        return View(request);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var material = await _serviceManager.MaterialService.GetMaterialByIdAsync(id);
        if (material == null) return NotFound();

        var request = new UpdateMaterialRequest(
            material.Id,
            material.Code,
            material.SKU,
            material.Name,
            material.ArabicName,
            material.Description,
            material.MaterialCategoryId,
            material.Unit,
            material.PurchaseUnit,
            material.ConversionFactor,
            material.MinimumStock,
            material.ReorderLevel,
            material.MaximumStock,
            material.StandardCost,
            material.CurrentCost,
            material.LastPurchaseCost,
            material.WarehouseId,
            material.BatchNumber,
            material.ManufacturingDate,
            material.ExpiryDate,
            material.IsActive);

        await PopulateDropdownsAsync(material.MaterialCategoryId, material.WarehouseId);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateMaterialRequest request)
    {
        if (id != request.Id) return BadRequest();

        try
        {
            var updated = await _serviceManager.MaterialService.UpdateMaterialAsync(request);
            TempData["Success"] = $"تم تعديل بيانات مادة الخام '{updated.Name}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await PopulateDropdownsAsync(request.MaterialCategoryId, request.WarehouseId);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var success = await _serviceManager.MaterialService.ToggleMaterialStatusAsync(id);
        if (success)
        {
            TempData["Success"] = "تم تحديث حالة تفعيل مادة الخام بنجاح.";
        }
        else
        {
            TempData["Error"] = "فشل في تحديث حالة مادة الخام.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _serviceManager.MaterialService.DeleteMaterialAsync(id);
            if (success)
            {
                TempData["Success"] = "تم حذف مادة الخام بنجاح.";
            }
            else
            {
                TempData["Error"] = "مادة الخام غير موجودة.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdownsAsync(int? selectedCategoryId = null, int? selectedWarehouseId = null)
    {
        var categories = await _serviceManager.MaterialCategoryService.GetAllCategoriesAsync();
        ViewBag.Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name", selectedCategoryId);

        var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        ViewBag.Warehouses = new SelectList(warehouses.Where(w => w.IsActive), "Id", "Name", selectedWarehouseId);
    }
}
