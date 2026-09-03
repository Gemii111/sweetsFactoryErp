using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class CustomerLedgerController : Controller
{
    private readonly IGeneralLedgerService _glService;
    private readonly ICustomerService _customerService;

    public CustomerLedgerController(IGeneralLedgerService glService, ICustomerService customerService)
    {
        _glService = glService;
        _customerService = customerService;
    }

    public async Task<IActionResult> Index(int? customerId = null, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var customers = await _customerService.GetAllCustomersAsync();
        ViewBag.Customers = customers;
        ViewBag.SelectedCustomerId = customerId;
        ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
        ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

        if (!customerId.HasValue || customerId.Value <= 0)
        {
            var first = customers.FirstOrDefault();
            if (first != null) customerId = first.Id;
        }

        if (customerId.HasValue && customerId.Value > 0)
        {
            var ledger = await _glService.GetCustomerLedgerAsync(customerId.Value, fromDate, toDate);
            return View(ledger);
        }

        return View(null);
    }
}
