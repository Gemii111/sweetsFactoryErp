using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class CustomerStatementsController : Controller
{
    private readonly ICustomerStatementService _statementService;
    private readonly ICustomerService _customerService;

    public CustomerStatementsController(
        ICustomerStatementService statementService,
        ICustomerService customerService)
    {
        _statementService = statementService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(string? searchTerm)
    {
        var balances = await _statementService.GetAllCustomerBalancesAsync(searchTerm);
        ViewBag.SearchTerm = searchTerm;
        return View(balances);
    }

    public async Task<IActionResult> Details(int id, DateTime? fromDate, DateTime? toDate)
    {
        var statement = await _statementService.GetCustomerStatementAsync(id, fromDate, toDate);
        if (statement == null)
        {
            TempData["ErrorMessage"] = "العميل المطلوب غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        return View(statement);
    }

    public async Task<IActionResult> Print(int id, DateTime? fromDate, DateTime? toDate)
    {
        var statement = await _statementService.GetCustomerStatementAsync(id, fromDate, toDate);
        if (statement == null)
        {
            return NotFound();
        }

        return View(statement);
    }
}
