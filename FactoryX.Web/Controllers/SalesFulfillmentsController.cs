using System.Security.Claims;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class SalesFulfillmentsController : Controller
{
    private readonly ISalesFulfillmentService _fulfillmentService;
    private readonly ISalesOrderService _orderService;
    private readonly ICustomerService _customerService;
    private readonly IWarehouseService _warehouseService;
    private readonly IWarehouseLocationService _locationService;

    public SalesFulfillmentsController(
        ISalesFulfillmentService fulfillmentService,
        ISalesOrderService orderService,
        ICustomerService customerService,
        IWarehouseService warehouseService,
        IWarehouseLocationService locationService)
    {
        _fulfillmentService = fulfillmentService;
        _orderService = orderService;
        _customerService = customerService;
        _warehouseService = warehouseService;
        _locationService = locationService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int id))
        {
            return id;
        }
        return 1;
    }

    public async Task<IActionResult> Index(
        SalesFulfillmentStatus? status,
        int? salesOrderId,
        int? customerId,
        int? warehouseId,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm)
    {
        var fulfillments = await _fulfillmentService.GetAllFulfillmentsAsync(
            status, salesOrderId, customerId, warehouseId, fromDate, toDate, searchTerm);
        var summary = await _fulfillmentService.GetSummaryAsync();
        var customers = await _customerService.GetAllCustomersAsync();
        var warehouses = await _warehouseService.GetAllAsync();

        ViewBag.Customers = new SelectList(customers, "Id", "Name", customerId);
        ViewBag.Warehouses = new SelectList(warehouses.Where(w => w.Type == WarehouseType.FinishedGoods), "Id", "Name", warehouseId);
        ViewBag.SelectedStatus = status;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Summary = summary;

        return View(fulfillments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var fulfillment = await _fulfillmentService.GetFulfillmentByIdAsync(id);
        if (fulfillment == null)
        {
            TempData["ErrorMessage"] = "سند صرف وتسليم المبيعات غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(fulfillment);
    }

    public async Task<IActionResult> Create(int? salesOrderId)
    {
        var confirmedOrders = (await _orderService.GetAllOrdersAsync())
            .Where(o => (o.Status == SalesOrderStatus.Confirmed || o.Status == SalesOrderStatus.PartiallyFulfilled) && o.RemainingQuantity > 0)
            .ToList();

        var warehouses = await _warehouseService.GetAllAsync();
        var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
        var locations = await _locationService.GetAllLocationsAsync();

        ViewBag.SalesOrders = new SelectList(confirmedOrders, "Id", "OrderNumber", salesOrderId);
        ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name");
        ViewBag.Locations = locations.Where(l => l.IsActive).ToList();

        var model = new CreateSalesFulfillmentRequest
        {
            SalesOrderId = salesOrderId ?? (confirmedOrders.FirstOrDefault()?.Id ?? 0),
            FulfillmentDate = DateTime.UtcNow.Date
        };

        if (salesOrderId.HasValue && salesOrderId.Value > 0)
        {
            var order = await _orderService.GetOrderByIdAsync(salesOrderId.Value);
            if (order != null)
            {
                model.CustomerId = order.CustomerId;
                model.WarehouseId = order.WarehouseId;
            }
        }
        else if (confirmedOrders.Any())
        {
            var first = confirmedOrders.First();
            model.CustomerId = first.CustomerId;
            model.WarehouseId = first.WarehouseId;
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSalesFulfillmentRequest request)
    {
        if (!ModelState.IsValid)
        {
            await PopulateCreateViewBags(request.SalesOrderId);
            return View(request);
        }

        try
        {
            var userId = GetCurrentUserId();
            var created = await _fulfillmentService.CreateFulfillmentAsync(request, userId);
            TempData["SuccessMessage"] = $"تم ترحيل وصرف شحنة المبيعات بنجاح برقم [{created.FulfillmentNumber}] وخصم الأرصدة من مخزون المنتجات التامة.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await PopulateCreateViewBags(request.SalesOrderId);
            return View(request);
        }
    }

    private async Task PopulateCreateViewBags(int? selectedOrderId)
    {
        var confirmedOrders = (await _orderService.GetAllOrdersAsync())
            .Where(o => (o.Status == SalesOrderStatus.Confirmed || o.Status == SalesOrderStatus.PartiallyFulfilled) && o.RemainingQuantity > 0)
            .ToList();

        var warehouses = await _warehouseService.GetAllAsync();
        var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
        var locations = await _locationService.GetAllLocationsAsync();

        ViewBag.SalesOrders = new SelectList(confirmedOrders, "Id", "OrderNumber", selectedOrderId);
        ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name");
        ViewBag.Locations = locations.Where(l => l.IsActive).ToList();
    }
}
