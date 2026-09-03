using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FluentValidation;

namespace FactoryX.Web.Controllers;

[Authorize]
public class MaterialCategoriesController : Controller
{
    private readonly IServiceManager _serviceManager;

    public MaterialCategoriesController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index(string? search, bool? isActive)
    {
        var categories = await _serviceManager.MaterialCategoryService.GetAllCategoriesAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.Trim().ToLower();
            categories = categories.Where(c => 
                (c.Name != null && c.Name.ToLower().Contains(q)) || 
                (c.Code != null && c.Code.ToLower().Contains(q)) ||
                (c.Description != null && c.Description.ToLower().Contains(q)));
        }

        if (isActive.HasValue)
        {
            categories = categories.Where(c => c.IsActive == isActive.Value);
        }

        ViewBag.Search = search;
        ViewBag.IsActive = isActive;

        return View(categories);
    }

    public async Task<IActionResult> Details(int id)
    {
        var category = await _serviceManager.MaterialCategoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();

        // Also fetch materials under this category
        var materials = await _serviceManager.MaterialService.GetAllMaterialsAsync(new MaterialFilterRequest(
            Search: null,
            CategoryId: id,
            IsActive: null,
            StockStatus: null,
            ExpiryStatus: null));

        ViewBag.Materials = materials;
        return View(category);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateMaterialCategoryRequest request)
    {
        try
        {
            await _serviceManager.MaterialCategoryService.CreateCategoryAsync(request);
            TempData["Success"] = "تم إضافة تصنيف المواد بنجاح.";
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

        return View(request);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var category = await _serviceManager.MaterialCategoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();

        var request = new UpdateMaterialCategoryRequest(
            category.Id,
            category.Code,
            category.Name,
            category.Description,
            category.IsActive);

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateMaterialCategoryRequest request)
    {
        if (id != request.Id) return BadRequest();

        try
        {
            await _serviceManager.MaterialCategoryService.UpdateCategoryAsync(request);
            TempData["Success"] = "تم تعديل تصنيف المواد بنجاح.";
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

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var success = await _serviceManager.MaterialCategoryService.ToggleCategoryStatusAsync(id);
        if (success)
        {
            TempData["Success"] = "تم تغيير حالة تفعيل التصنيف بنجاح.";
        }
        else
        {
            TempData["Error"] = "فشل في تحديث حالة التصنيف.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _serviceManager.MaterialCategoryService.DeleteCategoryAsync(id);
            if (success)
            {
                TempData["Success"] = "تم حذف التصنيف بنجاح.";
            }
            else
            {
                TempData["Error"] = "التصنيف غير موجود.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
