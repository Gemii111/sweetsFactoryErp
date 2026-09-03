using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class CustomersController : Controller
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(string? searchTerm, CustomerType? type, bool? isActive)
    {
        var customers = await _customerService.GetAllCustomersAsync(searchTerm, type, isActive);
        var summary = await _customerService.GetSummaryAsync();

        ViewBag.SearchTerm = searchTerm;
        ViewBag.SelectedType = type;
        ViewBag.SelectedIsActive = isActive;
        ViewBag.Summary = summary;

        return View(customers);
    }

    public async Task<IActionResult> Details(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            TempData["ErrorMessage"] = "العميل المطلوب غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(customer);
    }

    public async Task<IActionResult> Create()
    {
        var nextCode = await _customerService.GenerateNextCustomerCodeAsync();
        return View(new CreateCustomerRequest { Code = nextCode });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            var created = await _customerService.CreateCustomerAsync(request);
            TempData["SuccessMessage"] = $"تم إنشاء العميل '{created.Name}' بنجاح برمز [{created.Code}].";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var customer = await _customerService.GetCustomerByIdAsync(id);
        if (customer == null)
        {
            TempData["ErrorMessage"] = "العميل المطلوب غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        var updateRequest = new UpdateCustomerRequest
        {
            Id = customer.Id,
            Code = customer.Code,
            Name = customer.Name,
            ArabicName = customer.ArabicName,
            Type = customer.Type,
            ContactPerson = customer.ContactPerson,
            Phone = customer.Phone,
            Mobile = customer.Mobile,
            Email = customer.Email,
            Address = customer.Address,
            TaxNumber = customer.TaxNumber,
            Notes = customer.Notes,
            CreditLimit = customer.CreditLimit,
            IsActive = customer.IsActive
        };

        return View(updateRequest);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateCustomerRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(request);
        }

        try
        {
            var updated = await _customerService.UpdateCustomerAsync(request);
            TempData["SuccessMessage"] = $"تم تحديث بيانات العميل '{updated.Name}' بنجاح.";
            return RedirectToAction(nameof(Details), new { id = updated.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        try
        {
            var newState = await _customerService.ToggleActiveStatusAsync(id);
            TempData["SuccessMessage"] = newState
                ? "تم تفعيل العميل بنجاح."
                : "تم إلغاء تفعيل العميل (تعطيل مؤقت).";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل تغيير حالة العميل: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
