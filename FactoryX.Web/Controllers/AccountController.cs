using Microsoft.AspNetCore.Mvc;
using FactoryX.Application.DTOs;
using FactoryX.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Application.DTOs.Requests.UserManagementRequests;
using FactoryX.Application.DTOs.Requests.AuthenticationRequests;
using FactoryX.Application.DTOs.Responses.AuthenticationResponses;
using FactoryX.Application.DTOs.Responses.UserManagementResponses;
using AutoMapper;

namespace FactoryX.Web.Controllers;

public class AccountController : Controller
{
    private readonly IServiceManager _serviceManager;
    private readonly IMapper _mapper;
    public AccountController(IServiceManager serviceManager, IMapper mapper)
    {
        _serviceManager = serviceManager;
        _mapper = mapper;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
            return View(model);
        LoginResponse? user = await _serviceManager.UserService.AuthenticateAsync(new LoginRequest(Username: model.Username, Password: model.Password));
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return View(model);
        }
        // Set authentication cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username ?? ""),
            new Claim(ClaimTypes.Role, string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role)
        };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);
        return RedirectToLocal(returnUrl);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);
        try
        {
            RegisterRequest registerRequest = new RegisterRequest(
                
                Username: model.Username,
                Password: model.Password,
                ConfirmPassword: model.ConfirmPassword,
                Role: model.Role);

            var user = await _serviceManager.UserService.RegisterAsync(registerRequest);
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var profile = await _serviceManager.UserService.GetProfileAsync(userId);
        return View(profile);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(GetUserProfileResponse request)
    {
		if (!ModelState.IsValid) return View(request);
		var userProfileDto = _mapper.Map<UserProfileDto>(request);
        await _serviceManager.UserService.UpdateProfileAsync(userProfileDto);
        TempData["Success"] = "Profil başarıyla güncellendi.";
        return RedirectToAction("Profile");
    }

    [Authorize]
    [HttpGet]
    public IActionResult ChangePassword()
    {
        return View();
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
    {
        string? userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if(string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId)) {
            return Unauthorized();
        }
        if (!ModelState.IsValid) return View(request);

        var result = await _serviceManager.UserService.ChangePasswordAsync(userId: userId, request);
        if (!result)
        {
            ModelState.AddModelError(string.Empty, "Mevcut şifre yanlış veya işlem başarısız.");
            return View(request);
        }

        TempData["Success"] = "Şifre başarıyla değiştirildi.";
        return RedirectToAction("Profile");
    }

	private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}