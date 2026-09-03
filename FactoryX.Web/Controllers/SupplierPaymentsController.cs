using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FactoryX.Web.Controllers;

[Authorize]
public class SupplierPaymentsController : Controller
{
    private readonly ISupplierPaymentService _supplierPaymentService;
    private readonly ISupplierService _supplierService;
    private readonly IPurchaseReceiptService _receiptService;
    private readonly IPurchaseOrderService _orderService;

    public SupplierPaymentsController(
        ISupplierPaymentService supplierPaymentService,
        ISupplierService supplierService,
        IPurchaseReceiptService receiptService,
        IPurchaseOrderService orderService)
    {
        _supplierPaymentService = supplierPaymentService;
        _supplierService = supplierService;
        _receiptService = receiptService;
        _orderService = orderService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 1;
    }

    public async Task<IActionResult> Index()
    {
        var payments = await _supplierPaymentService.GetAllPaymentsAsync();
        return View(payments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var payment = await _supplierPaymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            TempData["ErrorMessage"] = "سند الصرف غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(payment);
    }

    public async Task<IActionResult> Receipt(int id)
    {
        var payment = await _supplierPaymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            TempData["ErrorMessage"] = "سند الصرف غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(payment);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? supplierId = null, int? receiptId = null, int? orderId = null)
    {
        var suppliers = await _supplierService.GetAllSuppliersAsync();
        var receipts = await _receiptService.GetAllReceiptsAsync();
        var orders = await _orderService.GetAllOrdersAsync();

        ViewBag.Suppliers = suppliers;
        ViewBag.Receipts = receipts;
        ViewBag.Orders = orders;

        var dto = new SupplierPaymentCreateDto
        {
            SupplierId = supplierId ?? 0,
            PurchaseReceiptId = receiptId,
            PurchaseOrderId = orderId,
            PaymentDate = DateTime.UtcNow.Date,
            Amount = 0
        };

        if (receiptId.HasValue && receiptId.Value > 0)
        {
            var receipt = receipts.FirstOrDefault(r => r.Id == receiptId.Value);
            if (receipt != null)
            {
                dto.SupplierId = receipt.SupplierId;
                dto.Amount = receipt.TotalCost;
            }
        }

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SupplierPaymentCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var receipts = await _receiptService.GetAllReceiptsAsync();
            var orders = await _orderService.GetAllOrdersAsync();

            ViewBag.Suppliers = suppliers;
            ViewBag.Receipts = receipts;
            ViewBag.Orders = orders;

            return View(dto);
        }

        try
        {
            var userId = GetCurrentUserId();
            var payment = await _supplierPaymentService.RecordPaymentAsync(dto, userId);
            TempData["SuccessMessage"] = $"تم تسجيل سند الصرف [{payment.PaymentNumber}] وترحيل قيده المحاسبي بنجاح.";
            return RedirectToAction(nameof(Details), new { id = payment.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var receipts = await _receiptService.GetAllReceiptsAsync();
            var orders = await _orderService.GetAllOrdersAsync();

            ViewBag.Suppliers = suppliers;
            ViewBag.Receipts = receipts;
            ViewBag.Orders = orders;

            return View(dto);
        }
    }
}
