using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FactoryX.Web.Controllers;

[Authorize]
public class AccountingPeriodsController : Controller
{
    private readonly IAccountingPeriodService _periodService;

    public AccountingPeriodsController(IAccountingPeriodService periodService)
    {
        _periodService = periodService;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 1;
    }

    public async Task<IActionResult> Index()
    {
        var periods = await _periodService.GetAllPeriodsAsync();
        return View(periods);
    }

    [HttpGet]
    public IActionResult Create()
    {
        var currentYear = DateTime.UtcNow.Year;
        var currentMonth = DateTime.UtcNow.Month;
        var start = new DateTime(currentYear, currentMonth, 1);
        var end = start.AddMonths(1).AddDays(-1);

        var dto = new AccountingPeriodCreateDto
        {
            PeriodName = $"FY{currentYear}-M{currentMonth:D2}",
            StartDate = start,
            EndDate = end,
            Notes = $"الفترة المالية لشهر {currentMonth} عام {currentYear}"
        };

        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccountingPeriodCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return View(dto);
        }

        try
        {
            await _periodService.CreatePeriodAsync(dto);
            TempData["SuccessMessage"] = $"تم إنشاء الفترة المالية '{dto.PeriodName}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(ClosePeriodDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _periodService.ClosePeriodAsync(dto, userId);
            TempData["SuccessMessage"] = "تم إغلاق الفترة المالية وتجميد قيودها بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
