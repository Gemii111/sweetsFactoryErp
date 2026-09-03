using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FluentValidation;

namespace FactoryX.Web.Controllers;

[Authorize]
public class RecipesController : Controller
{
    private readonly IServiceManager _serviceManager;

    public RecipesController(IServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }

    #region Recipe Master Actions

    public async Task<IActionResult> Index(string? search, int? productId, bool? isActive)
    {
        var filter = new RecipeFilterRequest(search, productId, isActive);
        var recipes = await _serviceManager.RecipeService.GetAllRecipesAsync(filter);
        var summary = await _serviceManager.RecipeService.GetRecipeSummaryAsync();
        var products = await _serviceManager.ProductService.GetActiveProductsAsync();

        ViewBag.Search = search;
        ViewBag.ProductId = productId;
        ViewBag.IsActive = isActive;
        ViewBag.Summary = summary;
        ViewBag.Products = new SelectList(products, "Id", "Name", productId);

        return View(recipes);
    }

    public async Task<IActionResult> Details(int id)
    {
        var recipe = await _serviceManager.RecipeService.GetRecipeDetailsAsync(id);
        if (recipe == null) return NotFound();
        return View(recipe);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateProductsDropdown();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateRecipeRequest request)
    {
        try
        {
            var created = await _serviceManager.RecipeService.CreateRecipeAsync(request);
            TempData["Success"] = $"تم إنشاء الوصفة المعيارية '{created.Name}' بنجاح. يمكنك الآن إضافة أول إصدار للوصفة وقائمة المواد (BOM).";
            return RedirectToAction(nameof(Details), new { id = created.Id });
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await PopulateProductsDropdown(request.ProductId);
        return View(request);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var recipe = await _serviceManager.RecipeService.GetRecipeByIdAsync(id);
        if (recipe == null) return NotFound();

        var request = new UpdateRecipeRequest(
            recipe.Id,
            recipe.ProductId,
            recipe.Code,
            recipe.Name,
            recipe.ArabicName,
            recipe.Description,
            recipe.BaseOutputQuantity,
            recipe.Unit,
            recipe.IsActive);

        await PopulateProductsDropdown(recipe.ProductId);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UpdateRecipeRequest request)
    {
        if (id != request.Id) return BadRequest();

        try
        {
            var updated = await _serviceManager.RecipeService.UpdateRecipeAsync(request);
            TempData["Success"] = $"تم تحديث بيانات الوصفة '{updated.Name}' بنجاح.";
            return RedirectToAction(nameof(Details), new { id = updated.Id });
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        await PopulateProductsDropdown(request.ProductId);
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _serviceManager.RecipeService.ToggleActiveAsync(id);
        if (!result) return NotFound();

        TempData["Success"] = "تم تغيير حالة تنشيط الوصفة بنجاح.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _serviceManager.RecipeService.DeleteRecipeAsync(id);
            TempData["Success"] = "تم حذف الوصفة بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    #endregion

    #region Recipe Version & BOM Actions

    public async Task<IActionResult> CreateVersion(int recipeId)
    {
        var recipe = await _serviceManager.RecipeService.GetRecipeByIdAsync(recipeId);
        if (recipe == null) return NotFound();

        ViewBag.Recipe = recipe;
        await PopulateMaterialsViewBag();

        var model = new CreateRecipeVersionRequest(
            recipeId,
            $"V{recipe.VersionCount + 1}.0",
            $"إصدار قياسي {recipe.Name}",
            DateTime.UtcNow.Date,
            null,
            recipe.BaseOutputQuantity > 0 ? recipe.BaseOutputQuantity : 100m,
            recipe.Unit ?? "KG",
            0.0m,
            0.0m,
            0.0m,
            0.0m,
            null,
            new List<RecipeItemRequest>());

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVersion(int recipeId, CreateRecipeVersionRequest request)
    {
        if (request.RecipeId == 0 && recipeId > 0)
        {
            request.RecipeId = recipeId;
        }

        try
        {
            var created = await _serviceManager.RecipeService.CreateVersionAsync(request);
            TempData["Success"] = $"تم حفظ إصدار الوصفة '{created.VersionNumber}' وقائمة المواد بنجاح كمسودة (Draft).";
            return RedirectToAction(nameof(DetailsVersion), new { versionId = created.Id });
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        var recipe = await _serviceManager.RecipeService.GetRecipeByIdAsync(request.RecipeId > 0 ? request.RecipeId : recipeId);
        ViewBag.Recipe = recipe;
        await PopulateMaterialsViewBag();
        return View(request);
    }

    [HttpGet]
    [Route("Recipes/EditVersion/{versionId?}")]
    public async Task<IActionResult> EditVersion(int? versionId, [FromQuery] int? id)
    {
        var targetId = versionId ?? id ?? 0;
        var version = await _serviceManager.RecipeService.GetVersionByIdAsync(targetId);
        if (version == null) return NotFound();

        var recipe = await _serviceManager.RecipeService.GetRecipeByIdAsync(version.RecipeId);
        ViewBag.Recipe = recipe;
        await PopulateMaterialsViewBag();

        var itemsList = version.Items?.Select(i => new RecipeItemRequest(
            i.MaterialId,
            i.Quantity,
            i.Unit,
            i.Sequence,
            i.Notes)).ToList() ?? new List<RecipeItemRequest>();

        var model = new UpdateRecipeVersionRequest(
            version.Id,
            version.RecipeId,
            version.VersionNumber,
            version.VersionName,
            version.EffectiveFrom,
            version.EffectiveTo,
            version.ExpectedOutput,
            version.OutputUnit,
            version.ExpectedWastePercentage,
            version.LaborCost,
            version.MachineCost,
            version.OverheadCost,
            version.Notes,
            itemsList);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Recipes/EditVersion/{versionId?}")]
    public async Task<IActionResult> EditVersion(int? versionId, UpdateRecipeVersionRequest request, [FromQuery] int? id)
    {
        var targetId = versionId ?? id ?? request.Id;
        if (request.Id == 0 && targetId > 0)
        {
            request.Id = targetId;
        }

        if (targetId != request.Id) return BadRequest();

        try
        {
            var updated = await _serviceManager.RecipeService.UpdateVersionAsync(request);
            TempData["Success"] = $"تم تحديث بيانات الإصدار '{updated.VersionNumber}' وقائمة المواد بنجاح.";
            return RedirectToAction(nameof(DetailsVersion), new { versionId = updated.Id });
        }
        catch (ValidationException ex)
        {
            foreach (var error in ex.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
        }

        var recipe = await _serviceManager.RecipeService.GetRecipeByIdAsync(request.RecipeId);
        ViewBag.Recipe = recipe;
        await PopulateMaterialsViewBag();
        return View(request);
    }

    [HttpGet]
    [Route("Recipes/DetailsVersion/{versionId?}")]
    public async Task<IActionResult> DetailsVersion(int? versionId, [FromQuery] int? id)
    {
        var targetId = versionId ?? id ?? 0;
        var version = await _serviceManager.RecipeService.GetVersionByIdAsync(targetId);
        if (version == null) return NotFound();
        return View(version);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Recipes/ActivateVersion/{versionId?}")]
    public async Task<IActionResult> ActivateVersion(int? versionId, [FromQuery] int? id)
    {
        var targetId = versionId ?? id ?? 0;
        try
        {
            var result = await _serviceManager.RecipeService.ActivateVersionAsync(targetId);
            if (!result) return NotFound();
            TempData["Success"] = "تم اعتماد وتنشيط إصدار الوصفة بنجاح للاستخدام في خطط وتكاليف الإنتاج.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(DetailsVersion), new { versionId = targetId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Recipes/DeactivateVersion/{versionId?}")]
    public async Task<IActionResult> DeactivateVersion(int? versionId, [FromQuery] int? id)
    {
        var targetId = versionId ?? id ?? 0;
        var result = await _serviceManager.RecipeService.DeactivateVersionAsync(targetId);
        if (!result) return NotFound();

        TempData["Success"] = "تم إيقاف تنشيط إصدار الوصفة.";
        return RedirectToAction(nameof(DetailsVersion), new { versionId = targetId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Recipes/DeleteVersion/{versionId?}")]
    public async Task<IActionResult> DeleteVersion(int? versionId, int recipeId, [FromQuery] int? id)
    {
        var targetId = versionId ?? id ?? 0;
        try
        {
            await _serviceManager.RecipeService.DeleteVersionAsync(targetId);
            TempData["Success"] = "تم حذف إصدار الوصفة بنجاح.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = recipeId });
    }

    #endregion

    #region Helpers & Live API

    [HttpGet]
    public async Task<IActionResult> GetMaterialsJson()
    {
        var materials = await _serviceManager.MaterialService.GetActiveMaterialsAsync();
        var result = materials.Select(m => new
        {
            id = m.Id,
            code = m.Code,
            name = m.Name,
            arabicName = m.ArabicName,
            unit = m.Unit,
            cost = m.CurrentCost
        });
        return Json(result);
    }

    private async Task PopulateProductsDropdown(int? selectedProductId = null)
    {
        var products = await _serviceManager.ProductService.GetActiveProductsAsync();
        ViewBag.Products = new SelectList(products, "Id", "Name", selectedProductId);
    }

    private async Task PopulateMaterialsViewBag()
    {
        var materials = await _serviceManager.MaterialService.GetActiveMaterialsAsync();
        ViewBag.Materials = materials.ToList();
    }

    #endregion
}
