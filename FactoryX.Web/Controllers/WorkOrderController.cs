using FactoryX.Application.DTOs.Requests.WorkOrderRequests;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

[Authorize]
public class WorkOrderController : Controller
{
	private readonly IServiceManager _serviceManager;

	public WorkOrderController(IServiceManager serviceManager)
	{
		_serviceManager = serviceManager;
	}

	public async Task<IActionResult> Index()
	{
		var workOrders = await _serviceManager.WorkOrderService.GetAllAsync();
		return View(workOrders);
	}

	public async Task<IActionResult> Details(int id)
	{
		var wo = await _serviceManager.WorkOrderService.GetByIdAsync(id);
		if (wo == null) return NotFound();
		return View(wo);
	}

	public async Task<IActionResult> Create()
	{
		await PopulateDropdowns();
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(InsertWorkOrderRequest request)
	{
		if (!ModelState.IsValid)
		{
			await PopulateDropdowns();
			return View(request);
		}
		await _serviceManager.WorkOrderService.CreateAsync(request);
		TempData["Success"] = "İş emri başarıyla eklendi.";
		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Edit(int id)
	{
		var wo = await _serviceManager.WorkOrderService.GetByIdAsync(id);
		if (wo == null) return NotFound();
		await PopulateDropdowns();
		return View(wo);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, UpdateWorkOrderRequest request)
	{
		if (id != request.Id) return BadRequest();
		if (!ModelState.IsValid)
		{
			await PopulateDropdowns();
			return View(request);
		}
		await _serviceManager.WorkOrderService.UpdateAsync(request);
		TempData["Success"] = "İş emri başarıyla güncellendi.";
		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Delete(int id)
	{
		var wo = await _serviceManager.WorkOrderService.GetByIdAsync(id);
		if (wo == null) return NotFound();
		return View(wo);
	}

	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(DeleteWorkOrderRequest request)
	{
		await _serviceManager.WorkOrderService.DeleteAsync(request);
		TempData["Success"] = "İş emri başarıyla silindi.";
		return RedirectToAction(nameof(Index));
	}

	private async Task PopulateDropdowns()
	{
		var products = await _serviceManager.ProductService.GetActiveProductsAsync();
		var machines = await _serviceManager.MachineService.GetAllAsync();
		ViewBag.Products = new SelectList(products, "Id", "Name");
		ViewBag.Machines = new SelectList(machines, "Id", "Name");
	}
}