using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class AccountingDashboardController : Controller
{
    private readonly IAccountingDashboardService _dashboardService;
    private readonly IAccountingPeriodService _periodService;

    public AccountingDashboardController(
        IAccountingDashboardService dashboardService,
        IAccountingPeriodService periodService)
    {
        _dashboardService = dashboardService;
        _periodService = periodService;
    }

    public async Task<IActionResult> Index(int? periodId = null)
    {
        var periods = await _periodService.GetAllPeriodsAsync();
        var dashboardData = await _dashboardService.GetDashboardDataAsync(periodId);

        ViewBag.Periods = periods;
        ViewBag.SelectedPeriodId = periodId;

        return View(dashboardData);
    }
}
