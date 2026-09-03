using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly IUserAdminService _userAdminService;
    private readonly IRoleService _roleService;
    private readonly IWarehouseAccessService _warehouseAccessService;

    public UsersController(
        IUserAdminService userAdminService,
        IRoleService roleService,
        IWarehouseAccessService warehouseAccessService)
    {
        _userAdminService = userAdminService;
        _roleService = roleService;
        _warehouseAccessService = warehouseAccessService;
    }

    [HttpGet]
    [HasPermission("Users.View")]
    public async Task<IActionResult> Index()
    {
        var users = await _userAdminService.GetAllUsersAsync();
        return View(users);
    }

    [HttpGet]
    [HasPermission("Users.Create")]
    public async Task<IActionResult> Create()
    {
        ViewBag.Roles = await _roleService.GetAllRolesAsync();
        return View(new CreateUserRequestDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Users.Create")]
    public async Task<IActionResult> Create(CreateUserRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await _roleService.GetAllRolesAsync();
            return View(model);
        }

        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _userAdminService.CreateUserAsync(model, username);
            TempData["SuccessMessage"] = $"تم إنشاء حساب المستخدم '{model.Username}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Roles = await _roleService.GetAllRolesAsync();
            return View(model);
        }
    }

    [HttpGet]
    [HasPermission("Users.Edit")]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userAdminService.GetUserByIdAsync(id);
        if (user == null) return NotFound();

        ViewBag.Roles = await _roleService.GetAllRolesAsync();
        var editDto = new EditUserRequestDto
        {
            Id = user.Id,
            Username = user.Username,
            FullName = user.FullName,
            Email = user.Email,
            PrimaryRole = user.Role,
            IsActive = user.IsActive,
            IsAllWarehousesAllowed = user.IsAllWarehousesAllowed
        };

        return View(editDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Users.Edit")]
    public async Task<IActionResult> Edit(EditUserRequestDto model)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Roles = await _roleService.GetAllRolesAsync();
            return View(model);
        }

        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _userAdminService.UpdateUserAsync(model, username);
            TempData["SuccessMessage"] = $"تم تحديث بيانات المستخدم '{model.Username}' بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            ViewBag.Roles = await _roleService.GetAllRolesAsync();
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Users.Edit")]
    public async Task<IActionResult> ToggleStatus(int id, bool isActive)
    {
        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _userAdminService.ToggleUserStatusAsync(id, isActive, username);
            TempData["SuccessMessage"] = $"تم {(isActive ? "تفعيل" : "تعطيل")} حساب المستخدم بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Users.Edit")]
    public async Task<IActionResult> Unlock(int id)
    {
        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _userAdminService.UnlockUserAsync(id, username);
            TempData["SuccessMessage"] = "تم إلغاء قفل الحساب بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    [HasPermission("Users.Edit")]
    public async Task<IActionResult> Warehouses(int id)
    {
        try
        {
            var model = await _warehouseAccessService.GetUserWarehouseAssignmentAsync(id);
            return View(model);
        }
        catch
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Users.Edit")]
    public async Task<IActionResult> Warehouses(int userId, bool isAllWarehousesAllowed, List<int> selectedWarehouseIds)
    {
        try
        {
            await _warehouseAccessService.SaveUserWarehouseAssignmentAsync(userId, isAllWarehousesAllowed, selectedWarehouseIds ?? new());
            TempData["SuccessMessage"] = "تم تحديث صلاحيات المستودعات للمستخدم بنجاح.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Warehouses), new { id = userId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [HasPermission("Users.Edit")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var username = User.Identity?.Name ?? "Admin";
            await _userAdminService.DeleteUserSafeAsync(id, username);
            TempData["SuccessMessage"] = "تم تعطيل/حذف حساب المستخدم بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
