using System;
using System.Linq;
using System.Threading.Tasks;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using FactoryX.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Web.Controllers;

[Authorize]
public class SettingsController : Controller
{
    private readonly ISettingsService _settingsService;
    private readonly AppDbContext _context;

    public SettingsController(ISettingsService settingsService, AppDbContext context)
    {
        _settingsService = settingsService;
        _context = context;
    }

    #region Hub / Dashboard
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> Index()
    {
        var company = await _settingsService.GetCompanyProfileAsync();
        var general = await _settingsService.GetGeneralSettingsAsync();
        var tax = await _settingsService.GetCurrentTaxSettingAsync();
        var docSettings = await _settingsService.GetDocumentNumberSettingsAsync();
        var opDefaults = await _settingsService.GetOperationalDefaultsAsync();

        ViewBag.Company = company;
        ViewBag.General = general;
        ViewBag.Tax = tax;
        ViewBag.DocSettingsCount = docSettings.Count();
        ViewBag.Operational = opDefaults;

        return View();
    }
    #endregion

    #region Company Profile
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> Company()
    {
        var profile = await _settingsService.GetCompanyProfileAsync();
        return View(profile);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Settings.Company.Manage")]
    public async Task<IActionResult> Company(CompanyProfileDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _settingsService.UpdateCompanyProfileAsync(model, username);
            TempData["SuccessMessage"] = "تم تحديث الملف التعريفي وهوية الشركة بنجاح.";
            return RedirectToAction(nameof(Company));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
    #endregion

    #region General & Regional Settings
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> General()
    {
        var general = await _settingsService.GetGeneralSettingsAsync();
        return View(general);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Settings.Edit")]
    public async Task<IActionResult> General(GeneralSettingsDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _settingsService.UpdateGeneralSettingsAsync(model, username);
            TempData["SuccessMessage"] = "تم حفظ الإعدادات العامة والإقليمية بنجاح.";
            return RedirectToAction(nameof(General));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }
    #endregion

    #region Tax Settings
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> Tax()
    {
        var taxes = await _settingsService.GetAllTaxSettingsAsync();
        return View(taxes);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Settings.Tax.Manage")]
    public async Task<IActionResult> SaveTax(TaxSettingDto model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "بيانات الضريبة غير مكتملة أو غير صحيحة.";
            return RedirectToAction(nameof(Tax));
        }

        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _settingsService.SaveTaxSettingAsync(model, username);
            TempData["SuccessMessage"] = $"تم حفظ إعداد الضريبة [{model.Name}] بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Tax));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Settings.Tax.Manage")]
    public async Task<IActionResult> ToggleTaxStatus(int id, bool isActive)
    {
        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _settingsService.ToggleTaxSettingStatusAsync(id, isActive, username);
            TempData["SuccessMessage"] = $"تم {(isActive ? "تفعيل" : "تعطيل")} إعداد الضريبة بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Tax));
    }
    #endregion

    #region Document Numbering
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> Numbering()
    {
        var settings = await _settingsService.GetDocumentNumberSettingsAsync();
        return View(settings);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Settings.DocumentNumbering.Manage")]
    public async Task<IActionResult> SaveNumbering(DocumentNumberSettingDto model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "بيانات ترقيم المستند غير صحيحة.";
            return RedirectToAction(nameof(Numbering));
        }

        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _settingsService.SaveDocumentNumberSettingAsync(model, username);
            TempData["SuccessMessage"] = $"تم تحديث صيغة ترقيم مستند [{model.DocumentTypeNameArabic}] بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Numbering));
    }
    #endregion

    #region Inventory Defaults
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> Inventory()
    {
        var defaults = await _settingsService.GetOperationalDefaultsAsync();
        ViewBag.Warehouses = await _context.Warehouses.AsNoTracking().Where(w => w.IsActive).ToListAsync();
        return View(defaults);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Settings.Inventory.Manage")]
    public async Task<IActionResult> Inventory(OperationalDefaultsDto model)
    {
        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _settingsService.SaveOperationalDefaultsAsync(model, username);
            TempData["SuccessMessage"] = "تم تحديث محددات وضوابط المخزون والمستودعات الافتراضية بنجاح.";
            return RedirectToAction(nameof(Inventory));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Warehouses = await _context.Warehouses.AsNoTracking().Where(w => w.IsActive).ToListAsync();
            return View(model);
        }
    }
    #endregion

    #region Production Defaults
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> Production()
    {
        var defaults = await _settingsService.GetOperationalDefaultsAsync();
        ViewBag.Warehouses = await _context.Warehouses.AsNoTracking().Where(w => w.IsActive).ToListAsync();
        return View(defaults);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Settings.Production.Manage")]
    public async Task<IActionResult> Production(OperationalDefaultsDto model)
    {
        try
        {
            var username = User.Identity?.Name ?? "Admin";
            var current = await _settingsService.GetOperationalDefaultsAsync();
            current.DefaultProductionWarehouseId = model.DefaultProductionWarehouseId;
            current.DefaultQuarantineWarehouseId = model.DefaultQuarantineWarehouseId;
            current.MaxWasteTolerancePercent = model.MaxWasteTolerancePercent;
            current.RequireWasteApproval = model.RequireWasteApproval;

            await _settingsService.SaveOperationalDefaultsAsync(current, username);
            TempData["SuccessMessage"] = "تم تحديث محددات وضوابط الإنتاج والهوالك بنجاح.";
            return RedirectToAction(nameof(Production));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Warehouses = await _context.Warehouses.AsNoTracking().Where(w => w.IsActive).ToListAsync();
            return View(model);
        }
    }
    #endregion

    #region Purchasing Defaults
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> Purchasing()
    {
        var defaults = await _settingsService.GetOperationalDefaultsAsync();
        ViewBag.RequirePOApproval = await _settingsService.GetSettingValueAsync<bool>("Purchasing.RequirePOApproval", true);
        ViewBag.Warehouses = await _context.Warehouses.AsNoTracking().Where(w => w.IsActive).ToListAsync();
        return View(defaults);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Settings.Purchasing.Manage")]
    public async Task<IActionResult> Purchasing(int? defaultRawWarehouseId, int? defaultPackagingWarehouseId, bool requirePOApproval)
    {
        try
        {
            var username = User.Identity?.Name ?? "Admin";
            var current = await _settingsService.GetOperationalDefaultsAsync();
            current.DefaultRawMaterialWarehouseId = defaultRawWarehouseId;
            current.DefaultPackagingWarehouseId = defaultPackagingWarehouseId;

            await _settingsService.SaveOperationalDefaultsAsync(current, username);
            await _settingsService.SetSettingValueAsync("Purchasing.RequirePOApproval", requirePOApproval.ToString(), username, SettingDataType.Boolean, SettingCategory.Purchasing, "إلزامية اعتماد أوامر الشراء");

            TempData["SuccessMessage"] = "تم تحديث محددات وضوابط المشتريات والاستلام بنجاح.";
            return RedirectToAction(nameof(Purchasing));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Purchasing));
        }
    }
    #endregion

    #region Sales Defaults
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> Sales()
    {
        var defaults = await _settingsService.GetOperationalDefaultsAsync();
        ViewBag.RequireCreditCheck = await _settingsService.GetSettingValueAsync<bool>("Sales.RequireCreditCheck", true);
        ViewBag.Warehouses = await _context.Warehouses.AsNoTracking().Where(w => w.IsActive).ToListAsync();
        return View(defaults);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Settings.Sales.Manage")]
    public async Task<IActionResult> Sales(int? defaultFinishedGoodsWarehouseId, bool requireCreditCheck)
    {
        try
        {
            var username = User.Identity?.Name ?? "Admin";
            var current = await _settingsService.GetOperationalDefaultsAsync();
            current.DefaultFinishedGoodsWarehouseId = defaultFinishedGoodsWarehouseId;

            await _settingsService.SaveOperationalDefaultsAsync(current, username);
            await _settingsService.SetSettingValueAsync("Sales.RequireCreditCheck", requireCreditCheck.ToString(), username, SettingDataType.Boolean, SettingCategory.Sales, "فحص الحد الائتماني للعميل قبل تأكيد البيع");

            TempData["SuccessMessage"] = "تم تحديث محددات وضوابط المبيعات بنجاح.";
            return RedirectToAction(nameof(Sales));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Sales));
        }
    }
    #endregion

    #region Accounting Mappings
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> Accounting()
    {
        var mappings = await _settingsService.GetAccountMappingsAsync();
        ViewBag.Accounts = await _context.Accounts.AsNoTracking()
            .Where(a => a.IsActive && !a.IsControlAccount)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();

        return View(mappings);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Settings.Accounting.Manage")]
    public async Task<IActionResult> SaveAccountingMapping(AccountingSettingUpdateDto model)
    {
        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _settingsService.SaveAccountMappingAsync(model, username);
            TempData["SuccessMessage"] = $"تم تحديث ربط الحساب المحاسبي لدور [{model.Role}] بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Accounting));
    }
    #endregion

    #region History & Audit Trail
    [HttpGet]
    [HasPermission("Settings.View")]
    public async Task<IActionResult> History()
    {
        var history = await _settingsService.GetConfigurationHistoryAsync(200);
        return View(history);
    }
    #endregion
}
