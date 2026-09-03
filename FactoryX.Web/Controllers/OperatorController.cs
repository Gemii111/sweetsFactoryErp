using AutoMapper;
using FactoryX.Application.DTOs.Requests.OperatorRequests;
using FactoryX.Application.DTOs.Responses.OperatorResponses;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class OperatorController : Controller
{
	private readonly IServiceManager _serviceManager;
	private readonly IMapper _mapper;

	public OperatorController(IServiceManager serviceManager, IMapper mapper)
	{
		_serviceManager = serviceManager;
		_mapper = mapper;
	}

	public async Task<IActionResult> Index()
	{
		var operators = await _serviceManager.OperatorService.GetAllAsync();
		return View(operators);
	}

	public async Task<IActionResult> Details(int id)
	{
		var op = await _serviceManager.OperatorService.GetByIdAsync(id);
		if (op == null) return NotFound();
		return View(op);
	}

	public IActionResult Create()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(InsertOperatorRequest request)
	{
		if (!ModelState.IsValid) return View(request);
		await _serviceManager.OperatorService.CreateAsync(request);
		TempData["Success"] = "Operatör başarıyla eklendi.";
		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Edit(int id)
	{
		var op = await _serviceManager.OperatorService.GetByIdAsync(id);
		if (op == null) return NotFound();
		return View(op);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, GetOperatorResponse request)
	{
		if (id != request.Id) return BadRequest();
		if (!ModelState.IsValid) return View(request);
		var updateOperatorRequest = _mapper.Map<UpdateOperatorRequest>(request);
		await _serviceManager.OperatorService.UpdateAsync(updateOperatorRequest);
		TempData["Success"] = "Operatör başarıyla güncellendi.";
		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Delete(int id)
	{
		var op = await _serviceManager.OperatorService.GetByIdAsync(id);
		if (op == null) return NotFound();
		return View(op);
	}

	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(GetOperatorResponse request)
	{
		var deleteOperatorRequest = _mapper.Map<DeleteOperatorRequest>(request);
		await _serviceManager.OperatorService.DeleteAsync(deleteOperatorRequest);
		TempData["Success"] = "Operatör başarıyla silindi.";
		return RedirectToAction(nameof(Index));
	}
}