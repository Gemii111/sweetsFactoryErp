using FactoryX.Application.DTOs;
using FactoryX.Application.DTOs.Requests.ProductionRecordRequests;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class ProductionRecordController : Controller
{
	private readonly IServiceManager _serviceManager;

	public ProductionRecordController(IServiceManager serviceManager)
	{
		_serviceManager = serviceManager;
	}

	public async Task<IActionResult> Index()
    {
        var records = await _serviceManager.ProductionRecordService.GetAllAsync();
        return View(records);
    }

    public async Task<IActionResult> Details(int id)
    {
        var record = await _serviceManager.ProductionRecordService.GetByIdAsync(id);
        if (record == null) return NotFound();
        return View(record);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDropdowns();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InsertProductionRecordRequest request)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(request);
        }
        await _serviceManager.ProductionRecordService.CreateAsync(request);
        TempData["Success"] = "Üretim kaydı başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var record = await _serviceManager.ProductionRecordService.GetByIdAsync(id);
        if (record == null) return NotFound();
        await PopulateDropdowns();
        return View(record);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateProductionRecordRequest request)
    {
        if (id != request.Id) return BadRequest();
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
            return View(request);
        }
        await _serviceManager.ProductionRecordService.UpdateAsync(request);
        TempData["Success"] = "Üretim kaydı başarıyla güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var record = await _serviceManager.ProductionRecordService.GetByIdAsync(id);
        if (record == null) return NotFound();
        return View(record);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(DeleteProductionRecordRequest request)
    {
        await _serviceManager.ProductionRecordService.DeleteAsync(request);
        TempData["Success"] = "Üretim kaydı başarıyla silindi.";
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDropdowns()
    {
        var workOrders = await _serviceManager.WorkOrderService.GetAllAsync();
        var operators = await _serviceManager.OperatorService.GetAllAsync();
        ViewBag.WorkOrders = new SelectList(workOrders, "Id", "Id");
        ViewBag.Operators = new SelectList(operators, "Id", "Name");
    }
}