using AutoMapper;
using FactoryX.Application.DTOs.Requests.MachineRequests;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class MachinesController : Controller
{
	private readonly IServiceManager _serviceManager;
	private readonly IMapper _mapper;
	public MachinesController(IServiceManager serviceManager, IMapper mapper)
	{
		_serviceManager = serviceManager;
		_mapper = mapper;
	}

	public async Task<IActionResult> Index()
	{
		var machines = await _serviceManager.MachineService.GetAllAsync();
		return View(machines);
	}

	public async Task<IActionResult> Details(int id)
	{
		var machine = await _serviceManager.MachineService.GetByIdAsync(id);
		if (machine == null) return NotFound();
		return View(machine);
	}

	public IActionResult Create()
	{
		return View();
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(InsertMachineRequest request)
	{
		if (!ModelState.IsValid) return View(request);
		await _serviceManager.MachineService.CreateAsync(request);
		TempData["Success"] = "Machine created successfully.";
		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Edit(int id)
	{
		var machine = await _serviceManager.MachineService.GetByIdAsync(id);
		if (machine == null) return NotFound();
		var updateRequest = _mapper.Map<UpdateMachineRequest>(machine);
		return View(updateRequest);
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Edit(int id, UpdateMachineRequest request)
	{
		if (id != request.Id) return BadRequest();
		if (!ModelState.IsValid) return View(request);
		await _serviceManager.MachineService.UpdateAsync(request);
		TempData["Success"] = "Machine updated successfully.";
		return RedirectToAction(nameof(Index));
	}

	public async Task<IActionResult> Delete(int id)
	{
		var machine = await _serviceManager.MachineService.GetByIdAsync(id);
		if (machine == null) return NotFound();
		return View(machine);
	}

	[HttpPost, ActionName("Delete")]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> DeleteConfirmed(int id)
	{
		DeleteMachineRequest request = new DeleteMachineRequest(id);
		await _serviceManager.MachineService.DeleteAsync(request);
		TempData["Success"] = "Machine deleted successfully.";
		return RedirectToAction(nameof(Index));
	}
}