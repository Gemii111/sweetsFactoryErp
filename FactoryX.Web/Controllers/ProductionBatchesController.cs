using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FactoryX.Web.Controllers;

[Authorize]
public class ProductionBatchesController : Controller
{
    private readonly IServiceManager _serviceManager;

    public ProductionBatchesController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    private int GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.TryParse(idClaim, out var id) ? id : 1;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string? search = null,
        int? workOrderId = null,
        int? productId = null,
        ProductionBatchStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var filter = new ProductionBatchFilterRequest(search, workOrderId, productId, status, fromDate, toDate);
        var batches = await _serviceManager.ProductionBatchService.GetBatchesAsync(filter);
        var summary = await _serviceManager.ProductionBatchService.GetSummaryAsync();

        ViewBag.Filter = filter;
        ViewBag.Summary = summary;
        ViewBag.Products = await _serviceManager.ProductService.GetAllAsync();
        ViewBag.WorkOrders = await _serviceManager.WorkOrderService.GetProductionOrdersAsync();

        return View(batches);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? workOrderId = null)
    {
        var model = new CreateProductionBatchRequest
        {
            ProductionDate = DateTime.UtcNow.Date,
            OutputUnit = "KG"
        };

        if (workOrderId.HasValue && workOrderId.Value > 0)
        {
            var order = await _serviceManager.WorkOrderService.GetProductionOrderByIdAsync(workOrderId.Value);
            if (order != null)
            {
                model.WorkOrderId = order.Id;
                model.PlannedQuantity = order.PlannedQuantity;
                model.OutputUnit = order.OutputUnit;
                model.ProductionLineId = order.ProductionLineId;
                model.WorkCenterId = order.WorkCenterId;
                model.MachineId = order.MachineId;
                model.OperatorId = order.OperatorId;
                model.ShiftId = order.ShiftId;
                ViewBag.SelectedOrder = order;
            }
        }

        model.BatchNumber = await _serviceManager.ProductionBatchService.GenerateBatchNumberAsync();
        await PopulateViewBagsAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateProductionBatchRequest request)
    {
        try
        {
            var created = await _serviceManager.ProductionBatchService.CreateBatchAsync(request);
            TempData["SuccessMessage"] = $"تم إنشاء دفعة الإنتاج رقم [{created.BatchNumber}] بنجاح، وجاهزة للتشغيل.";
            return RedirectToAction(nameof(Execute), new { id = created.Id });
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

        await PopulateViewBagsAsync();
        return View(request);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var batch = await _serviceManager.ProductionBatchService.GetBatchByIdAsync(id);
        if (batch == null)
        {
            TempData["ErrorMessage"] = $"دفعة الإنتاج بالمعرف #{id} غير موجودة.";
            return RedirectToAction(nameof(Index));
        }

        return View(batch);
    }

    [HttpGet]
    public async Task<IActionResult> Execute(int id)
    {
        try
        {
            var details = await _serviceManager.ProductionExecutionService.GetExecutionDetailsAsync(id);
            return View(details);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(StartBatchRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var batch = await _serviceManager.ProductionExecutionService.StartBatchAsync(request, userId);
            TempData["SuccessMessage"] = $"تم بدء تشغيل الدفعة [{batch.BatchNumber}] وصرف الخامات من المخزون بنجاح!";
            return RedirectToAction(nameof(Execute), new { id = request.BatchId });
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = string.Join("<br/>", ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Execute), new { id = request.BatchId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pause(int id, string? reason)
    {
        try
        {
            var userId = GetCurrentUserId();
            var batch = await _serviceManager.ProductionExecutionService.PauseBatchAsync(id, reason, userId);
            TempData["SuccessMessage"] = $"تم إيقاف تشغيل الدفعة [{batch.BatchNumber}] مؤقتاً.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Execute), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resume(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var batch = await _serviceManager.ProductionExecutionService.ResumeBatchAsync(id, userId);
            TempData["SuccessMessage"] = $"تم استئناف تشغيل الدفعة [{batch.BatchNumber}] بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Execute), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(CompleteBatchRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var batch = await _serviceManager.ProductionExecutionService.CompleteBatchAsync(request, userId);
            TempData["SuccessMessage"] = $"تم إكمال تشغيل الدفعة [{batch.BatchNumber}] بنجاح وتسجيل الإنتاج الفعلي ({batch.ActualOutputQuantity:N2} {batch.OutputUnit})!";
            return RedirectToAction(nameof(Details), new { id = request.BatchId });
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = string.Join("<br/>", ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Execute), new { id = request.BatchId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(CancelBatchRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var batch = await _serviceManager.ProductionExecutionService.CancelBatchAsync(request, userId);
            TempData["SuccessMessage"] = $"تم إلغاء دفعة الإنتاج [{batch.BatchNumber}].";
            return RedirectToAction(nameof(Details), new { id = request.BatchId });
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = string.Join("<br/>", ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Execute), new { id = request.BatchId });
    }

    [HttpGet]
    public async Task<IActionResult> GetOrderDetailsJson(int orderId)
    {
        var order = await _serviceManager.WorkOrderService.GetProductionOrderByIdAsync(orderId);
        if (order == null) return NotFound();

        return Json(new
        {
            orderId = order.Id,
            orderNumber = order.OrderNumber,
            productId = order.ProductId,
            productName = order.ProductDisplayName,
            recipeVersionId = order.RecipeVersionId,
            recipeVersionName = $"{order.RecipeName} ({order.RecipeVersionNumber})",
            plannedQuantity = order.PlannedQuantity,
            outputUnit = order.OutputUnit,
            machineId = order.MachineId,
            operatorId = order.OperatorId,
            shiftId = order.ShiftId,
            productionLineId = order.ProductionLineId,
            workCenterId = order.WorkCenterId
        });
    }

    private async Task PopulateViewBagsAsync()
    {
        var orders = (await _serviceManager.WorkOrderService.GetProductionOrdersAsync())
            .Where(o => o.OrderStatus != ProductionOrderStatus.Cancelled && o.OrderStatus != ProductionOrderStatus.Completed)
            .ToList();

        ViewBag.Orders = orders;
        ViewBag.Machines = await _serviceManager.MachineService.GetAllAsync();
        ViewBag.Operators = await _serviceManager.OperatorService.GetAllAsync();
        ViewBag.Shifts = await _serviceManager.ShiftService.GetAllAsync();
        ViewBag.Warehouses = await _serviceManager.WarehouseService.GetAllAsync();
    }
}
