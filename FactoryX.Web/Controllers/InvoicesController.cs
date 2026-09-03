using System.Security.Claims;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class InvoicesController : Controller
{
    private readonly IInvoiceService _invoiceService;
    private readonly ISalesOrderService _salesOrderService;
    private readonly ICustomerService _customerService;

    public InvoicesController(
        IInvoiceService invoiceService,
        ISalesOrderService salesOrderService,
        ICustomerService customerService)
    {
        _invoiceService = invoiceService;
        _salesOrderService = salesOrderService;
        _customerService = customerService;
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
        InvoiceStatus? status,
        int? customerId,
        int? salesOrderId,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm)
    {
        var invoices = await _invoiceService.GetAllInvoicesAsync(
            status, customerId, salesOrderId, fromDate, toDate, searchTerm);
        var summary = await _invoiceService.GetSummaryAsync();
        var customers = await _customerService.GetAllCustomersAsync();

        ViewBag.Customers = new SelectList(customers, "Id", "Name", customerId);
        ViewBag.Summary = summary;
        ViewBag.SelectedStatus = status;
        ViewBag.SelectedCustomerId = customerId;
        ViewBag.SelectedSalesOrderId = salesOrderId;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.SearchTerm = searchTerm;

        return View(invoices);
    }

    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            TempData["ErrorMessage"] = "الفاتورة المطلوبة غير موجودة.";
            return RedirectToAction(nameof(Index));
        }

        return View(invoice);
    }

    public async Task<IActionResult> Create(int? customerId = null, int? salesOrderId = null)
    {
        var customers = (await _customerService.GetAllCustomersAsync()).Where(c => c.IsActive).ToList();
        var invoiceableOrders = await _invoiceService.GetInvoiceableOrdersAsync(customerId);

        ViewBag.Customers = new SelectList(customers, "Id", "Name", customerId);
        ViewBag.InvoiceableOrders = invoiceableOrders;
        ViewBag.SelectedSalesOrderId = salesOrderId;
        ViewBag.NextInvoiceNumber = await _invoiceService.GenerateNextInvoiceNumberAsync();

        var model = new CreateInvoiceRequest
        {
            CustomerId = customerId ?? 0,
            SalesOrderId = salesOrderId ?? 0,
            InvoiceDate = DateTime.UtcNow.Date,
            TaxRate = 14.00m,
            IssueImmediately = true
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoiceableOrderDetails(int orderId)
    {
        var orders = await _invoiceService.GetInvoiceableOrdersAsync();
        var order = orders.FirstOrDefault(o => o.SalesOrderId == orderId);
        if (order == null)
        {
            return Json(new { success = false, message = "لا توجد كميات مسلمة متاحة للفوترة في هذا الأمر." });
        }

        return Json(new { success = true, order });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateInvoiceRequest request)
    {
        if (request.Items == null || !request.Items.Any(i => i.Quantity > 0))
        {
            TempData["ErrorMessage"] = "يجب إدراج بند واحد على الأقل بكمية أكبر من الصفر.";
            return RedirectToAction(nameof(Create), new { customerId = request.CustomerId, salesOrderId = request.SalesOrderId });
        }

        try
        {
            var invoice = await _invoiceService.CreateInvoiceAsync(request, GetCurrentUserId());
            TempData["SuccessMessage"] = $"تم إنشاء الفاتورة بنجاح برقم [{invoice.InvoiceNumber}].";
            return RedirectToAction(nameof(Details), new { id = invoice.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"تعذر إنشاء الفاتورة: {ex.Message}";
            return RedirectToAction(nameof(Create), new { customerId = request.CustomerId, salesOrderId = request.SalesOrderId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Issue(int id)
    {
        try
        {
            var invoice = await _invoiceService.IssueInvoiceAsync(id, GetCurrentUserId());
            TempData["SuccessMessage"] = $"تم اعتماد وإصدار الفاتورة [{invoice.InvoiceNumber}] بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"تعذر إصدار الفاتورة: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            TempData["ErrorMessage"] = "يجب إدخال سبب إلغاء الفاتورة.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            await _invoiceService.CancelInvoiceAsync(id, reason, GetCurrentUserId());
            TempData["SuccessMessage"] = "تم إلغاء الفاتورة بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"تعذر إلغاء الفاتورة: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Print(int id)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }

        return View(invoice);
    }
}
