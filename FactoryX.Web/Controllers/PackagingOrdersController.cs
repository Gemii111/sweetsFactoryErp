using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

public class PackagingOrdersController : Controller
{
    private readonly IServiceManager _serviceManager;

    public PackagingOrdersController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index(
        PackagingOrderStatus? status,
        int? batchId,
        int? productId,
        int? bomId,
        int? operatorId,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm)
    {
        var orders = await _serviceManager.PackagingOrderService.GetAllOrdersAsync(
            status, batchId, productId, bomId, operatorId, fromDate, toDate, searchTerm);

        var products = await _serviceManager.ProductService.GetActiveProductsAsync();
        var operators = await _serviceManager.OperatorService.GetAllAsync();

        ViewBag.Products = new SelectList(products, "Id", "Name", productId);
        ViewBag.Operators = new SelectList(operators, "Id", "Name", operatorId);
        ViewBag.SelectedStatus = status;
        ViewBag.SearchTerm = searchTerm;

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var order = await _serviceManager.PackagingOrderService.GetOrderByIdAsync(id);
            var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            var activeWarehouses = warehouses.Where(w => w.IsActive).ToList();

            ViewBag.Warehouses = new SelectList(activeWarehouses, "Id", "Name");

            return View(order);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "أمر التعبئة والتغليف المطلوب غير موجود.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? batchId)
    {
        var batches = await _serviceManager.ProductionBatchService.GetBatchesAsync();
        var completedBatches = batches.Where(b => b.Status == ProductionBatchStatus.Completed || b.ActualOutputQuantity > 0).ToList();
        var boms = await _serviceManager.PackagingBOMService.GetAllBOMsAsync(onlyActive: true);
        var operators = await _serviceManager.OperatorService.GetAllAsync();

        ViewBag.Batches = new SelectList(completedBatches, "Id", "BatchNumber", batchId);
        ViewBag.PackagingBOMs = boms;
        ViewBag.Operators = new SelectList(operators, "Id", "Name");
        ViewBag.SelectedBatchId = batchId;

        var model = new CreatePackagingOrderRequest
        {
            ProductionBatchId = batchId ?? 0,
            PlannedQuantity = 100
        };

        if (batchId.HasValue && batchId.Value > 0)
        {
            var batch = completedBatches.FirstOrDefault(b => b.Id == batchId.Value);
            if (batch != null)
            {
                var matchingBom = boms.FirstOrDefault(b => b.ProductId == batch.ProductId);
                if (matchingBom != null)
                {
                    model.PackagingBOMId = matchingBom.Id;
                    var packSizeKg = matchingBom.PackSizeKg > 0 ? matchingBom.PackSizeKg : 1.0m;
                    var outputKg = batch.ActualOutputQuantity > 0 ? batch.ActualOutputQuantity : batch.PlannedQuantity;
                    model.PlannedQuantity = packSizeKg > 0 ? Math.Floor(outputKg / packSizeKg) : 100;
                }
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePackagingOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            var batches = await _serviceManager.ProductionBatchService.GetBatchesAsync();
            var completedBatches = batches.Where(b => b.Status == ProductionBatchStatus.Completed || b.ActualOutputQuantity > 0).ToList();
            var boms = await _serviceManager.PackagingBOMService.GetAllBOMsAsync(onlyActive: true);
            var operators = await _serviceManager.OperatorService.GetAllAsync();
            ViewBag.Batches = new SelectList(completedBatches, "Id", "BatchNumber", request.ProductionBatchId);
            ViewBag.PackagingBOMs = boms;
            ViewBag.Operators = new SelectList(operators, "Id", "Name", request.OperatorId);
            return View(request);
        }

        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingOrderService.CreateOrderAsync(request, userId);
            TempData["SuccessMessage"] = $"تم إنشاء أمر التعبئة والتغليف [{result.OrderNumber}] بنجاح!";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var batches = await _serviceManager.ProductionBatchService.GetBatchesAsync();
            var completedBatches = batches.Where(b => b.Status == ProductionBatchStatus.Completed || b.ActualOutputQuantity > 0).ToList();
            var boms = await _serviceManager.PackagingBOMService.GetAllBOMsAsync(onlyActive: true);
            var operators = await _serviceManager.OperatorService.GetAllAsync();
            ViewBag.Batches = new SelectList(completedBatches, "Id", "BatchNumber", request.ProductionBatchId);
            ViewBag.PackagingBOMs = boms;
            ViewBag.Operators = new SelectList(operators, "Id", "Name", request.OperatorId);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Execute(int id)
    {
        try
        {
            var order = await _serviceManager.PackagingOrderService.GetOrderByIdAsync(id);
            if (order.Status == PackagingOrderStatus.Completed)
            {
                TempData["InfoMessage"] = "أمر التعبئة هذا مكتمل بالفعل.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            var activeWarehouses = warehouses.Where(w => w.IsActive).ToList();

            ViewBag.Warehouses = new SelectList(activeWarehouses, "Id", "Name");

            var model = new ExecutePackagingOrderRequest
            {
                PackagingOrderId = order.Id,
                ActualPackagedQuantity = order.PlannedQuantity,
                WarehouseId = activeWarehouses.FirstOrDefault()?.Id ?? 1
            };

            return View(order);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "أمر التعبئة والتغليف المطلوب غير موجود.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Execute(ExecutePackagingOrderRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingOrderService.ExecuteAndCompleteOrderAsync(request, userId);
            TempData["SuccessMessage"] = $"تم إتمام تنفيذ أمر التعبئة [{result.OrderNumber}] وصرف مواد التعبئة بنجاح ({result.ActualQuantity} عبوة)!";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل تنفيذ أمر التعبئة: {ex.Message}";
            return RedirectToAction(nameof(Execute), new { id = request.PackagingOrderId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingOrderService.StartOrderAsync(id, userId);
            TempData["SuccessMessage"] = $"تم بدء تشغيل أمر التعبئة [{result.OrderNumber}] بنجاح!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل بدء أمر التعبئة: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pause(PausePackagingOrderRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingOrderService.PauseOrderAsync(request, userId);
            TempData["SuccessMessage"] = $"تم إيقاف أمر التعبئة [{result.OrderNumber}] مؤقتاً.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل إيقاف أمر التعبئة: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = request.PackagingOrderId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resume(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingOrderService.ResumeOrderAsync(id, userId);
            TempData["SuccessMessage"] = $"تم استئناف تشغيل أمر التعبئة [{result.OrderNumber}] بنجاح!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل استئناف أمر التعبئة: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(CancelPackagingOrderRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingOrderService.CancelOrderAsync(request, userId);
            TempData["SuccessMessage"] = $"تم إلغاء أمر التعبئة [{result.OrderNumber}] بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل إلغاء أمر التعبئة: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = request.PackagingOrderId });
    }

    [HttpGet]
    public async Task<IActionResult> GetBatchInfo(int batchId)
    {
        try
        {
            var batch = await _serviceManager.ProductionBatchService.GetBatchByIdAsync(batchId);
            if (batch == null)
            {
                return Json(new { success = false, message = "دفعة الإنتاج غير موجودة." });
            }

            var gate = await _serviceManager.QualityGateService.CanReleaseBatchAsync(batchId);
            var boms = await _serviceManager.PackagingBOMService.GetAllBOMsAsync(onlyActive: true, productId: batch.ProductId);

            var batchOutputKg = batch.ActualOutputQuantity > 0 ? batch.ActualOutputQuantity : batch.PlannedQuantity;

            return Json(new
            {
                success = true,
                batch = new
                {
                    id = batch.Id,
                    batchNumber = batch.BatchNumber,
                    productId = batch.ProductId,
                    productName = batch.ProductName,
                    outputKg = batchOutputKg,
                    status = batch.StatusName
                },
                qcGate = gate,
                applicableBOMs = boms.Select(b => new
                {
                    id = b.Id,
                    code = b.Code,
                    name = b.Name,
                    packSize = b.PackSize,
                    packSizeKg = b.PackSizeKg,
                    packUnit = b.PackUnit,
                    activeVersionNumber = b.ActiveVersionNumber,
                    costPerPack = b.TotalPackagingMaterialCost,
                    maxPacks = b.PackSizeKg > 0 ? Math.Floor(batchOutputKg / b.PackSizeKg) : 0
                })
            });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRequirements(int bomId, decimal quantity, int? versionId, int? warehouseId)
    {
        try
        {
            var requirements = await _serviceManager.PackagingOrderService.CalculateOrderRequirementsAsync(
                bomId, quantity, versionId, warehouseId);

            var allSufficient = requirements.All(r => r.IsSufficient);
            return Json(new { success = true, requirements, allSufficient });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int id))
        {
            return id;
        }
        return 1;
    }
}
