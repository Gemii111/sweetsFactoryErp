using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class TrialBalanceController : Controller
{
    private readonly IGeneralLedgerService _glService;

    public TrialBalanceController(IGeneralLedgerService glService)
    {
        _glService = glService;
    }

    public async Task<IActionResult> Index(TrialBalanceQueryDto query)
    {
        var trialBalance = await _glService.GetTrialBalanceAsync(query);
        ViewBag.Query = query;
        return View(trialBalance);
    }
}
