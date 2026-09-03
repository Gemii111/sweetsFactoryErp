using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class SupplierLedgerController : Controller
{
    private readonly IGeneralLedgerService _glService;
    private readonly ISupplierService _supplierService;

    public SupplierLedgerController(IGeneralLedgerService glService, ISupplierService supplierService)
    {
        _glService = glService;
        _supplierService = supplierService;
    }

    public async Task<IActionResult> Index(int? supplierId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var suppliers = await _supplierService.GetAllSuppliersAsync();
        ViewBag.Suppliers = suppliers;
        ViewBag.SelectedSupplierId = supplierId;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        if (!supplierId.HasValue || supplierId.Value <= 0)
        {
            var first = suppliers.FirstOrDefault();
            if (first != null) supplierId = first.Id;
        }

        if (supplierId.HasValue && supplierId.Value > 0)
        {
            var ledger = await _glService.GetSupplierLedgerAsync(supplierId.Value, fromDate, toDate);
            return View(ledger);
        }

        return View(null);
    }
}
