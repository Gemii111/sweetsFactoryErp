using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FactoryX.Web.Models;
using Microsoft.AspNetCore.Authorization;
using FactoryX.Application.Services.Abstracts;

namespace FactoryX.Web.Controllers;

public class HomeController : Controller
{
    private readonly IServiceManager _serviceManager;

	public HomeController(IServiceManager serviceManager)
	{
		_serviceManager = serviceManager;
	}

	public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            MachineCount = (await _serviceManager.MachineService.GetAllAsync()).Count(),
            ProductCount = (await _serviceManager.ProductService.GetAllAsync()).Count(),
            OperatorCount = (await _serviceManager.OperatorService.GetAllAsync()).Count(),
            WorkOrderCount = (await _serviceManager.WorkOrderService.GetAllAsync()).Count(),
            ProductionRecordCount = (await _serviceManager.ProductionRecordService.GetAllAsync()).Count(),
            ShiftCount = (await _serviceManager.ShiftService.GetAllAsync()).Count(),
            RecentWorkOrders = (await _serviceManager.WorkOrderService.GetAllAsync()).OrderByDescending(x => x.StartDate).Take(5).ToList(),
            RecentProductionRecords = (await _serviceManager.ProductionRecordService.GetAllAsync()).OrderByDescending(x => x.Timestamp).Take(5).ToList()
        };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> Dashboard()
    {
        ViewBag.MachineCount = (await _serviceManager.MachineService.GetAllAsync()).Count();
        ViewBag.ProductCount = (await _serviceManager.ProductService.GetAllAsync()).Count();
        ViewBag.WorkOrderCount = (await _serviceManager.WorkOrderService.GetAllAsync()).Count();
        ViewBag.OperatorCount = (await _serviceManager.OperatorService.GetAllAsync()).Count();
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var correlationId = HttpContext.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? $"ERR-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

        return View(new ErrorViewModel
        {
            RequestId = requestId,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            Path = HttpContext.Request.Path
        });
    }
}

