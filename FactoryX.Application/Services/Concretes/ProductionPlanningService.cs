using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class ProductionPlanningService : IProductionPlanningService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public ProductionPlanningService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<List<MaterialRequirementDto>> CalculateMaterialRequirementsAsync(int recipeVersionId, decimal plannedQuantity)
    {
        if (recipeVersionId <= 0 || plannedQuantity <= 0)
        {
            return new List<MaterialRequirementDto>();
        }

        var version = await _repositoryManager.RecipeVersionRepository.GetVersionWithItemsAndCostsAsync(recipeVersionId);
        if (version == null)
        {
            throw new KeyNotFoundException($"إصدار الوصفة بالمعرف {recipeVersionId} غير موجود.");
        }

        if (version.ExpectedOutput <= 0)
        {
            throw new InvalidOperationException($"الكمية المستهدفة للتشغيلة في الوصفة ({version.ExpectedOutput}) غير صالحة.");
        }

        var items = version.Items?.OrderBy(i => i.Sequence).ToList() ?? new List<RecipeItem>();
        var result = new List<MaterialRequirementDto>();

        foreach (var item in items)
        {
            // Formula: Required = RecipeItem.Quantity * (PlannedQuantity / RecipeVersion.ExpectedOutput)
            var requiredQty = Math.Round(item.Quantity * (plannedQuantity / version.ExpectedOutput), 4);

            // Fetch current stock from Phase 3 Inventory / Material master
            decimal currentStock = 0m;
            var balances = await _repositoryManager.StockBalanceRepository.GetStockBalancesAsync(null, null, item.MaterialId, null, null);
            var balanceList = balances.ToList();
            if (balanceList.Any())
            {
                currentStock = balanceList.Sum(b => b.Quantity);
            }
            else if (item.Material != null)
            {
                currentStock = item.Material.CurrentStock;
            }
            else
            {
                var mat = await _repositoryManager.MaterialRepository.GetByIdAsync(item.MaterialId);
                if (mat != null) currentStock = mat.CurrentStock;
            }

            var shortage = Math.Max(0m, requiredQty - currentStock);
            var availability = currentStock >= requiredQty ? MaterialAvailabilityStatus.Available :
                               currentStock > 0 ? MaterialAvailabilityStatus.Shortage :
                               MaterialAvailabilityStatus.OutOfStock;

            result.Add(new MaterialRequirementDto
            {
                MaterialId = item.MaterialId,
                MaterialCode = item.Material?.Code ?? string.Empty,
                MaterialName = item.Material?.Name ?? string.Empty,
                MaterialArabicName = item.Material?.ArabicName,
                StockUnit = string.IsNullOrWhiteSpace(item.Unit) ? (item.Material?.Unit ?? "KG") : item.Unit,
                RecipeQuantity = item.Quantity,
                ExpectedOutputQuantity = version.ExpectedOutput,
                PlannedProductionQuantity = plannedQuantity,
                RequiredQuantity = requiredQty,
                CurrentStock = currentStock,
                ShortageQuantity = shortage,
                AvailabilityStatus = availability
            });
        }

        return result;
    }

    public async Task<IEnumerable<RecipeVersionDto>> GetActiveRecipeVersionsForProductAsync(int productId, DateTime plannedDate)
    {
        if (productId <= 0) return Enumerable.Empty<RecipeVersionDto>();

        var recipes = await _repositoryManager.RecipeRepository.GetFilteredRecipesAsync(null, productId, true);
        var activeVersions = new List<RecipeVersionDto>();

        foreach (var recipe in recipes.Where(r => r.IsActive))
        {
            if (recipe.Versions == null) continue;

            foreach (var v in recipe.Versions)
            {
                if (v.Status == RecipeStatus.Active && v.IsActive)
                {
                    // Check effective date window
                    var isEffective = v.EffectiveFrom.Date <= plannedDate.Date &&
                                      (!v.EffectiveTo.HasValue || v.EffectiveTo.Value.Date >= plannedDate.Date);

                    if (isEffective)
                    {
                        var dto = _mapper.Map<RecipeVersionDto>(v);
                        activeVersions.Add(dto);
                    }
                }
            }
        }

        return activeVersions;
    }

    public async Task ValidateRecipeVersionForPlanningAsync(int productId, int recipeVersionId, DateTime plannedDate)
    {
        var product = await _repositoryManager.ProductRepository.GetByIdAsync(productId);
        if (product == null || !product.IsActive)
        {
            throw new InvalidOperationException("المنتج التام المحدد غير موجود أو غير نشط في النظام.");
        }

        var version = await _repositoryManager.RecipeVersionRepository.GetVersionWithItemsAndCostsAsync(recipeVersionId);
        if (version == null)
        {
            throw new InvalidOperationException($"إصدار الوصفة بالمعرف {recipeVersionId} غير موجود.");
        }

        var recipe = await _repositoryManager.RecipeRepository.GetByIdAsync(version.RecipeId);
        if (recipe == null || recipe.ProductId != productId)
        {
            throw new InvalidOperationException($"إصدار الوصفة '{version.VersionNumber}' لا ينتمي إلى المنتج التام المختار ('{product.Name}').");
        }

        if (version.Status != RecipeStatus.Active || !version.IsActive)
        {
            throw new InvalidOperationException($"إصدار الوصفة '{version.VersionNumber}' ليس في حالة نشطة ومعتمدة (Active). لا يمكن التخطيط باستخدام مسودة أو إصدار معطل.");
        }

        if (version.EffectiveFrom.Date > plannedDate.Date)
        {
            throw new InvalidOperationException($"إصدار الوصفة '{version.VersionNumber}' يبدأ سريانه في {version.EffectiveFrom:yyyy-MM-dd} وهو لاحق لتاريخ الإنتاج المخطط ({plannedDate:yyyy-MM-dd}).");
        }

        if (version.EffectiveTo.HasValue && version.EffectiveTo.Value.Date < plannedDate.Date)
        {
            throw new InvalidOperationException($"إصدار الوصفة '{version.VersionNumber}' انتهت فترة سريانه في {version.EffectiveTo.Value:yyyy-MM-dd}.");
        }
    }
}
