using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

public class FinishedGoodsReleasesController : Controller
{
    private readonly IServiceManager _serviceManager;

    public FinishedGoodsReleasesController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index(
        int? productId,
        int? batchId,
        int? warehouseId,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm)
    {
        var releases = await _serviceManager.FinishedGoodsReleaseService.GetAllReleasesAsync(
            productId, batchId, warehouseId, fromDate, toDate, searchTerm);

        var products = await _serviceManager.ProductService.GetActiveProductsAsync();
        var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();

        ViewBag.Products = new SelectList(products, "Id", "Name", productId);
        ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", warehouseId);
        ViewBag.SelectedProductId = productId;
        ViewBag.SelectedWarehouseId = warehouseId;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        return View(releases);
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var release = await _serviceManager.FinishedGoodsReleaseService.GetReleaseByIdAsync(id);
            return View(release);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "سجل إفراج المنتج التام المطلوب غير موجود.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? batchId)
    {
        var batches = await _serviceManager.ProductionBatchService.GetBatchesAsync();
        var candidateBatches = batches.Where(b => b.Status == ProductionBatchStatus.Completed || b.ActualOutputQuantity > 0).ToList();

        var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
        var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();

        ViewBag.Batches = new SelectList(candidateBatches, "Id", "BatchNumber", batchId);
        ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name");
        ViewBag.SelectedBatchId = batchId;

        var model = new CreateFinishedGoodsReleaseRequest
        {
            ProductionBatchId = batchId ?? 0,
            WarehouseId = fgWarehouses.FirstOrDefault()?.Id ?? 0
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateFinishedGoodsReleaseRequest request)
    {
        if (!ModelState.IsValid)
        {
            var batches = await _serviceManager.ProductionBatchService.GetBatchesAsync();
            var candidateBatches = batches.Where(b => b.Status == ProductionBatchStatus.Completed || b.ActualOutputQuantity > 0).ToList();
            var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();

            ViewBag.Batches = new SelectList(candidateBatches, "Id", "BatchNumber", request.ProductionBatchId);
            ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", request.WarehouseId);
            return View(request);
        }

        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.FinishedGoodsReleaseService.ReleaseFinishedGoodsAsync(request, userId);
            TempData["SuccessMessage"] = $"تم الإفراج عن المنتج التام بنجاح بموجب سند إفراج رقم [{result.ReleaseNumber}] بكمية ({result.Quantity} {result.Unit})!";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var batches = await _serviceManager.ProductionBatchService.GetBatchesAsync();
            var candidateBatches = batches.Where(b => b.Status == ProductionBatchStatus.Completed || b.ActualOutputQuantity > 0).ToList();
            var warehouses = await _serviceManager.WarehouseService.GetAllAsync();
            var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();

            ViewBag.Batches = new SelectList(candidateBatches, "Id", "BatchNumber", request.ProductionBatchId);
            ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", request.WarehouseId);
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetBatchReleaseInfo(int batchId)
    {
        try
        {
            var availability = await _serviceManager.FinishedGoodsReleaseService.GetReleaseAvailabilityAsync(batchId);
            return Json(new { success = true, data = availability });
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
