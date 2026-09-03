using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FluentValidation;

namespace FactoryX.Web.Controllers;

[Authorize]
public class ProductCategoriesController : Controller
{
    private readonly IServiceManager _serviceManager;

    public ProductCategoriesController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index(string? search, bool? isActive)
    {
        var categories = await _serviceManager.ProductCategoryService.GetAllCategoriesAsync();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            categories = categories.Where(c =>
                c.Name.ToLower().Contains(term) ||
                c.Code.ToLower().Contains(term) ||
                (c.ArabicName != null && c.ArabicName.ToLower().Contains(term)) ||
                (c.Description != null && c.Description.ToLower().Contains(term)));
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
        var category = await _serviceManager.ProductCategoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();
        return View(category);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductCategoryRequest request)
    {
        try
        {
            var created = await _serviceManager.ProductCategoryService.CreateCategoryAsync(request);
            TempData["Success"] = $"تم إضافة تصنيف المنتج '{created.Name}' بنجاح.";
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
        var category = await _serviceManager.ProductCategoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();

        var request = new UpdateProductCategoryRequest(
            category.Id,
            category.Code,
            category.Name,
            category.ArabicName,
            category.Description,
            category.IsActive);

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateProductCategoryRequest request)
    {
        if (id != request.Id) return BadRequest();

        try
        {
            var updated = await _serviceManager.ProductCategoryService.UpdateCategoryAsync(request);
            TempData["Success"] = $"تم تعديل تصنيف المنتج '{updated.Name}' بنجاح.";
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
        var result = await _serviceManager.ProductCategoryService.ToggleActiveAsync(id);
        if (!result) return NotFound();

        TempData["Success"] = "تم تغيير حالة تنشيط التصنيف بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _serviceManager.ProductCategoryService.DeleteCategoryAsync(id);
            TempData["Success"] = "تم حذف تصنيف المنتج بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
