using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class AuditController : Controller
{
    private readonly IAuditService _auditService;
    private readonly IUserAdminService _userAdminService;

    public AuditController(IAuditService auditService, IUserAdminService userAdminService)
    {
        _auditService = auditService;
        _userAdminService = userAdminService;
    }

    [HttpGet]
    [HasPermission("Audit.View")]
    public async Task<IActionResult> Index([FromQuery] AuditLogFilterDto filter)
    {
        filter.Page = filter.Page <= 0 ? 1 : filter.Page;
        filter.PageSize = filter.PageSize <= 0 ? 50 : filter.PageSize;

        ViewBag.Users = await _userAdminService.GetAllUsersAsync();
        ViewBag.Filter = filter;

        var logs = await _auditService.GetAuditLogsAsync(filter);
        return View(logs);
    }

    [HttpGet]
    [HasPermission("Audit.View")]
    public async Task<IActionResult> Details(int id)
    {
        var log = await _auditService.GetAuditLogDetailsAsync(id);
        if (log == null) return NotFound();

        return View(log);
    }
}
