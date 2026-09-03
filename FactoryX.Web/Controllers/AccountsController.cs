using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class AccountsController : Controller
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    public async Task<IActionResult> Index()
    {
        var tree = await _accountService.GetAccountTreeAsync();
        var accounts = await _accountService.GetAllAccountsAsync();
        ViewBag.AllAccounts = accounts;
        return View(tree);
    }

    public async Task<IActionResult> List()
    {
        var accounts = await _accountService.GetAllAccountsAsync();
        return View(accounts);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var accounts = await _accountService.GetAllAccountsAsync();
        ViewBag.ParentAccounts = accounts.Where(a => a.IsControlAccount || a.ParentAccountId == null).ToList();
        return View(new AccountCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccountCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            ViewBag.ParentAccounts = accounts.Where(a => a.IsControlAccount || a.ParentAccountId == null).ToList();
            return View(dto);
        }

        try
        {
            await _accountService.CreateAccountAsync(dto);
            TempData["SuccessMessage"] = $"تم إنشاء الحساب '{dto.AccountCode} - {dto.AccountNameAr}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var accounts = await _accountService.GetAllAccountsAsync();
            ViewBag.ParentAccounts = accounts.Where(a => a.IsControlAccount || a.ParentAccountId == null).ToList();
            return View(dto);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var account = await _accountService.GetAccountByIdAsync(id);
        if (account == null)
        {
            TempData["ErrorMessage"] = "الحساب غير موجود.";
            return RedirectToAction(nameof(Index));
        }

        var dto = new AccountUpdateDto
        {
            Id = account.Id,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            AccountNameAr = account.AccountNameAr,
            AccountType = account.AccountType,
            ParentAccountId = account.ParentAccountId,
            IsActive = account.IsActive,
            IsControlAccount = account.IsControlAccount,
            AccountRole = account.AccountRole,
            Notes = account.Notes
        };

        var accounts = await _accountService.GetAllAccountsAsync();
        ViewBag.ParentAccounts = accounts.Where(a => a.Id != id && (a.IsControlAccount || a.ParentAccountId == null)).ToList();
        return View(dto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AccountUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            var accounts = await _accountService.GetAllAccountsAsync();
            ViewBag.ParentAccounts = accounts.Where(a => a.Id != dto.Id && (a.IsControlAccount || a.ParentAccountId == null)).ToList();
            return View(dto);
        }

        try
        {
            await _accountService.UpdateAccountAsync(dto);
            TempData["SuccessMessage"] = $"تم تحديث بيانات الحساب '{dto.AccountCode} - {dto.AccountNameAr}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            var accounts = await _accountService.GetAllAccountsAsync();
            ViewBag.ParentAccounts = accounts.Where(a => a.Id != dto.Id && (a.IsControlAccount || a.ParentAccountId == null)).ToList();
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        try
        {
            await _accountService.ToggleActiveAsync(id);
            TempData["SuccessMessage"] = "تم تعديل حالة تفعيل الحساب بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _accountService.DeleteAccountAsync(id);
            TempData["SuccessMessage"] = "تم حذف الحساب بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        var settings = await _accountService.GetAccountingSettingsAsync();
        var accounts = await _accountService.GetActivePostableAccountsAsync();
        ViewBag.Accounts = accounts;
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateSetting(AccountingSettingUpdateDto dto)
    {
        try
        {
            await _accountService.UpdateAccountingSettingAsync(dto);
            TempData["SuccessMessage"] = "تم تحديث ربط الحساب بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Settings));
    }
}
