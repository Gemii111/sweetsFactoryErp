using System.Security.Claims;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class PurchaseReceiptsController : Controller
{
    private readonly IPurchaseReceiptService _receiptService;
    private readonly IPurchaseOrderService _orderService;
    private readonly ISupplierService _supplierService;
    private readonly IWarehouseService _warehouseService;
    private readonly IWarehouseLocationService _locationService;
    private readonly IMaterialService _materialService;

    public PurchaseReceiptsController(
        IPurchaseReceiptService receiptService,
        IPurchaseOrderService orderService,
        ISupplierService supplierService,
        IWarehouseService warehouseService,
        IWarehouseLocationService locationService,
        IMaterialService materialService)
    {
        _receiptService = receiptService;
        _orderService = orderService;
        _supplierService = supplierService;
        _warehouseService = warehouseService;
        _locationService = locationService;
        _materialService = materialService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var userId) ? userId : 1;
    }

    public async Task<IActionResult> Index(
        PurchaseReceiptStatus? status,
        int? purchaseOrderId,
        int? supplierId,
        int? warehouseId,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm)
    {
        var receipts = await _receiptService.GetAllReceiptsAsync(
            status, purchaseOrderId, supplierId, warehouseId, fromDate, toDate, searchTerm);
        var summary = await _receiptService.GetSummaryAsync();
        var suppliers = await _supplierService.GetAllSuppliersAsync();
        var warehouses = await _warehouseService.GetAllAsync();

        ViewBag.Suppliers = new SelectList(suppliers, "Id", "Name", supplierId);
        ViewBag.Warehouses = new SelectList(warehouses.Where(w => w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging), "Id", "Name", warehouseId);
        ViewBag.SelectedStatus = status;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Summary = summary;

        return View(receipts);
    }

    public async Task<IActionResult> Details(int id)
    {
        var receipt = await _receiptService.GetReceiptByIdAsync(id);
        if (receipt == null)
        {
            TempData["ErrorMessage"] = "محضر وسند الاستلام غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(receipt);
    }

    public async Task<IActionResult> Create(int? purchaseOrderId)
    {
        var approvedOrders = await _orderService.GetAllOrdersAsync();
        var releasableOrders = approvedOrders
            .Where(o => o.Status == PurchaseOrderStatus.Approved || o.Status == PurchaseOrderStatus.PartiallyReceived)
            .OrderByDescending(o => o.OrderDate)
            .ToList();

        var warehouses = await _warehouseService.GetAllAsync();
        var rawWarehouses = warehouses.Where(w => (w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging) && w.IsActive).ToList();
        var locations = await _locationService.GetAllLocationsAsync();

        ViewBag.PurchaseOrders = new SelectList(releasableOrders, "Id", "OrderNumber", purchaseOrderId);
        ViewBag.Warehouses = rawWarehouses;
        ViewBag.Locations = locations;

        var model = new CreatePurchaseReceiptRequest
        {
            ReceiptDate = DateTime.UtcNow.Date,
            PurchaseOrderId = purchaseOrderId ?? 0,
            WarehouseId = rawWarehouses.FirstOrDefault()?.Id ?? 0
        };

        if (purchaseOrderId.HasValue && purchaseOrderId.Value > 0)
        {
            var receivingInfo = await _orderService.GetReceivingInfoAsync(purchaseOrderId.Value);
            if (receivingInfo != null)
            {
                model.SupplierId = receivingInfo.SupplierId;
                model.WarehouseId = receivingInfo.WarehouseId > 0 ? receivingInfo.WarehouseId : model.WarehouseId;
                model.Items = receivingInfo.Items.Select(i => new CreatePurchaseReceiptItemRequest
                {
                    PurchaseOrderItemId = i.PurchaseOrderItemId,
                    MaterialId = i.MaterialId,
                    OrderedQuantity = i.OrderedQuantity,
                    ReceivedQuantity = i.RemainingQuantity,
                    AcceptedQuantity = i.RemainingQuantity,
                    RejectedQuantity = 0,
                    Unit = i.Unit,
                    UnitPrice = i.UnitPrice,
                    WarehouseId = model.WarehouseId,
                    ExpiryDate = DateTime.UtcNow.Date.AddMonths(12)
                }).ToList();
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePurchaseReceiptRequest request)
    {
        if (!ModelState.IsValid)
        {
            var approvedOrders = await _orderService.GetAllOrdersAsync();
            var releasableOrders = approvedOrders
                .Where(o => o.Status == PurchaseOrderStatus.Approved || o.Status == PurchaseOrderStatus.PartiallyReceived)
                .ToList();
            var warehouses = await _warehouseService.GetAllAsync();
            var rawWarehouses = warehouses.Where(w => (w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging) && w.IsActive).ToList();
            var locations = await _locationService.GetAllLocationsAsync();

            ViewBag.PurchaseOrders = new SelectList(releasableOrders, "Id", "OrderNumber", request.PurchaseOrderId);
            ViewBag.Warehouses = rawWarehouses;
            ViewBag.Locations = locations;
            return View(request);
        }

        try
        {
            var userId = GetCurrentUserId();
            var created = await _receiptService.CreateAndPostReceiptAsync(request, userId);
            TempData["SuccessMessage"] = $"تم ترحيل سند الاستلام المخزني [{created.ReceiptNumber}] وتحديث أرصدة الخامات وتكاليفها بنجاح.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var approvedOrders = await _orderService.GetAllOrdersAsync();
            var releasableOrders = approvedOrders
                .Where(o => o.Status == PurchaseOrderStatus.Approved || o.Status == PurchaseOrderStatus.PartiallyReceived)
                .ToList();
            var warehouses = await _warehouseService.GetAllAsync();
            var rawWarehouses = warehouses.Where(w => (w.Type == WarehouseType.RawMaterial || w.Type == WarehouseType.Packaging) && w.IsActive).ToList();
            var locations = await _locationService.GetAllLocationsAsync();

            ViewBag.PurchaseOrders = new SelectList(releasableOrders, "Id", "OrderNumber", request.PurchaseOrderId);
            ViewBag.Warehouses = rawWarehouses;
            ViewBag.Locations = locations;
            return View(request);
        }
    }
}
