using System.Security.Claims;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class SalesOrdersController : Controller
{
    private readonly ISalesOrderService _salesOrderService;
    private readonly ICustomerService _customerService;
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;

    public SalesOrdersController(
        ISalesOrderService salesOrderService,
        ICustomerService customerService,
        IProductService productService,
        IWarehouseService warehouseService)
    {
        _salesOrderService = salesOrderService;
        _customerService = customerService;
        _productService = productService;
        _warehouseService = warehouseService;
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
        SalesOrderStatus? status,
        int? customerId,
        int? warehouseId,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm)
    {
        var orders = await _salesOrderService.GetAllOrdersAsync(status, customerId, warehouseId, fromDate, toDate, searchTerm);
        var summary = await _salesOrderService.GetSummaryAsync();
        var customers = await _customerService.GetAllCustomersAsync();
        var warehouses = await _warehouseService.GetAllAsync();

        ViewBag.Customers = new SelectList(customers, "Id", "Name", customerId);
        ViewBag.Warehouses = new SelectList(warehouses.Where(w => w.Type == WarehouseType.FinishedGoods), "Id", "Name", warehouseId);
        ViewBag.SelectedStatus = status;
        ViewBag.SearchTerm = searchTerm;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.Summary = summary;

        return View(orders);
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _salesOrderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            TempData["ErrorMessage"] = "أمر البيع غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(order);
    }

    public async Task<IActionResult> Create(int? customerId)
    {
        var customers = await _customerService.GetAllCustomersAsync(isActive: true);
        var warehouses = await _warehouseService.GetAllAsync();
        var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
        var products = await _productService.GetActiveProductsAsync();

        ViewBag.Customers = new SelectList(customers, "Id", "Name", customerId);
        ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name");
        ViewBag.Products = products.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();

        var model = new CreateSalesOrderRequest
        {
            CustomerId = customerId ?? 0,
            WarehouseId = fgWarehouses.FirstOrDefault()?.Id ?? 0,
            OrderDate = DateTime.UtcNow.Date,
            Priority = SalesOrderPriority.Normal,
            Items = new List<CreateSalesOrderItemRequest>
            {
                new() { OrderedQuantity = 10 }
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSalesOrderRequest request)
    {
        if (!ModelState.IsValid)
        {
            var customers = await _customerService.GetAllCustomersAsync(isActive: true);
            var warehouses = await _warehouseService.GetAllAsync();
            var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
            var products = await _productService.GetActiveProductsAsync();

            ViewBag.Customers = new SelectList(customers, "Id", "Name", request.CustomerId);
            ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", request.WarehouseId);
            ViewBag.Products = products.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
            return View(request);
        }

        try
        {
            var created = await _salesOrderService.CreateOrderAsync(request);
            TempData["SuccessMessage"] = $"تم إنشاء أمر البيع '{created.OrderNumber}' بنجاح كمسودة.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            var customers = await _customerService.GetAllCustomersAsync(isActive: true);
            var warehouses = await _warehouseService.GetAllAsync();
            var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
            var products = await _productService.GetActiveProductsAsync();

            ViewBag.Customers = new SelectList(customers, "Id", "Name", request.CustomerId);
            ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", request.WarehouseId);
            ViewBag.Products = products.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
            return View(request);
        }
    }

    public async Task<IActionResult> Edit(int id)
    {
        var order = await _salesOrderService.GetOrderByIdAsync(id);
        if (order == null)
        {
            TempData["ErrorMessage"] = "أمر البيع غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        if (order.Status != SalesOrderStatus.Draft)
        {
            TempData["ErrorMessage"] = "لا يمكن تعديل أمر البيع بعد اعتماده.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var customers = await _customerService.GetAllCustomersAsync(isActive: true);
        var warehouses = await _warehouseService.GetAllAsync();
        var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
        var products = await _productService.GetActiveProductsAsync();

        ViewBag.Customers = new SelectList(customers, "Id", "Name", order.CustomerId);
        ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", order.WarehouseId);
        ViewBag.Products = products.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();

        var model = new UpdateSalesOrderRequest
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            WarehouseId = order.WarehouseId,
            OrderDate = order.OrderDate,
            RequiredDeliveryDate = order.RequiredDeliveryDate,
            Priority = order.Priority,
            Notes = order.Notes,
            SubTotal = order.SubTotal,
            DiscountAmount = order.DiscountAmount,
            TaxAmount = order.TaxAmount,
            TotalAmount = order.TotalAmount,
            Items = order.Items.Select(i => new CreateSalesOrderItemRequest
            {
                ProductId = i.ProductId,
                OrderedQuantity = i.OrderedQuantity,
                Unit = i.Unit,
                UnitPrice = i.UnitPrice,
                DiscountAmount = i.DiscountAmount,
                TaxAmount = i.TaxAmount,
                TotalPrice = i.TotalPrice,
                Notes = i.Notes
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateSalesOrderRequest request)
    {
        if (id != request.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            var customers = await _customerService.GetAllCustomersAsync(isActive: true);
            var warehouses = await _warehouseService.GetAllAsync();
            var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
            var products = await _productService.GetActiveProductsAsync();

            ViewBag.Customers = new SelectList(customers, "Id", "Name", request.CustomerId);
            ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", request.WarehouseId);
            ViewBag.Products = products.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
            return View(request);
        }

        try
        {
            var updated = await _salesOrderService.UpdateOrderAsync(request);
            TempData["SuccessMessage"] = $"تم تحديث أمر البيع '{updated.OrderNumber}' بنجاح.";
            return RedirectToAction(nameof(Details), new { id = updated.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            var customers = await _customerService.GetAllCustomersAsync(isActive: true);
            var warehouses = await _warehouseService.GetAllAsync();
            var fgWarehouses = warehouses.Where(w => w.Type == WarehouseType.FinishedGoods && w.IsActive).ToList();
            var products = await _productService.GetActiveProductsAsync();

            ViewBag.Customers = new SelectList(customers, "Id", "Name", request.CustomerId);
            ViewBag.Warehouses = new SelectList(fgWarehouses, "Id", "Name", request.WarehouseId);
            ViewBag.Products = products.Where(p => p.IsActive).OrderBy(p => p.Name).ToList();
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _salesOrderService.ConfirmOrderAsync(id, userId);
            TempData["SuccessMessage"] = "تم اعتماد أمر البيع بنجاح وجاهز للصرف والتسليم.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل اعتماد أمر البيع: {ex.Message}";
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
            await _salesOrderService.CancelOrderAsync(id, reason, userId);
            TempData["SuccessMessage"] = "تم إلغاء أمر البيع بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل إلغاء أمر البيع: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _salesOrderService.CloseOrderAsync(id, userId);
            TempData["SuccessMessage"] = "تم إغلاق أمر البيع بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل إغلاق أمر البيع: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    [Route("SalesOrders/GetFulfillmentInfo/{id}")]
    public async Task<IActionResult> GetFulfillmentInfo(int id)
    {
        var info = await _salesOrderService.GetFulfillmentInfoAsync(id);
        if (info == null)
        {
            return NotFound(new { message = "أمر البيع غير موجود." });
        }

        return Json(info);
    }
}
