using System.Security.Claims;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Extensions;

public static class HtmlPermissionExtensions
{
    public static async Task<bool> HasPermissionAsync(this IHtmlHelper html, string permissionCode)
    {
        var user = html.ViewContext.HttpContext.User;
        if (user == null || user.Identity == null || !user.Identity.IsAuthenticated)
            return false;

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            return false;

        var permissionService = html.ViewContext.HttpContext.RequestServices.GetService<IPermissionService>();
        if (permissionService == null) return false;

        return await permissionService.HasPermissionAsync(userId, permissionCode);
    }
}
