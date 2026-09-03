using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FactoryX.Web.Controllers;

public class PackagingBOMsController : Controller
{
    private readonly IServiceManager _serviceManager;

    public PackagingBOMsController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    public async Task<IActionResult> Index(int? productId, bool? onlyActive)
    {
        var boms = await _serviceManager.PackagingBOMService.GetAllBOMsAsync(onlyActive ?? false, productId);
        var products = await _serviceManager.ProductService.GetActiveProductsAsync();

        ViewBag.Products = new SelectList(products, "Id", "Name", productId);
        ViewBag.SelectedProductId = productId;
        ViewBag.OnlyActive = onlyActive ?? false;

        return View(boms);
    }

    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var bom = await _serviceManager.PackagingBOMService.GetBOMByIdAsync(id);
            var costSummary = await _serviceManager.PackagingCostService.CalculatePackagingCostAsync(id);
            var packagingMaterials = await _serviceManager.PackagingBOMService.GetAvailablePackagingMaterialsAsync();

            ViewBag.CostSummary = costSummary;
            ViewBag.PackagingMaterials = packagingMaterials;

            return View(bom);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "مواصفة التعبئة والتغليف المطلوبة غير موجودة.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var products = await _serviceManager.ProductService.GetActiveProductsAsync();
        var materials = await _serviceManager.PackagingBOMService.GetAvailablePackagingMaterialsAsync();

        ViewBag.Products = new SelectList(products, "Id", "Name");
        ViewBag.PackagingMaterials = materials;

        var model = new CreatePackagingBOMRequest
        {
            PackSize = 0.50m,
            PackSizeKg = 0.50m,
            PackUnit = "Box",
            IsActive = true,
            Items = new List<PackagingItemRequest>()
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePackagingBOMRequest request)
    {
        if (!ModelState.IsValid)
        {
            var products = await _serviceManager.ProductService.GetActiveProductsAsync();
            var materials = await _serviceManager.PackagingBOMService.GetAvailablePackagingMaterialsAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name", request.ProductId);
            ViewBag.PackagingMaterials = materials;
            return View(request);
        }

        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingBOMService.CreateBOMAsync(request, userId);
            TempData["SuccessMessage"] = $"تم إنشاء مواصفة التعبئة والتغليف [{result.Code}] بنجاح!";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var products = await _serviceManager.ProductService.GetActiveProductsAsync();
            var materials = await _serviceManager.PackagingBOMService.GetAvailablePackagingMaterialsAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name", request.ProductId);
            ViewBag.PackagingMaterials = materials;
            return View(request);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var bom = await _serviceManager.PackagingBOMService.GetBOMByIdAsync(id);
            var products = await _serviceManager.ProductService.GetActiveProductsAsync();

            ViewBag.Products = new SelectList(products, "Id", "Name", bom.ProductId);

            var model = new UpdatePackagingBOMRequest
            {
                Id = bom.Id,
                Code = bom.Code,
                Name = bom.Name,
                ProductId = bom.ProductId,
                PackSize = bom.PackSize,
                PackSizeKg = bom.PackSizeKg,
                PackUnit = bom.PackUnit,
                Description = bom.Description,
                IsActive = bom.IsActive
            };

            return View(model);
        }
        catch (KeyNotFoundException)
        {
            TempData["ErrorMessage"] = "مواصفة التعبئة والتغليف المطلوبة غير موجودة.";
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdatePackagingBOMRequest request)
    {
        if (!ModelState.IsValid)
        {
            var products = await _serviceManager.ProductService.GetActiveProductsAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name", request.ProductId);
            return View(request);
        }

        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingBOMService.UpdateBOMAsync(request, userId);
            TempData["SuccessMessage"] = $"تم تحديث مواصفة التعبئة [{result.Code}] بنجاح!";
            return RedirectToAction(nameof(Details), new { id = result.Id });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            var products = await _serviceManager.ProductService.GetActiveProductsAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name", request.ProductId);
            return View(request);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVersion(CreatePackagingBOMVersionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingBOMService.CreateVersionAsync(request, userId);
            TempData["SuccessMessage"] = $"تم إنشاء الإصدار الجديد [{result.VersionName} (v{result.VersionNumber})] بنجاح!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل إنشاء الإصدار الجديد: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = request.PackagingBOMId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ActivateVersion(int versionId, int bomId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingBOMService.ActivateVersionAsync(versionId, userId);
            TempData["SuccessMessage"] = $"تم تفعيل الإصدار [{result.VersionName} (v{result.VersionNumber})] بنجاح!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل تفعيل الإصدار: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = bomId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeactivateVersion(int versionId, int bomId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var result = await _serviceManager.PackagingBOMService.DeactivateVersionAsync(versionId, userId);
            TempData["SuccessMessage"] = $"تم إلغاء تفعيل الإصدار [{result.VersionName} (v{result.VersionNumber})] بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"فشل إلغاء تفعيل الإصدار: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = bomId });
    }

    [HttpGet]
    public async Task<IActionResult> GetCostSummary(int bomId, int? versionId)
    {
        try
        {
            var cost = await _serviceManager.PackagingCostService.CalculatePackagingCostAsync(bomId, versionId);
            return Json(new { success = true, data = cost });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    private int GetCurrentUserId()
    {
        var claim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier);
        if (claim != null && int.TryParse(claim.Value, out int id))
        {
            return id;
        }
        return 1; // Default admin user ID
    }
}
