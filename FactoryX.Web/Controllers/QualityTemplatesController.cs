using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class QualityTemplatesController : Controller
{
    private readonly IServiceManager _serviceManager;

    public QualityTemplatesController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewBag.Categories = new SelectList(await _serviceManager.ProductCategoryService.GetAllCategoriesAsync(), "Id", "Name");
        ViewBag.Products = new SelectList(await _serviceManager.ProductService.GetActiveProductsAsync(), "Id", "Name");
    }

    public async Task<IActionResult> Index(bool onlyActive = false, int? categoryId = null, int? productId = null)
    {
        var templates = await _serviceManager.QualityTemplateService.GetAllTemplatesAsync(onlyActive, categoryId, productId);
        ViewBag.OnlyActive = onlyActive;
        await PopulateDropdownsAsync();
        return View(templates);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdownsAsync();
        var model = new CreateQualityTemplateRequest
        {
            IsActive = true,
            Items = new List<CreateQualityTemplateItemRequest>
            {
                new() { SpecificationName = "الوزن الصافي (Net Weight)", Sequence = 1, IsRequired = true, DataType = Domain.Entities.InspectionDataType.Number, Unit = "G" },
                new() { SpecificationName = "المظهر واللون (Appearance & Color)", Sequence = 2, IsRequired = true, DataType = Domain.Entities.InspectionDataType.PassFail },
                new() { SpecificationName = "القوام والملمس (Texture)", Sequence = 3, IsRequired = true, DataType = Domain.Entities.InspectionDataType.PassFail },
                new() { SpecificationName = "المذاق والرائحة (Taste & Aroma)", Sequence = 4, IsRequired = true, DataType = Domain.Entities.InspectionDataType.PassFail }
            }
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQualityTemplateRequest request)
    {
        try
        {
            var result = await _serviceManager.QualityTemplateService.CreateTemplateAsync(request);
            TempData["SuccessMessage"] = $"تم إنشاء قالب فحص الجودة [{result.Code} - {result.Name}] بنجاح!";
            return RedirectToAction(nameof(Details), new { id = result.Id });
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

        await PopulateDropdownsAsync();
        return View(request);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var template = await _serviceManager.QualityTemplateService.GetTemplateByIdAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        await PopulateDropdownsAsync();

        var request = new UpdateQualityTemplateRequest
        {
            Id = template.Id,
            Code = template.Code,
            Name = template.Name,
            Description = template.Description,
            ProductCategoryId = template.ProductCategoryId,
            ProductId = template.ProductId,
            IsActive = template.IsActive,
            Notes = template.Notes,
            Items = template.Items.Select(i => new CreateQualityTemplateItemRequest
            {
                SpecificationName = i.SpecificationName,
                Description = i.Description,
                Sequence = i.Sequence,
                IsRequired = i.IsRequired,
                DataType = i.DataType,
                MinValue = i.MinValue,
                MaxValue = i.MaxValue,
                TargetValue = i.TargetValue,
                AllowedTextValues = i.AllowedTextValues,
                Unit = i.Unit,
                Notes = i.Notes
            }).ToList()
        };

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateQualityTemplateRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }

        try
        {
            var result = await _serviceManager.QualityTemplateService.UpdateTemplateAsync(request);
            TempData["SuccessMessage"] = $"تم تحديث قالب الفحص [{result.Code}] بنجاح!";
            return RedirectToAction(nameof(Details), new { id = result.Id });
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

        await PopulateDropdownsAsync();
        return View(request);
    }

    public async Task<IActionResult> Details(int id)
    {
        var template = await _serviceManager.QualityTemplateService.GetTemplateByIdAsync(id);
        if (template == null)
        {
            return NotFound();
        }

        return View(template);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        try
        {
            var isActive = await _serviceManager.QualityTemplateService.ToggleActiveAsync(id);
            var status = isActive ? "تفعيل" : "تعطيل";
            TempData["SuccessMessage"] = $"تم {status} قالب الفحص بنجاح!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
