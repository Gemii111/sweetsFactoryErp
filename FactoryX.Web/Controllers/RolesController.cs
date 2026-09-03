using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class RolesController : Controller
{
    private readonly IRoleService _roleService;
    private readonly IPermissionService _permissionService;

    public RolesController(IRoleService roleService, IPermissionService permissionService)
    {
        _roleService = roleService;
        _permissionService = permissionService;
    }

    [HttpGet]
    [HasPermission("Roles.Manage")]
    public async Task<IActionResult> Index()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return View(roles);
    }

    [HttpGet]
    [HasPermission("Roles.Manage")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Permissions = await _permissionService.GetAllPermissionsAsync();
        return View(new CreateRoleRequestDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Roles.Manage")]
    public async Task<IActionResult> Create(CreateRoleRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Permissions = await _permissionService.GetAllPermissionsAsync();
            return View(model);
        }

        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _roleService.CreateRoleAsync(model, username);
            TempData["SuccessMessage"] = $"تم إنشاء الدور '{model.DisplayName}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Permissions = await _permissionService.GetAllPermissionsAsync();
            return View(model);
        }
    }

    [HttpGet]
    [HasPermission("Roles.Manage")]
    public async Task<IActionResult> Edit(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null) return NotFound();

        var editDto = new EditRoleRequestDto
        {
            Id = role.Id,
            Name = role.Name,
            Code = role.Code,
            DisplayName = role.DisplayName,
            Description = role.Description,
            IsActive = role.IsActive
        };

        return View(editDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Roles.Manage")]
    public async Task<IActionResult> Edit(EditRoleRequestDto model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _roleService.UpdateRoleAsync(model, username);
            TempData["SuccessMessage"] = $"تم تعديل الدور '{model.DisplayName}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    [HasPermission("Roles.Manage")]
    public async Task<IActionResult> Permissions(int id)
    {
        try
        {
            var matrix = await _permissionService.GetRolePermissionMatrixAsync(id);
            return View(matrix);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Roles.Manage")]
    public async Task<IActionResult> Permissions(int roleId, List<int> selectedPermissionIds)
    {
        try
        {
            await _permissionService.UpdateRolePermissionsAsync(roleId, selectedPermissionIds ?? new());
            TempData["SuccessMessage"] = "تم حفظ وتحديث مصفوفة الصلاحيات للدور بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Permissions), new { id = roleId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Roles.Manage")]
    public async Task<IActionResult> ToggleStatus(int id, bool isActive)
    {
        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _roleService.ToggleRoleStatusAsync(id, isActive, username);
            TempData["SuccessMessage"] = $"تم {(isActive ? "تفعيل" : "تعطيل")} الدور بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
