using System.Security.Claims;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class PurchaseRequestsController : Controller
{
    private readonly IPurchaseRequestService _requestService;
    private readonly IMaterialService _materialService;

    public PurchaseRequestsController(
        IPurchaseRequestService requestService,
        IMaterialService materialService)
    {
        _requestService = requestService;
        _materialService = materialService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 1;
    }

    public async Task<IActionResult> Index(
        PurchaseRequestStatus? status,
        int? departmentId,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm)
    {
        var requests = await _requestService.GetAllRequestsAsync(
            status, departmentId, null, fromDate, toDate, searchTerm);

        ViewBag.SelectedStatus = status;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        return View(requests);
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _requestService.GetRequestByIdAsync(id);
        if (request == null)
        {
            TempData["ErrorMessage"] = "طلب الشراء غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(request);
    }

    public async Task<IActionResult> Create()
    {
        var materials = await _materialService.GetAllMaterialsAsync();
        ViewBag.Materials = materials.Where(m => m.IsActive).OrderBy(m => m.Name).ToList();

        var model = new CreatePurchaseRequest
        {
            RequestDate = DateTime.UtcNow.Date,
            Priority = "Normal",
            Items = new List<CreatePurchaseRequestItemRequest>
            {
                new() { RequestedQuantity = 100 }
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePurchaseRequest request)
    {
        if (!ModelState.IsValid)
        {
            var materials = await _materialService.GetAllMaterialsAsync();
            ViewBag.Materials = materials.Where(m => m.IsActive).OrderBy(m => m.Name).ToList();
            return View(request);
        }

        try
        {
            var userId = GetCurrentUserId();
            var created = await _requestService.CreateRequestAsync(request, userId);
            TempData["SuccessMessage"] = $"تم حفظ طلب الشراء [{created.RequestNumber}] بنجاح كمسودة.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var materials = await _materialService.GetAllMaterialsAsync();
            ViewBag.Materials = materials.Where(m => m.IsActive).OrderBy(m => m.Name).ToList();
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var updated = await _requestService.SubmitRequestAsync(id, userId);
            TempData["SuccessMessage"] = $"تم تقديم طلب الشراء [{updated.RequestNumber}] للاعتماد.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var updated = await _requestService.ApproveRequestAsync(id, userId);
            TempData["SuccessMessage"] = $"تم اعتماد طلب الشراء [{updated.RequestNumber}] بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string? reason)
    {
        try
        {
            var userId = GetCurrentUserId();
            var updated = await _requestService.RejectRequestAsync(id, userId, reason);
            TempData["SuccessMessage"] = $"تم رفض طلب الشراء [{updated.RequestNumber}].";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        try
        {
            var userId = GetCurrentUserId();
            var updated = await _requestService.CancelRequestAsync(id, userId, reason);
            TempData["SuccessMessage"] = $"تم إلغاء طلب الشراء [{updated.RequestNumber}].";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
