using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class GeneralLedgerController : Controller
{
    private readonly IGeneralLedgerService _glService;
    private readonly IAccountService _accountService;

    public GeneralLedgerController(IGeneralLedgerService glService, IAccountService accountService)
    {
        _glService = glService;
        _accountService = accountService;
    }

    public async Task<IActionResult> Index(GeneralLedgerQueryDto query)
    {
        var accounts = await _accountService.GetAllAccountsAsync();
        var ledgerData = await _glService.GetGeneralLedgerAsync(query);

        ViewBag.Accounts = accounts;
        ViewBag.Query = query;

        return View(ledgerData);
    }
}
