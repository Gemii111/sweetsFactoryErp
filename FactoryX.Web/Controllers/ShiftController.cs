using AutoMapper;
using FactoryX.Application.DTOs.Requests.ShiftRequests;
using FactoryX.Application.DTOs.Responses.ShiftResponses;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class ShiftController : Controller
{
	private readonly IServiceManager _serviceManager;
	private readonly IMapper _mapper;

	public ShiftController(IServiceManager serviceManager, IMapper mapper)
	{
		_serviceManager = serviceManager;
		_mapper = mapper;
	}

	public async Task<IActionResult> Index()
	{
		var shifts = await _serviceManager.ShiftService.GetAllAsync();
		return View(shifts);
	}

	public async Task<IActionResult> Details(int id)
	{
		var shift = await _serviceManager.ShiftService.GetByIdAsync(id);
		if (shift == null) return NotFound();
		return View(shift);
	}

	public IActionResult Create()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(InsertShiftRequest request)
	{
		if (!ModelState.IsValid) return View(request);
		await _serviceManager.ShiftService.CreateAsync(request);
		TempData["Success"] = "Vardiya başarıyla eklendi.";
		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Edit(int id)
	{
		var shift = await _serviceManager.ShiftService.GetByIdAsync(id);
		if (shift == null) return NotFound();
		return View(shift);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, GetShiftResponse request)
	{
		if (id != request.Id) return BadRequest();
		if (!ModelState.IsValid) return View(request);
		var updateShiftRequest = _mapper.Map<UpdateShiftRequest>(request);
		await _serviceManager.ShiftService.UpdateAsync(updateShiftRequest);
		TempData["Success"] = "Vardiya başarıyla güncellendi.";
		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Delete(int id)
	{
		var shift = await _serviceManager.ShiftService.GetByIdAsync(id);
		if (shift == null) return NotFound();
		return View(shift);
	}

	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(GetShiftResponse request)
	{
		var deleteShiftRequest = _mapper.Map<DeleteShiftRequest>(request);
		await _serviceManager.ShiftService.DeleteAsync(deleteShiftRequest);
		TempData["Success"] = "Vardiya başarıyla silindi.";
		return RedirectToAction(nameof(Index));
	}
}