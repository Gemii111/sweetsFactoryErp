using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;

namespace FactoryX.Application.Services.Concretes;

public class RecipeService : IRecipeService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IRecipeCostService _recipeCostService;
    private readonly IValidator<CreateRecipeRequest>? _createRecipeValidator;
    private readonly IValidator<UpdateRecipeRequest>? _updateRecipeValidator;
    private readonly IValidator<CreateRecipeVersionRequest>? _createVersionValidator;
    private readonly IValidator<UpdateRecipeVersionRequest>? _updateVersionValidator;

    public RecipeService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IRecipeCostService recipeCostService,
        IValidator<CreateRecipeRequest>? createRecipeValidator = null,
        IValidator<UpdateRecipeRequest>? updateRecipeValidator = null,
        IValidator<CreateRecipeVersionRequest>? createVersionValidator = null,
        IValidator<UpdateRecipeVersionRequest>? updateVersionValidator = null)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _recipeCostService = recipeCostService;
        _createRecipeValidator = createRecipeValidator;
        _updateRecipeValidator = updateRecipeValidator;
        _createVersionValidator = createVersionValidator;
        _updateVersionValidator = updateVersionValidator;
    }

    #region Recipe Master Operations

    public async Task<IEnumerable<RecipeDto>> GetAllRecipesAsync(RecipeFilterRequest? filter)
    {
        var recipes = await _repositoryManager.RecipeRepository.GetFilteredRecipesAsync(
            filter?.Search,
            filter?.ProductId,
            filter?.IsActive);

        var dtos = new List<RecipeDto>();
        foreach (var r in recipes)
        {
            var dto = _mapper.Map<RecipeDto>(r);
            dto.VersionCount = r.Versions?.Count ?? 0;

            // Find active version
            var activeVer = r.Versions?.FirstOrDefault(v => v.Status == RecipeStatus.Active && v.IsActive);
            if (activeVer != null)
            {
                dto.ActiveVersionId = activeVer.Id;
                dto.ActiveVersionNumber = activeVer.VersionNumber;
                dto.ActiveVersionOutput = activeVer.ExpectedOutput;
                var cost = await _recipeCostService.CalculateVersionCostAsync(activeVer);
                dto.ActiveVersionCostPerUnit = cost.CostPerOutputUnit;
            }
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<RecipeDto?> GetRecipeByIdAsync(int id)
    {
        var recipe = await _repositoryManager.RecipeRepository.GetRecipeWithDetailsAsync(id);
        if (recipe == null) return null;

        var dto = _mapper.Map<RecipeDto>(recipe);
        dto.VersionCount = recipe.Versions?.Count ?? 0;

        var activeVer = recipe.Versions?.FirstOrDefault(v => v.Status == RecipeStatus.Active && v.IsActive);
        if (activeVer != null)
        {
            dto.ActiveVersionId = activeVer.Id;
            dto.ActiveVersionNumber = activeVer.VersionNumber;
            dto.ActiveVersionOutput = activeVer.ExpectedOutput;
            var cost = await _recipeCostService.CalculateVersionCostAsync(activeVer);
            dto.ActiveVersionCostPerUnit = cost.CostPerOutputUnit;
        }

        return dto;
    }

    public async Task<RecipeDto?> GetRecipeDetailsAsync(int id)
    {
        var recipe = await _repositoryManager.RecipeRepository.GetRecipeWithDetailsAsync(id);
        if (recipe == null) return null;

        var dto = _mapper.Map<RecipeDto>(recipe);
        dto.VersionCount = recipe.Versions?.Count ?? 0;

        if (recipe.Versions != null && recipe.Versions.Any())
        {
            var versionDtos = new List<RecipeVersionDto>();
            foreach (var ver in recipe.Versions.OrderByDescending(v => v.EffectiveFrom))
            {
                var vDto = _mapper.Map<RecipeVersionDto>(ver);
                vDto.CostBreakdown = await _recipeCostService.CalculateVersionCostAsync(ver);
                versionDtos.Add(vDto);
            }
            dto.Versions = versionDtos;

            var activeVer = versionDtos.FirstOrDefault(v => v.Status == RecipeStatus.Active && v.IsActive);
            if (activeVer != null)
            {
                dto.ActiveVersionId = activeVer.Id;
                dto.ActiveVersionNumber = activeVer.VersionNumber;
                dto.ActiveVersionOutput = activeVer.ExpectedOutput;
                dto.ActiveVersionCostPerUnit = activeVer.CostBreakdown?.CostPerOutputUnit;
            }
        }

        return dto;
    }

    public async Task<RecipeDto> CreateRecipeAsync(CreateRecipeRequest request)
    {
        if (_createRecipeValidator != null)
        {
            var validationResult = await _createRecipeValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        if (!await _repositoryManager.RecipeRepository.IsCodeUniqueAsync(request.Code))
        {
            throw new InvalidOperationException($"كود الوصفة '{request.Code}' مستخدم بالفعل في النظام. يرجى اختيار كود فريد.");
        }

        var product = await _repositoryManager.ProductRepository.GetByIdAsync(request.ProductId);
        if (product == null || !product.IsActive)
        {
            throw new InvalidOperationException("المنتج المختار غير موجود أو غير نشط في النظام.");
        }

        var recipe = _mapper.Map<Recipe>(request);
        recipe.IsActive = true;
        recipe.CreatedAt = DateTime.UtcNow;
        recipe.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.RecipeRepository.Create(recipe);
        await _repositoryManager.SaveAsync();

        return (await GetRecipeByIdAsync(recipe.Id))!;
    }

    public async Task<RecipeDto> UpdateRecipeAsync(UpdateRecipeRequest request)
    {
        if (_updateRecipeValidator != null)
        {
            var validationResult = await _updateRecipeValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        var recipe = await _repositoryManager.RecipeRepository.GetByIdAsync(request.Id, trackChanges: true);
        if (recipe == null)
        {
            throw new KeyNotFoundException($"الوصفة بالمعرف {request.Id} غير موجودة.");
        }

        if (!await _repositoryManager.RecipeRepository.IsCodeUniqueAsync(request.Code, request.Id))
        {
            throw new InvalidOperationException($"كود الوصفة '{request.Code}' مستخدم لوصفة أخرى.");
        }

        _mapper.Map(request, recipe);
        recipe.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.RecipeRepository.Update(recipe);
        await _repositoryManager.SaveAsync();

        return (await GetRecipeByIdAsync(recipe.Id))!;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var recipe = await _repositoryManager.RecipeRepository.GetByIdAsync(id, trackChanges: true);
        if (recipe == null) return false;

        recipe.IsActive = !recipe.IsActive;
        recipe.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.RecipeRepository.Update(recipe);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteRecipeAsync(int id)
    {
        var recipe = await _repositoryManager.RecipeRepository.GetRecipeWithDetailsAsync(id);
        if (recipe == null) return false;

        // Check if any version has work orders
        var hasWorkOrders = recipe.Versions?.Any(v => v.WorkOrders != null && v.WorkOrders.Any()) ?? false;
        if (hasWorkOrders)
        {
            throw new InvalidOperationException("لا يمكن حذف هذه الوصفة لوجود أوامر تشغيل سابقة مرتبطة بإصداراتها. يمكنك تعطيل الوصفة بدلاً من حذفها.");
        }

        _repositoryManager.RecipeRepository.Remove(recipe);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<RecipeSummaryDto> GetRecipeSummaryAsync()
    {
        var allRecipes = await _repositoryManager.RecipeRepository.GetAllAsync();
        var total = allRecipes.Count();
        var active = allRecipes.Count(r => r.IsActive);
        var productsWithRecipes = allRecipes.Select(r => r.ProductId).Distinct().Count();

        var allVersions = await _repositoryManager.RecipeVersionRepository.GetAllAsync();
        var totalVersions = allVersions.Count();
        var activeVersions = allVersions.Count(v => v.Status == RecipeStatus.Active && v.IsActive);
        var draftVersions = allVersions.Count(v => v.Status == RecipeStatus.Draft);

        return new RecipeSummaryDto(total, active, totalVersions, activeVersions, draftVersions, productsWithRecipes);
    }

    #endregion

    #region Recipe Version Operations

    public async Task<RecipeVersionDto?> GetVersionByIdAsync(int versionId)
    {
        var version = await _repositoryManager.RecipeVersionRepository.GetVersionWithItemsAndCostsAsync(versionId);
        if (version == null) return null;

        var dto = _mapper.Map<RecipeVersionDto>(version);
        dto.CostBreakdown = await _recipeCostService.CalculateVersionCostAsync(version);

        // Populate item names and costs
        if (dto.Items != null && version.Items != null)
        {
            var totalQty = version.Items.Sum(i => i.Quantity);
            foreach (var itemDto in dto.Items)
            {
                var entityItem = version.Items.FirstOrDefault(i => i.Id == itemDto.Id);
                if (entityItem?.Material != null)
                {
                    itemDto.MaterialCode = entityItem.Material.Code;
                    itemDto.MaterialName = entityItem.Material.Name;
                    itemDto.MaterialArabicName = entityItem.Material.ArabicName;
                    itemDto.UnitCost = entityItem.Material.CurrentCost;
                    itemDto.TotalCost = itemDto.Quantity * entityItem.Material.CurrentCost;
                    itemDto.Percentage = totalQty > 0 ? Math.Round((itemDto.Quantity / totalQty) * 100m, 2) : 0m;
                }
            }
        }

        return dto;
    }

    public async Task<RecipeVersionDto> CreateVersionAsync(CreateRecipeVersionRequest request)
    {
        if (_createVersionValidator != null)
        {
            var validationResult = await _createVersionValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        var recipe = await _repositoryManager.RecipeRepository.GetByIdAsync(request.RecipeId);
        if (recipe == null)
        {
            throw new KeyNotFoundException($"الوصفة بالمعرف {request.RecipeId} غير موجودة.");
        }

        if (!await _repositoryManager.RecipeVersionRepository.IsVersionNumberUniqueAsync(request.RecipeId, request.VersionNumber))
        {
            throw new InvalidOperationException($"رقم الإصدار '{request.VersionNumber}' مستخدم بالفعل لهذه الوصفة.");
        }

        // Validate items: quantities > 0, active materials, no duplicates
        if (request.Items != null && request.Items.Any())
        {
            var duplicates = request.Items.GroupBy(i => i.MaterialId).Where(g => g.Count() > 1).ToList();
            if (duplicates.Any())
            {
                throw new InvalidOperationException("لا يمكن تكرار نفس المادة الخام أكثر من مرة في نفس إصدار الوصفة.");
            }

            var materialIds = request.Items.Select(i => i.MaterialId).Distinct().ToList();
            var materials = (await _repositoryManager.MaterialRepository.GetAllAsync())
                .Where(m => materialIds.Contains(m.Id))
                .ToDictionary(m => m.Id, m => m);

            foreach (var item in request.Items)
            {
                if (!materials.TryGetValue(item.MaterialId, out var mat) || !mat.IsActive)
                {
                    throw new InvalidOperationException($"المادة الخام بالمعرف {item.MaterialId} غير نشطة أو غير موجودة في النظام.");
                }
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException($"كمية المادة الخام '{mat.Name}' يجب أن تكون أكبر من الصفر.");
                }
            }
        }

        var version = _mapper.Map<RecipeVersion>(request);
        version.Status = RecipeStatus.Draft; // Always start as Draft
        version.IsActive = true;
        version.EffectiveDate = request.EffectiveFrom;
        version.CreatedAt = DateTime.UtcNow;
        version.UpdatedAt = DateTime.UtcNow;

        if (request.Items != null && request.Items.Any())
        {
            version.Items = new List<RecipeItem>();
            int seq = 1;
            foreach (var itemReq in request.Items)
            {
                version.Items.Add(new RecipeItem
                {
                    MaterialId = itemReq.MaterialId,
                    Quantity = itemReq.Quantity,
                    Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? "KG" : itemReq.Unit,
                    Sequence = itemReq.Sequence > 0 ? itemReq.Sequence : seq++,
                    Notes = itemReq.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        _repositoryManager.RecipeVersionRepository.Create(version);
        await _repositoryManager.SaveAsync();

        return (await GetVersionByIdAsync(version.Id))!;
    }

    public async Task<RecipeVersionDto> UpdateVersionAsync(UpdateRecipeVersionRequest request)
    {
        if (_updateVersionValidator != null)
        {
            var validationResult = await _updateVersionValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        var version = await _repositoryManager.RecipeVersionRepository.GetVersionWithItemsAndCostsAsync(request.Id);
        if (version == null)
        {
            throw new KeyNotFoundException($"إصدار الوصفة بالمعرف {request.Id} غير موجود.");
        }

        if (version.Status == RecipeStatus.Active)
        {
            // If active and has effective date changes, check overlap
            if (await _repositoryManager.RecipeVersionRepository.HasOverlappingActiveVersionAsync(
                version.RecipeId, request.EffectiveFrom, request.EffectiveTo, version.Id))
            {
                throw new InvalidOperationException("يوجد بالفعل إصدار نشط آخر لنفس الوصفة يتداخل في نفس فترة السريان المحددة.");
            }
        }

        if (!await _repositoryManager.RecipeVersionRepository.IsVersionNumberUniqueAsync(request.RecipeId, request.VersionNumber, request.Id))
        {
            throw new InvalidOperationException($"رقم الإصدار '{request.VersionNumber}' مستخدم لإصدار آخر في هذه الوصفة.");
        }

        // Validate items
        if (request.Items != null && request.Items.Any())
        {
            var duplicates = request.Items.GroupBy(i => i.MaterialId).Where(g => g.Count() > 1).ToList();
            if (duplicates.Any())
            {
                throw new InvalidOperationException("لا يمكن تكرار نفس المادة الخام أكثر من مرة في نفس إصدار الوصفة.");
            }

            var materialIds = request.Items.Select(i => i.MaterialId).Distinct().ToList();
            var materials = (await _repositoryManager.MaterialRepository.GetAllAsync())
                .Where(m => materialIds.Contains(m.Id))
                .ToDictionary(m => m.Id, m => m);

            foreach (var item in request.Items)
            {
                if (!materials.TryGetValue(item.MaterialId, out var mat) || !mat.IsActive)
                {
                    throw new InvalidOperationException($"المادة الخام بالمعرف {item.MaterialId} غير نشطة أو غير موجودة في النظام.");
                }
                if (item.Quantity <= 0)
                {
                    throw new InvalidOperationException($"كمية المادة الخام '{mat.Name}' يجب أن تكون أكبر من الصفر.");
                }
            }
        }

        version.VersionNumber = request.VersionNumber;
        version.VersionName = request.VersionName;
        version.EffectiveFrom = request.EffectiveFrom;
        version.EffectiveTo = request.EffectiveTo;
        version.EffectiveDate = request.EffectiveFrom;
        version.ExpectedOutput = request.ExpectedOutput;
        version.OutputUnit = string.IsNullOrWhiteSpace(request.OutputUnit) ? "KG" : request.OutputUnit;
        version.ExpectedWastePercentage = request.ExpectedWastePercentage;
        version.LaborCost = request.LaborCost;
        version.MachineCost = request.MachineCost;
        version.OverheadCost = request.OverheadCost;
        version.Notes = request.Notes;
        version.UpdatedAt = DateTime.UtcNow;

        // Replace items
        if (version.Items != null)
        {
            version.Items.Clear();
        }
        else
        {
            version.Items = new List<RecipeItem>();
        }

        if (request.Items != null && request.Items.Any())
        {
            int seq = 1;
            foreach (var itemReq in request.Items)
            {
                version.Items.Add(new RecipeItem
                {
                    RecipeVersionId = version.Id,
                    MaterialId = itemReq.MaterialId,
                    Quantity = itemReq.Quantity,
                    Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? "KG" : itemReq.Unit,
                    Sequence = itemReq.Sequence > 0 ? itemReq.Sequence : seq++,
                    Notes = itemReq.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        _repositoryManager.RecipeVersionRepository.Update(version);
        await _repositoryManager.SaveAsync();

        return (await GetVersionByIdAsync(version.Id))!;
    }

    public async Task<bool> ActivateVersionAsync(int versionId)
    {
        var version = await _repositoryManager.RecipeVersionRepository.GetByIdAsync(versionId, trackChanges: true);
        if (version == null) return false;

        // Check date overlap
        if (await _repositoryManager.RecipeVersionRepository.HasOverlappingActiveVersionAsync(
            version.RecipeId, version.EffectiveFrom, version.EffectiveTo, version.Id))
        {
            throw new InvalidOperationException("يوجد بالفعل إصدار نشط آخر لنفس الوصفة يتداخل في نفس فترة السريان المحددة. يرجى تعديل التواريخ أو إلغاء تنشيط الإصدار السابق أولاً.");
        }

        version.Status = RecipeStatus.Active;
        version.IsActive = true;
        version.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.RecipeVersionRepository.Update(version);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<bool> DeactivateVersionAsync(int versionId)
    {
        var version = await _repositoryManager.RecipeVersionRepository.GetByIdAsync(versionId, trackChanges: true);
        if (version == null) return false;

        version.Status = RecipeStatus.Inactive;
        version.IsActive = false;
        version.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.RecipeVersionRepository.Update(version);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteVersionAsync(int versionId)
    {
        var version = await _repositoryManager.RecipeVersionRepository.GetVersionWithItemsAndCostsAsync(versionId);
        if (version == null) return false;

        if (version.WorkOrders != null && version.WorkOrders.Any())
        {
            throw new InvalidOperationException("لا يمكن حذف هذا الإصدار لوجود أوامر تشغيل سابقة مسجلة عليه. يمكنك إلغاء تنشيطه بدلاً من حذفه.");
        }

        _repositoryManager.RecipeVersionRepository.Remove(version);
        await _repositoryManager.SaveAsync();
        return true;
    }

    #endregion
}
