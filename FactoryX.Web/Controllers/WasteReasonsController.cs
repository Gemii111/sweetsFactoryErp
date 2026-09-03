using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class WasteReasonsController : Controller
{
    private readonly IServiceManager _serviceManager;

    public WasteReasonsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index(bool onlyActive = false)
    {
        var reasons = await _serviceManager.WasteReasonService.GetAllAsync(onlyActive);
        ViewBag.OnlyActive = onlyActive;
        return View(reasons);
    }

    public IActionResult Create()
    {
        return View(new CreateWasteReasonRequest { IsActive = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWasteReasonRequest request)
    {
        try
        {
            var result = await _serviceManager.WasteReasonService.CreateAsync(request);
            TempData["SuccessMessage"] = $"تم إضافة سبب الهالك [{result.Code} - {result.Reason}] بنجاح!";
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
        var reason = await _serviceManager.WasteReasonService.GetByIdAsync(id);
        if (reason == null)
        {
            return NotFound();
        }

        var request = new UpdateWasteReasonRequest
        {
            Id = reason.Id,
            Code = reason.Code,
            Reason = reason.Reason,
            Description = reason.Description,
            IsActive = reason.IsActive
        };

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateWasteReasonRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }

        try
        {
            var result = await _serviceManager.WasteReasonService.UpdateAsync(request);
            TempData["SuccessMessage"] = $"تم تحديث سبب الهالك [{result.Code}] بنجاح!";
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

    public async Task<IActionResult> Details(int id)
    {
        var reason = await _serviceManager.WasteReasonService.GetByIdAsync(id);
        if (reason == null)
        {
            return NotFound();
        }

        return View(reason);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        try
        {
            var isActive = await _serviceManager.WasteReasonService.ToggleActiveAsync(id);
            var status = isActive ? "تفعيل" : "تعطيل";
            TempData["SuccessMessage"] = $"تم {status} سبب الهالك بنجاح!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
