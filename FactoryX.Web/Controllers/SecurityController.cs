using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FactoryX.Web.Controllers;

[Authorize]
public class SecurityController : Controller
{
    private readonly IAuditService _auditService;

    public SecurityController(IAuditService auditService)
    {
        _auditService = auditService;
    }

    [HttpGet]
    [HasPermission("Security.Dashboard.View")]
    public async Task<IActionResult> Dashboard()
    {
        var dashboard = await _auditService.GetSecurityDashboardAsync();
        return View(dashboard);
    }

    [HttpGet]
    [HasPermission("Audit.View")]
    public async Task<IActionResult> Events([FromQuery] SecurityEventFilterDto filter)
    {
        filter.Page = filter.Page <= 0 ? 1 : filter.Page;
        filter.PageSize = filter.PageSize <= 0 ? 50 : filter.PageSize;

        ViewBag.Filter = filter;
        var events = await _auditService.GetSecurityEventsAsync(filter);
        return View(events);
    }
}
