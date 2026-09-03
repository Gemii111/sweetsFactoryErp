using System.Security.Claims;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class QualityInspectionsController : Controller
{
    private readonly IServiceManager _serviceManager;

    public QualityInspectionsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out var id))
        {
            return id;
        }
        return 1;
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewBag.Batches = new SelectList(await _serviceManager.ProductionBatchService.GetBatchesAsync(), "Id", "BatchNumber");
        ViewBag.Products = new SelectList(await _serviceManager.ProductService.GetActiveProductsAsync(), "Id", "Name");
        ViewBag.Templates = new SelectList(await _serviceManager.QualityTemplateService.GetAllTemplatesAsync(onlyActive: true), "Id", "Name");
        ViewBag.Users = new SelectList(await _serviceManager.OperatorService.GetAllAsync(), "Id", "Name");
    }

    public async Task<IActionResult> Index(
        QualityInspectionStatus? status = null,
        QualityDecision? decision = null,
        int? batchId = null,
        int? orderId = null,
        int? productId = null,
        int? inspectorId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var inspections = await _serviceManager.QualityInspectionService.GetAllInspectionsAsync(
            status, decision, batchId, orderId, productId, inspectorId, fromDate, toDate, searchTerm);

        var summary = await _serviceManager.QualityInspectionService.GetSummaryAsync();

        ViewBag.Summary = summary;
        ViewBag.SelectedStatus = status;
        ViewBag.SelectedDecision = decision;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        await PopulateDropdownsAsync();

        return View(inspections);
    }

    public async Task<IActionResult> Create(int? batchId = null)
    {
        await PopulateDropdownsAsync();

        var model = new CreateQualityInspectionRequest
        {
            ProductionBatchId = batchId ?? 0,
            InspectionDate = DateTime.UtcNow
        };

        if (batchId.HasValue && batchId.Value > 0)
        {
            var batch = await _serviceManager.ProductionBatchService.GetBatchByIdAsync(batchId.Value);
            if (batch != null)
            {
                var template = await _serviceManager.QualityTemplateService.GetApplicableTemplateForProductAsync(batch.ProductId);
                if (template != null)
                {
                    model.QualityTemplateId = template.Id;
                }
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQualityInspectionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.QualityInspectionService.CreateInspectionAsync(request, userId);
            TempData["SuccessMessage"] = $"تم فتح سجل فحص الجودة [{result.InspectionNumber}] بنجاح! يرجى إدخال القياسات المطلوبة.";
            return RedirectToAction(nameof(Inspect), new { id = result.Id });
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

    public async Task<IActionResult> Inspect(int id)
    {
        var inspection = await _serviceManager.QualityInspectionService.GetInspectionByIdAsync(id);
        if (inspection == null)
        {
            return NotFound();
        }

        return View(inspection);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecordMeasurements(RecordInspectionMeasurementsRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.QualityInspectionService.RecordMeasurementsAsync(request, userId);
            TempData["SuccessMessage"] = "تم تقييم وحفظ القياسات الفعلية بنجاح!";
            return RedirectToAction(nameof(Inspect), new { id = result.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Inspect), new { id = request.InspectionId });
        }
    }

    public async Task<IActionResult> Details(int id)
    {
        var inspection = await _serviceManager.QualityInspectionService.GetInspectionByIdAsync(id);
        if (inspection == null)
        {
            return NotFound();
        }

        if (inspection.ProductionBatchId.HasValue)
        {
            ViewBag.GateStatus = await _serviceManager.QualityGateService.CanReleaseBatchAsync(inspection.ProductionBatchId.Value);
            ViewBag.History = await _serviceManager.QualityInspectionService.GetInspectionHistoryForBatchAsync(inspection.ProductionBatchId.Value);
        }

        return View(inspection);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.QualityInspectionService.SubmitInspectionAsync(id, userId);
            TempData["SuccessMessage"] = $"تم تقديم فحص الجودة [{result.InspectionNumber}] للاعتماد بنجاح!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(ApproveInspectionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.QualityInspectionService.ApproveInspectionAsync(request, userId);
            TempData["SuccessMessage"] = $"تم اعتماد دفعة الإنتاج بنجاح بموجب فحص الجودة [{result.InspectionNumber}]!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل الاعتماد: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = request.InspectionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(RejectInspectionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.QualityInspectionService.RejectInspectionAsync(request, userId);
            TempData["SuccessMessage"] = $"تم رفض دفعة الإنتاج بموجب فحص الجودة [{result.InspectionNumber}].";
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = string.Join("<br/>", ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = request.InspectionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Hold(HoldInspectionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.QualityInspectionService.HoldInspectionAsync(request, userId);
            TempData["SuccessMessage"] = $"تم احتجاز (Hold) دفعة الإنتاج بموجب فحص الجودة [{result.InspectionNumber}].";
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = string.Join("<br/>", ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = request.InspectionId });
    }

    public async Task<IActionResult> Reinspect(int id)
    {
        var inspection = await _serviceManager.QualityInspectionService.GetInspectionByIdAsync(id);
        if (inspection == null)
        {
            return NotFound();
        }

        await PopulateDropdownsAsync();

        var model = new ReinspectRequest
        {
            PreviousInspectionId = inspection.Id,
            QualityTemplateId = inspection.QualityTemplateId,
            InspectorId = inspection.InspectorId
        };

        ViewBag.PreviousInspection = inspection;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reinspect(ReinspectRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.QualityInspectionService.ReinspectAsync(request, userId);
            TempData["SuccessMessage"] = $"تم فتح فحص إعادة تقييم جديد برقم [{result.InspectionNumber}] مرتبط بالفحص السابق!";
            return RedirectToAction(nameof(Inspect), new { id = result.Id });
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
        ViewBag.PreviousInspection = await _serviceManager.QualityInspectionService.GetInspectionByIdAsync(request.PreviousInspectionId);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.QualityInspectionService.CancelInspectionAsync(id, userId, reason);
            TempData["SuccessMessage"] = $"تم إلغاء فحص الجودة [{result.InspectionNumber}] بنجاح!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetReleaseGateStatus(int batchId)
    {
        var result = await _serviceManager.QualityGateService.CanReleaseBatchAsync(batchId);
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetTemplateForBatch(int batchId)
    {
        var batch = await _serviceManager.ProductionBatchService.GetBatchByIdAsync(batchId);
        if (batch == null) return Json(null);

        var template = await _serviceManager.QualityTemplateService.GetApplicableTemplateForProductAsync(batch.ProductId);
        return Json(new
        {
            batchId = batch.Id,
            batchNumber = batch.BatchNumber,
            productId = batch.ProductId,
            productName = batch.ProductName,
            templateId = template?.Id,
            templateName = template?.Name,
            templateCode = template?.Code
        });
    }
}
