using System.Security.Claims;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IInvoiceService _invoiceService;
    private readonly ICustomerService _customerService;

    public PaymentsController(
        IPaymentService paymentService,
        IInvoiceService invoiceService,
        ICustomerService customerService)
    {
        _paymentService = paymentService;
        _invoiceService = invoiceService;
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
        int? invoiceId,
        int? customerId,
        PaymentMethod? method,
        PaymentStatus? status,
        DateTime? fromDate,
        DateTime? toDate,
        string? searchTerm)
    {
        var payments = await _paymentService.GetAllPaymentsAsync(
            invoiceId, customerId, method, status, fromDate, toDate, searchTerm);
        var summary = await _paymentService.GetSummaryAsync();
        var customers = await _customerService.GetAllCustomersAsync();

        ViewBag.Customers = new SelectList(customers, "Id", "Name", customerId);
        ViewBag.Summary = summary;
        ViewBag.SelectedMethod = method;
        ViewBag.SelectedStatus = status;
        ViewBag.SelectedCustomerId = customerId;
        ViewBag.SelectedInvoiceId = invoiceId;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.SearchTerm = searchTerm;

        return View(payments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            TempData["ErrorMessage"] = "سند السداد المطلوب غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(payment);
    }

    public async Task<IActionResult> Create(int? invoiceId = null)
    {
        var payableInvoices = (await _invoiceService.GetAllInvoicesAsync())
            .Where(i => i.Status == InvoiceStatus.Issued || i.Status == InvoiceStatus.PartiallyPaid)
            .ToList();

        ViewBag.Invoices = payableInvoices;
        ViewBag.NextPaymentNumber = await _paymentService.GenerateNextPaymentNumberAsync();

        var model = new CreatePaymentRequest
        {
            PaymentDate = DateTime.UtcNow.Date,
            PaymentMethod = PaymentMethod.Cash
        };

        if (invoiceId.HasValue && invoiceId.Value > 0)
        {
            var selectedInvoice = payableInvoices.FirstOrDefault(i => i.Id == invoiceId.Value);
            if (selectedInvoice != null)
            {
                model.InvoiceId = selectedInvoice.Id;
                model.CustomerId = selectedInvoice.CustomerId;
                model.Amount = selectedInvoice.RemainingAmount;
                model.Currency = selectedInvoice.Currency;
                ViewBag.SelectedInvoice = selectedInvoice;
            }
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> GetInvoicePaymentDetails(int invoiceId)
    {
        var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        if (invoice == null)
        {
            return Json(new { success = false, message = "الفاتورة غير موجودة." });
        }

        return Json(new
        {
            success = true,
            invoiceId = invoice.Id,
            invoiceNumber = invoice.InvoiceNumber,
            customerId = invoice.CustomerId,
            customerName = invoice.CustomerName,
            customerCode = invoice.CustomerCode,
            totalAmount = invoice.TotalAmount,
            paidAmount = invoice.PaidAmount,
            remainingAmount = invoice.RemainingAmount,
            currency = invoice.Currency,
            status = invoice.Status.ToString()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePaymentRequest request)
    {
        if (request.Amount <= 0)
        {
            TempData["ErrorMessage"] = "يجب أن يكون مبلغ السداد أكبر من الصفر.";
            return RedirectToAction(nameof(Create), new { invoiceId = request.InvoiceId });
        }

        try
        {
            var payment = await _paymentService.CreatePaymentAsync(request, GetCurrentUserId());
            TempData["SuccessMessage"] = $"تم تسجيل سند القبض بنجاح برقم [{payment.PaymentNumber}].";
            return RedirectToAction(nameof(Details), new { id = payment.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"تعذر تسجيل السند: {ex.Message}";
            return RedirectToAction(nameof(Create), new { invoiceId = request.InvoiceId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Void(VoidPaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            TempData["ErrorMessage"] = "يجب إدخال سبب إلغاء السند.";
            return RedirectToAction(nameof(Details), new { id = request.PaymentId });
        }

        try
        {
            await _paymentService.VoidPaymentAsync(request, GetCurrentUserId());
            TempData["SuccessMessage"] = "تم إلغاء/استرداد السند بنجاح وتحديث رصيد الفاتورة المرتبطة.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"تعذر إلغاء السند: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = request.PaymentId });
    }

    public async Task<IActionResult> Receipt(int id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        if (payment == null)
        {
            return NotFound();
        }

        return View(payment);
    }
}
