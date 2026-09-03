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
public class WasteController : Controller
{
    private readonly IServiceManager _serviceManager;

    public WasteController(IServiceManager serviceManager)
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
        return 1; // Default to admin
    }

    private async Task PopulateDropdownsAsync()
    {
        ViewBag.Materials = new SelectList(await _serviceManager.MaterialService.GetActiveMaterialsAsync(), "Id", "Name");
        ViewBag.Products = new SelectList(await _serviceManager.ProductService.GetActiveProductsAsync(), "Id", "Name");
        ViewBag.Warehouses = new SelectList(await _serviceManager.WarehouseService.GetAllAsync(), "Id", "Name");
        ViewBag.WasteReasons = new SelectList(await _serviceManager.WasteReasonService.GetAllAsync(onlyActive: true), "Id", "Reason");
        ViewBag.ProductionBatches = new SelectList(await _serviceManager.ProductionBatchService.GetBatchesAsync(), "Id", "BatchNumber");
    }

    public async Task<IActionResult> Index(
        WasteType? wasteType = null,
        WasteStatus? status = null,
        int? batchId = null,
        int? productId = null,
        int? materialId = null,
        int? reasonId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var wastes = await _serviceManager.WasteService.GetAllAsync(
            wasteType, status, batchId, productId, materialId, reasonId, fromDate, toDate, searchTerm);

        var summary = await _serviceManager.WasteService.GetSummaryAsync();

        ViewBag.Summary = summary;
        ViewBag.SelectedType = wasteType;
        ViewBag.SelectedStatus = status;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        await PopulateDropdownsAsync();

        return View(wastes);
    }

    public async Task<IActionResult> Create(WasteType? type = null, int? batchId = null, int? materialId = null)
    {
        await PopulateDropdownsAsync();

        var model = new CreateWasteRequest
        {
            WasteType = type ?? WasteType.RawMaterialWaste,
            ProductionBatchId = batchId,
            MaterialId = materialId,
            WasteDate = DateTime.UtcNow
        };

        if (materialId.HasValue && materialId.Value > 0)
        {
            model.UnitCost = await _serviceManager.WasteService.EstimateUnitCostAsync(model.WasteType, materialId.Value, null, null);
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateWasteRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.WasteService.CreateAsync(request, userId);

            var statusMsg = request.SubmitDirectly
                ? "وتم تقديمه للاعتماد بنجاح!"
                : "وحفظه كمسودة بنجاح!";

            TempData["SuccessMessage"] = $"تم إنشاء سجل الهالك [{result.WasteNumber}] {statusMsg}";
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
        var waste = await _serviceManager.WasteService.GetByIdAsync(id);
        if (waste == null)
        {
            return NotFound();
        }

        if (waste.Status != WasteStatus.Draft)
        {
            TempData["ErrorMessage"] = "لا يمكن تعديل سجل الهالك إلا وهو في حالة المسودة (Draft).";
            return RedirectToAction(nameof(Details), new { id });
        }

        var request = new UpdateWasteRequest
        {
            Id = waste.Id,
            WasteType = waste.WasteType,
            ProductionBatchId = waste.ProductionBatchId,
            MaterialId = waste.MaterialId,
            ProductId = waste.ProductId,
            RawMaterialBatchNumber = waste.RawMaterialBatchNumber,
            WarehouseId = waste.WarehouseId,
            LocationId = waste.LocationId,
            Quantity = waste.Quantity,
            Unit = waste.Unit,
            UnitCost = waste.UnitCost,
            WasteReasonId = waste.WasteReasonId,
            ReasonDescription = waste.ReasonDescription,
            WasteDate = waste.WasteDate,
            Notes = waste.Notes
        };

        await PopulateDropdownsAsync();
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateWasteRequest request)
    {
        if (id != request.Id)
        {
            return BadRequest();
        }

        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.WasteService.UpdateAsync(request, userId);
            TempData["SuccessMessage"] = $"تم تحديث سجل الهالك [{result.WasteNumber}] بنجاح!";
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
        var waste = await _serviceManager.WasteService.GetByIdAsync(id);
        if (waste == null)
        {
            return NotFound();
        }

        return View(waste);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.WasteService.SubmitForApprovalAsync(id, userId);
            TempData["SuccessMessage"] = $"تم تقديم سجل الهالك [{result.WasteNumber}] للاعتماد بنجاح!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Approve(int id)
    {
        var waste = await _serviceManager.WasteService.GetByIdAsync(id);
        if (waste == null)
        {
            return NotFound();
        }

        if (waste.Status != WasteStatus.PendingApproval)
        {
            TempData["ErrorMessage"] = "السجل ليس معلقاً للاعتماد.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Fetch current stock if raw material waste
        if (waste.WasteType == WasteType.RawMaterialWaste && waste.MaterialId.HasValue && waste.WarehouseId.HasValue)
        {
            var stock = await _serviceManager.InventoryService.GetStockAsync(
                waste.WarehouseId, waste.LocationId, waste.MaterialId, null, waste.RawMaterialBatchNumber);
            ViewBag.AvailableStock = stock.Sum(s => s.Quantity);
        }

        return View(waste);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(ApproveWasteRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.WasteService.ApproveWasteAsync(request, userId);
            TempData["SuccessMessage"] = $"تم اعتماد سجل الهالك [{result.WasteNumber}] بنجاح، وإسقاط الكمية من المخزون حيثما انطبق!";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل الاعتماد: {ex.Message}";
            return RedirectToAction(nameof(Approve), new { id = request.WasteId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(RejectWasteRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.WasteService.RejectWasteAsync(request, userId);
            TempData["SuccessMessage"] = $"تم رفض اعتماد سجل الهالك [{result.WasteNumber}].";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (ValidationException ex)
        {
            TempData["ErrorMessage"] = string.Join("<br/>", ex.Errors.Select(e => e.ErrorMessage));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Approve), new { id = request.WasteId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string? reason)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.WasteService.CancelWasteAsync(id, userId, reason);
            TempData["SuccessMessage"] = $"تم إلغاء سجل الهالك [{result.WasteNumber}] بنجاح!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> GetItemCost(WasteType type, int? materialId, int? productId, int? batchId)
    {
        var cost = await _serviceManager.WasteService.EstimateUnitCostAsync(type, materialId, productId, batchId);
        string unit = "KG";

        if (materialId.HasValue && materialId.Value > 0)
        {
            var mat = await _serviceManager.MaterialService.GetMaterialByIdAsync(materialId.Value);
            if (mat != null) unit = mat.Unit;
        }
        else if (productId.HasValue && productId.Value > 0)
        {
            var prod = await _serviceManager.ProductService.GetProductByIdAsync(productId.Value);
            if (prod != null) unit = prod.Unit;
        }

        return Json(new { cost, unit });
    }

    [HttpGet]
    public async Task<IActionResult> GetMaterialLots(int warehouseId, int materialId)
    {
        var balances = await _serviceManager.InventoryService.GetStockAsync(warehouseId, null, materialId, null, null);
        var lots = balances.Select(b => new
        {
            b.LocationId,
            b.LocationName,
            b.BatchNumber,
            b.Quantity,
            b.Unit,
            ExpiryDate = b.ExpiryDate?.ToString("yyyy-MM-dd")
        });

        return Json(lots);
    }
}
