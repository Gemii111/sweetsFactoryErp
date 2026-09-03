using System.Security.Claims;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class PurchaseOrdersController : Controller
{
    private readonly IPurchaseOrderService _orderService;
    private readonly IPurchaseRequestService _requestService;
    private readonly ISupplierService _supplierService;
    private readonly IMaterialService _materialService;
    private readonly IWarehouseService _warehouseService;

    public PurchaseOrdersController(
        IPurchaseOrderService orderService,
        IPurchaseRequestService requestService,
        ISupplierService supplierService,
        IMaterialService materialService,
        IWarehouseService warehouseService)
    {
        _orderService = orderService;
        _requestService = requestService;
        _supplierService = supplierService;
        _materialService = materialService;
        _warehouseService = warehouseService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 1;
    }

    public async Task<IActionResult> Index(
        PurchaseOrderStatus? status,
        int? supplierId,
        int? warehouseId,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm)
    {
        var orders = await _orderService.GetAllOrdersAsync(
            status, supplierId, warehouseId, fromDate, toDate, searchTerm);
        var summary = await _orderService.GetSummaryAsync();
        var suppliers = await _supplierService.GetAllSuppliersAsync();
        var warehouses = await _warehouseService.GetAllAsync();

        ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", supplierId);
        ViewBag.Warehouses = new SelectList(warehouses.Where(w => w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging), "Id", "Name", warehouseId);
        ViewBag.SelectedStatus = status;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Summary = summary;

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            TempData["ErrorMessage"] = "أمر الشراء غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(order);
    }

    public async Task<IActionResult> Create(int? fromRequestId)
    {
        var suppliers = await _supplierService.GetAllSuppliersAsync(isActive: true);
        var warehouses = await _warehouseService.GetAllAsync();
        var rawWarehouses = warehouses.Where(w => (w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging) && w.IsActive).ToList();
        var materials = await _materialService.GetAllMaterialsAsync();

        ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name");
        ViewBag.Warehouses = new SelectList(rawWarehouses, "Id", "Name");
        ViewBag.Materials = materials.Where(m => m.IsActive).OrderBy(m => m.Name).ToList();

        var model = new CreatePurchaseOrderRequest
        {
            OrderDate = DateTime.UtcNow.Date,
            Currency = "EGP",
            WarehouseId = rawWarehouses.FirstOrDefault()?.Id ?? 0,
            Items = new List<CreatePurchaseOrderItemRequest>
            {
                new() { OrderedQuantity = 100 }
            }
        };

        if (fromRequestId.HasValue)
        {
            var pr = await _requestService.GetRequestByIdAsync(fromRequestId.Value);
            if (pr != null)
            {
                model.PurchaseRequestId = pr.Id;
                model.ExpectedDeliveryDate = pr.RequiredDate;
                model.Notes = $"بناءً على طلب الشراء المعتمد [{pr.RequestNumber}]".Trim();
                model.Items = pr.Items.Select(i => new CreatePurchaseOrderItemRequest
                {
                    MaterialId = i.MaterialId,
                    OrderedQuantity = i.RequestedQuantity,
                    Unit = i.Unit,
                    UnitPrice = i.EstimatedUnitPrice,
                    Notes = i.Notes
                }).ToList();
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePurchaseOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync(isActive: true);
            var warehouses = await _warehouseService.GetAllAsync();
            var rawWarehouses = warehouses.Where(w => (w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging) && w.IsActive).ToList();
            var materials = await _materialService.GetAllMaterialsAsync();

            ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", request.SupplierId);
            ViewBag.Warehouses = new SelectList(rawWarehouses, "Id", "Name", request.WarehouseId);
            ViewBag.Materials = materials.Where(m => m.IsActive).OrderBy(m => m.Name).ToList();
            return View(request);
        }

        try
        {
            var userId = GetCurrentUserId();
            var created = await _orderService.CreateOrderAsync(request, userId);
            TempData["SuccessMessage"] = $"تم إنشاء أمر الشراء [{created.OrderNumber}] بنجاح كمسودة.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var suppliers = await _supplierService.GetAllSuppliersAsync(isActive: true);
            var warehouses = await _warehouseService.GetAllAsync();
            var rawWarehouses = warehouses.Where(w => (w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging) && w.IsActive).ToList();
            var materials = await _materialService.GetAllMaterialsAsync();

            ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", request.SupplierId);
            ViewBag.Warehouses = new SelectList(rawWarehouses, "Id", "Name", request.WarehouseId);
            ViewBag.Materials = materials.Where(m => m.IsActive).OrderBy(m => m.Name).ToList();
            return View(request);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            TempData["ErrorMessage"] = "أمر الشراء غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        if (order.Status != PurchaseOrderStatus.Draft)
        {
            TempData["ErrorMessage"] = "لا يمكن تعديل أمر الشراء إلا في حالة المسودة.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var suppliers = await _supplierService.GetAllSuppliersAsync(isActive: true);
        var warehouses = await _warehouseService.GetAllAsync();
        var rawWarehouses = warehouses.Where(w => (w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging) && w.IsActive).ToList();
        var materials = await _materialService.GetAllMaterialsAsync();

        ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", order.SupplierId);
        ViewBag.Warehouses = new SelectList(rawWarehouses, "Id", "Name", order.WarehouseId);
        ViewBag.Materials = materials.Where(m => m.IsActive).OrderBy(m => m.Name).ToList();

        var request = new UpdatePurchaseOrderRequest
        {
            Id = order.Id,
            SupplierId = order.SupplierId,
            PurchaseRequestId = order.PurchaseRequestId,
            OrderDate = order.OrderDate,
            ExpectedDeliveryDate = order.ExpectedDeliveryDate,
            WarehouseId = order.WarehouseId,
            Currency = order.Currency,
            Notes = order.Notes,
            Items = order.Items.Select(i => new CreatePurchaseOrderItemRequest
            {
                MaterialId = i.MaterialId,
                OrderedQuantity = i.OrderedQuantity,
                Unit = i.Unit,
                UnitPrice = i.UnitPrice,
                DiscountAmount = i.DiscountAmount,
                TaxAmount = i.TaxAmount,
                Notes = i.Notes
            }).ToList()
        };

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdatePurchaseOrderRequest request)
    {
        if (id != request.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync(isActive: true);
            var warehouses = await _warehouseService.GetAllAsync();
            var rawWarehouses = warehouses.Where(w => (w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging) && w.IsActive).ToList();
            var materials = await _materialService.GetAllMaterialsAsync();

            ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", request.SupplierId);
            ViewBag.Warehouses = new SelectList(rawWarehouses, "Id", "Name", request.WarehouseId);
            ViewBag.Materials = materials.Where(m => m.IsActive).OrderBy(m => m.Name).ToList();
            return View(request);
        }

        try
        {
            var updated = await _orderService.UpdateOrderAsync(request);
            TempData["SuccessMessage"] = $"تم تحديث أمر الشراء [{updated.OrderNumber}] بنجاح.";
            return RedirectToAction(nameof(Details), new { id = updated.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var suppliers = await _supplierService.GetAllSuppliersAsync(isActive: true);
            var warehouses = await _warehouseService.GetAllAsync();
            var rawWarehouses = warehouses.Where(w => (w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging) && w.IsActive).ToList();
            var materials = await _materialService.GetAllMaterialsAsync();

            ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", request.SupplierId);
            ViewBag.Warehouses = new SelectList(rawWarehouses, "Id", "Name", request.WarehouseId);
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
            var updated = await _orderService.SubmitOrderAsync(id, userId);
            TempData["SuccessMessage"] = $"تم تقديم أمر الشراء [{updated.OrderNumber}] للاعتماد.";
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
            var updated = await _orderService.ApproveOrderAsync(id, userId);
            TempData["SuccessMessage"] = $"تم اعتماد أمر الشراء [{updated.OrderNumber}] بنجاح، وأصبح جاهزاً للاستلام المخزني.";
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
            var updated = await _orderService.CancelOrderAsync(id, userId, reason);
            TempData["SuccessMessage"] = $"تم إلغاء أمر الشراء [{updated.OrderNumber}].";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id, string? reason)
    {
        try
        {
            var userId = GetCurrentUserId();
            var updated = await _orderService.CloseOrderAsync(id, userId, reason);
            TempData["SuccessMessage"] = $"تم إغلاق أمر الشراء [{updated.OrderNumber}].";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Route("PurchaseOrders/GetReceivingInfo/{id}")]
    public async Task<IActionResult> GetReceivingInfo(int id)
    {
        var info = await _orderService.GetReceivingInfoAsync(id);
        if (info == null) return NotFound();
        return Json(info);
    }
}
