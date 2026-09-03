using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class SuppliersController : Controller
{
    private readonly ISupplierService _supplierService;
    private readonly IMaterialService _materialService;

    public SuppliersController(ISupplierService supplierService, IMaterialService materialService)
    {
        _supplierService = supplierService;
        _materialService = materialService;
    }

    public async Task<IActionResult> Index(string? searchTerm, int? categoryId, bool? isActive)
    {
        var suppliers = await _supplierService.GetAllSuppliersAsync(searchTerm, categoryId, isActive);
        var categories = await _supplierService.GetAllCategoriesAsync();
        var summary = await _supplierService.GetSummaryAsync();

        ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);
        ViewBag.SearchTerm = searchTerm;
        ViewBag.SelectedCategoryId = categoryId;
        ViewBag.SelectedIsActive = isActive;
        ViewBag.Summary = summary;

        return View(suppliers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id);
        if (supplier == null)
        {
            TempData["ErrorMessage"] = "المورد المطلوب غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        var priceHistories = await _supplierService.GetPriceHistoryAsync(supplierId: id);
        ViewBag.PriceHistories = priceHistories;

        return View(supplier);
    }

    public async Task<IActionResult> Create()
    {
        var categories = await _supplierService.GetAllCategoriesAsync(onlyActive: true);
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View(new CreateSupplierRequest { Code = $"SUP-{(new Random().Next(1000, 9999))}" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSupplierRequest request)
    {
        if (!ModelState.IsValid)
        {
            var categories = await _supplierService.GetAllCategoriesAsync(onlyActive: true);
            ViewBag.Categories = new SelectList(categories, "Id", "Name", request.CategoryId);
            return View(request);
        }

        try
        {
            var created = await _supplierService.CreateSupplierAsync(request);
            TempData["SuccessMessage"] = $"تم إضافة المورد [{created.Name}] بنجاح بالكود [{created.Code}].";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var categories = await _supplierService.GetAllCategoriesAsync(onlyActive: true);
            ViewBag.Categories = new SelectList(categories, "Id", "Name", request.CategoryId);
            return View(request);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var supplier = await _supplierService.GetSupplierByIdAsync(id);
        if (supplier == null)
        {
            TempData["ErrorMessage"] = "المورد المطلوب غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        var categories = await _supplierService.GetAllCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, "Id", "Name", supplier.CategoryId);

        var request = new UpdateSupplierRequest
        {
            Id = supplier.Id,
            Code = supplier.Code,
            Name = supplier.Name,
            ArabicName = supplier.ArabicName,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone,
            Mobile = supplier.Mobile,
            Email = supplier.Email,
            Address = supplier.Address,
            TaxNumber = supplier.TaxNumber,
            CategoryId = supplier.CategoryId,
            IsActive = supplier.IsActive,
            Notes = supplier.Notes
        };

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateSupplierRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            var categories = await _supplierService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", request.CategoryId);
            return View(request);
        }

        try
        {
            var updated = await _supplierService.UpdateSupplierAsync(request);
            TempData["SuccessMessage"] = $"تم تحديث بيانات المورد [{updated.Name}] بنجاح.";
            return RedirectToAction(nameof(Details), new { id = updated.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var categories = await _supplierService.GetAllCategoriesAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", request.CategoryId);
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        try
        {
            var newState = await _supplierService.ToggleActiveAsync(id);
            TempData["SuccessMessage"] = newState ? "تم تفعيل المورد بنجاح." : "تم تعطيل المورد بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _supplierService.DeleteSupplierAsync(id);
            TempData["SuccessMessage"] = "تم حذف المورد بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        return RedirectToAction(nameof(Index));
    }
}
