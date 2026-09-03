using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FactoryX.Web.Controllers;

[Authorize]
public class JournalEntriesController : Controller
{
    private readonly IJournalEntryService _journalService;
    private readonly IAccountService _accountService;
    private readonly IAccountingPeriodService _periodService;
    private readonly ICustomerService _customerService;
    private readonly ISupplierService _supplierService;
    private readonly IProductService _productService;
    private readonly IMaterialService _materialService;

    public JournalEntriesController(
        IJournalEntryService journalService,
        IAccountService accountService,
        IAccountingPeriodService periodService,
        ICustomerService customerService,
        ISupplierService supplierService,
        IProductService productService,
        IMaterialService materialService)
    {
        _journalService = journalService;
        _accountService = accountService;
        _periodService = periodService;
        _customerService = customerService;
        _supplierService = supplierService;
        _productService = productService;
        _materialService = materialService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 1;
    }

    public async Task<IActionResult> Index(
        int? periodId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        JournalEntryStatus? status = null,
        JournalReferenceType? referenceType = null,
        string? searchTerm = null)
    {
        var journals = await _journalService.GetJournalsAsync(periodId, fromDate, toDate, status, referenceType, searchTerm);
        var periods = await _periodService.GetAllPeriodsAsync();

        ViewBag.Periods = periods;
        ViewBag.SelectedPeriodId = periodId;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
        ViewBag.SelectedStatus = status;
        ViewBag.SelectedReferenceType = referenceType;
        ViewBag.SearchTerm = searchTerm;

        return View(journals);
    }

    public async Task<IActionResult> Details(int id)
    {
        var journal = await _journalService.GetJournalByIdAsync(id);
        if (journal == null)
        {
            TempData["ErrorMessage"] = "القيد اليومي غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(journal);
    }

    public async Task<IActionResult> Print(int id)
    {
        var journal = await _journalService.GetJournalByIdAsync(id);
        if (journal == null)
        {
            TempData["ErrorMessage"] = "القيد اليومي غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        return View(journal);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var accounts = await _accountService.GetActivePostableAccountsAsync();
        var customers = await _customerService.GetAllCustomersAsync();
        var suppliers = await _supplierService.GetAllSuppliersAsync();
        var products = await _productService.GetActiveProductsAsync();
        var materials = await _materialService.GetAllMaterialsAsync();

        ViewBag.Accounts = accounts;
        ViewBag.Customers = customers;
        ViewBag.Suppliers = suppliers;
        ViewBag.Products = products;
        ViewBag.Materials = materials;

        var dto = new JournalEntryCreateDto
        {
            EntryDate = DateTime.UtcNow.Date,
            Description = "قيد تسوية محاسبي يدوي",
            Lines = new List<JournalEntryLineCreateDto>
            {
                new JournalEntryLineCreateDto { Debit = 0, Credit = 0 },
                new JournalEntryLineCreateDto { Debit = 0, Credit = 0 }
            }
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(JournalEntryCreateDto dto)
    {
        if (dto.Lines != null)
        {
            // Filter out empty rows where both debit and credit are zero
            dto.Lines = dto.Lines.Where(l => l.AccountId > 0 && (l.Debit > 0 || l.Credit > 0)).ToList();
        }

        if (dto.Lines == null || dto.Lines.Count < 2)
        {
            ModelState.AddModelError("", "يجب إضافة بندين على الأقل في القيد (طرف مدين وطرف دائن).");
        }
        else
        {
            var totalDebit = dto.Lines.Sum(l => l.Debit);
            var totalCredit = dto.Lines.Sum(l => l.Credit);
            if (totalDebit <= 0 || Math.Abs(totalDebit - totalCredit) >= 0.01m)
            {
                ModelState.AddModelError("", $"القيد غير متوازن! إجمالي المدين ({totalDebit:N2}) لا يساوي إجمالي الدائن ({totalCredit:N2}).");
            }
        }

        if (!ModelState.IsValid)
        {
            var accounts = await _accountService.GetActivePostableAccountsAsync();
            var customers = await _customerService.GetAllCustomersAsync();
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var products = await _productService.GetActiveProductsAsync();
            var materials = await _materialService.GetAllMaterialsAsync();

            ViewBag.Accounts = accounts;
            ViewBag.Customers = customers;
            ViewBag.Suppliers = suppliers;
            ViewBag.Products = products;
            ViewBag.Materials = materials;

            return View(dto);
        }

        try
        {
            var userId = GetCurrentUserId();
            var created = await _journalService.CreateManualJournalAsync(dto, userId);
            TempData["SuccessMessage"] = $"تم ترحيل القيد المحاسبي [{created.JournalNumber}] بنجاح.";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var accounts = await _accountService.GetActivePostableAccountsAsync();
            var customers = await _customerService.GetAllCustomersAsync();
            var suppliers = await _supplierService.GetAllSuppliersAsync();
            var products = await _productService.GetActiveProductsAsync();
            var materials = await _materialService.GetAllMaterialsAsync();

            ViewBag.Accounts = accounts;
            ViewBag.Customers = customers;
            ViewBag.Suppliers = suppliers;
            ViewBag.Products = products;
            ViewBag.Materials = materials;

            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reverse(ReverseJournalDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            TempData["ErrorMessage"] = "يجب تحديد سبب عكس القيد.";
            return RedirectToAction(nameof(Details), new { id = dto.JournalEntryId });
        }

        try
        {
            var userId = GetCurrentUserId();
            var reversal = await _journalService.ReverseJournalEntryAsync(dto, userId);
            TempData["SuccessMessage"] = $"تم إنشاء القيد العكسي [{reversal.JournalNumber}] بنجاح وعكس القيد الأصلي.";
            return RedirectToAction(nameof(Details), new { id = reversal.Id });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = dto.JournalEntryId });
        }
    }
}
