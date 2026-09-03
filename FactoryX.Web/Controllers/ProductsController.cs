using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FluentValidation;

namespace FactoryX.Web.Controllers;

[Authorize]
public class ProductsController : Controller
{
    private readonly IServiceManager _serviceManager;

    public ProductsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index(
        string? search,
        int? categoryId,
        bool? isActive,
        ProductType? productType)
    {
        var filter = new ProductFilterRequest(search, categoryId, isActive, productType);
        var products = await _serviceManager.ProductService.GetAllProductsAsync(filter);
        var summary = await _serviceManager.ProductService.GetProductSummaryAsync();
        var categories = await _serviceManager.ProductCategoryService.GetAllCategoriesAsync();

        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.IsActive = isActive;
        ViewBag.ProductType = productType;
        ViewBag.Summary = summary;
        ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);

        return View(products);
    }

    public async Task<IActionResult> Details(int id)
    {
        var product = await _serviceManager.ProductService.GetProductDetailsAsync(id);
        if (product == null) return NotFound();
        return View(product);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductRequest request)
    {
        try
        {
            var created = await _serviceManager.ProductService.CreateProductAsync(request);
            TempData["Success"] = $"تم تعريف المنتج التام '{created.Name}' بنجاح.";
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

        await PopulateDropdowns(request.ProductCategoryId);
        return View(request);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _serviceManager.ProductService.GetProductByIdAsync(id);
        if (product == null) return NotFound();

        var request = new UpdateProductRequest(
            product.Id,
            product.Code,
            product.SKU,
            product.Barcode,
            product.Name,
            product.ArabicName,
            product.Description,
            product.ProductCategoryId,
            product.ProductType,
            product.Weight,
            product.WeightUnit,
            product.Unit,
            product.SellingPrice,
            product.WholesalePrice,
            product.DistributorPrice,
            product.StandardCost,
            product.MinimumStock,
            product.ExpiryPeriod,
            product.ExpiryUnit,
            product.IsActive);

        await PopulateDropdowns(product.ProductCategoryId);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateProductRequest request)
    {
        if (id != request.Id) return BadRequest();

        try
        {
            var updated = await _serviceManager.ProductService.UpdateProductAsync(request);
            TempData["Success"] = $"تم تحديث بيانات المنتج التام '{updated.Name}' بنجاح.";
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

        await PopulateDropdowns(request.ProductCategoryId);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _serviceManager.ProductService.ToggleActiveAsync(id);
        if (!result) return NotFound();

        TempData["Success"] = "تم تغيير حالة تنشيط المنتج بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _serviceManager.ProductService.DeleteProductAsync(id);
            TempData["Success"] = "تم حذف المنتج بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns(int? selectedCategoryId = null)
    {
        var categories = await _serviceManager.ProductCategoryService.GetAllCategoriesAsync();
        ViewBag.Categories = new SelectList(categories.Where(c => c.IsActive), "Id", "Name", selectedCategoryId);
    }
}