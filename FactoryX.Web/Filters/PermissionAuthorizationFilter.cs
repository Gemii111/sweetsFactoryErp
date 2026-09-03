using System.Security.Claims;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FactoryX.Web.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class HasPermissionAttribute : TypeFilterAttribute
{
    public HasPermissionAttribute(string permissionCode) : base(typeof(PermissionAuthorizationFilter))
    {
        Arguments = new object[] { permissionCode };
    }
}

public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly string _permissionCode;
    private readonly IPermissionService _permissionService;

    public PermissionAuthorizationFilter(string permissionCode, IPermissionService permissionService)
    {
        _permissionCode = permissionCode;
        _permissionService = permissionService;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;
        if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new ChallengeResult();
            return;
        }

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            context.Result = new ForbidResult();
            return;
        }

        var hasPermission = await _permissionService.HasPermissionAsync(userId, _permissionCode);
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
        }
    }
}
