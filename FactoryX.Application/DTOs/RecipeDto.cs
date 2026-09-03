using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class RecipeDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductSKU { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public string? Description { get; set; }
    public decimal BaseOutputQuantity { get; set; }
    public string Unit { get; set; } = "KG";
    public bool IsActive { get; set; } = true;

    // Statistics & Active Version Info
    public int VersionCount { get; set; }
    public int? ActiveVersionId { get; set; }
    public string? ActiveVersionNumber { get; set; }
    public decimal? ActiveVersionOutput { get; set; }
    public decimal? ActiveVersionCostPerUnit { get; set; }

    public ICollection<RecipeVersionDto>? Versions { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RecipeVersionDto
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public string RecipeCode { get; set; } = string.Empty;
    public string RecipeName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public string VersionNumber { get; set; } = "V1.0";
    public string? VersionName { get; set; }
    public RecipeStatus Status { get; set; } = RecipeStatus.Draft;
    public string StatusName => Status switch
    {
        RecipeStatus.Draft => "مسودة (Draft)",
        RecipeStatus.Active => "نشطة ومعتمدة (Active)",
        RecipeStatus.Inactive => "معطلة (Inactive)",
        _ => Status.ToString()
    };

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public decimal ExpectedOutput { get; set; }
    public string OutputUnit { get; set; } = "KG";
    public decimal ExpectedWastePercentage { get; set; }

    public decimal LaborCost { get; set; }
    public decimal MachineCost { get; set; }
    public decimal OverheadCost { get; set; }

    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }

    public ICollection<RecipeItemDto>? Items { get; set; }
    public RecipeCostBreakdownDto? CostBreakdown { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class RecipeItemDto
{
    public int Id { get; set; }
    public int RecipeVersionId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialCode { get; set; } = string.Empty;
    public string MaterialName { get; set; } = string.Empty;
    public string? MaterialArabicName { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";
    public decimal? Percentage { get; set; }
    public int Sequence { get; set; } = 1;
    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public string? Notes { get; set; }
}

public class RecipeCostBreakdownDto
{
    public decimal MaterialCost { get; set; }
    public decimal LaborCost { get; set; }
    public decimal MachineCost { get; set; }
    public decimal OverheadCost { get; set; }
    public decimal WasteCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal ExpectedOutput { get; set; }
    public string OutputUnit { get; set; } = "KG";
    public decimal CostPerOutputUnit { get; set; }
    public DateTime CalculatedAt { get; set; } = DateTime.UtcNow;
    public bool IsLiveEstimate { get; set; } = true;
    public string FormulaExplanation { get; set; } = "Total Cost = Material Cost + Labor Cost + Machine Cost + Overhead + Waste Cost. Cost / Unit = Total Cost / Expected Output Quantity.";
}

public class CreateRecipeRequest
{
    public int ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public string? Description { get; set; }
    public decimal BaseOutputQuantity { get; set; } = 100m;
    public string? Unit { get; set; } = "KG";

    public CreateRecipeRequest() { }
    public CreateRecipeRequest(int productId, string code, string name, string? arabicName, string? description, decimal baseOutputQuantity, string? unit)
    {
        ProductId = productId;
        Code = code;
        Name = name;
        ArabicName = arabicName;
        Description = description;
        BaseOutputQuantity = baseOutputQuantity;
        Unit = unit;
    }
}

public class UpdateRecipeRequest
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public string? Description { get; set; }
    public decimal BaseOutputQuantity { get; set; }
    public string? Unit { get; set; }
    public bool IsActive { get; set; } = true;

    public UpdateRecipeRequest() { }
    public UpdateRecipeRequest(int id, int productId, string code, string name, string? arabicName, string? description, decimal baseOutputQuantity, string? unit, bool isActive)
    {
        Id = id;
        ProductId = productId;
        Code = code;
        Name = name;
        ArabicName = arabicName;
        Description = description;
        BaseOutputQuantity = baseOutputQuantity;
        Unit = unit;
        IsActive = isActive;
    }
}

public class RecipeItemRequest
{
    public int MaterialId { get; set; }
    public decimal Quantity { get; set; }
    public string? Unit { get; set; } = "KG";
    public int Sequence { get; set; } = 1;
    public string? Notes { get; set; }

    public RecipeItemRequest() { }
    public RecipeItemRequest(int materialId, decimal quantity, string? unit, int sequence, string? notes)
    {
        MaterialId = materialId;
        Quantity = quantity;
        Unit = unit;
        Sequence = sequence;
        Notes = notes;
    }
}

public class CreateRecipeVersionRequest
{
    public int RecipeId { get; set; }
    public string VersionNumber { get; set; } = "V1.0";
    public string? VersionName { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;
    public DateTime? EffectiveTo { get; set; }
    public decimal ExpectedOutput { get; set; } = 100m;
    public string? OutputUnit { get; set; } = "KG";
    public decimal ExpectedWastePercentage { get; set; }
    public decimal LaborCost { get; set; }
    public decimal MachineCost { get; set; }
    public decimal OverheadCost { get; set; }
    public string? Notes { get; set; }
    public List<RecipeItemRequest> Items { get; set; } = new();

    public CreateRecipeVersionRequest() { }
    public CreateRecipeVersionRequest(int recipeId, string versionNumber, string? versionName, DateTime effectiveFrom, DateTime? effectiveTo, decimal expectedOutput, string? outputUnit, decimal expectedWastePercentage, decimal laborCost, decimal machineCost, decimal overheadCost, string? notes, List<RecipeItemRequest>? items)
    {
        RecipeId = recipeId;
        VersionNumber = versionNumber;
        VersionName = versionName;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        ExpectedOutput = expectedOutput;
        OutputUnit = outputUnit;
        ExpectedWastePercentage = expectedWastePercentage;
        LaborCost = laborCost;
        MachineCost = machineCost;
        OverheadCost = overheadCost;
        Notes = notes;
        Items = items ?? new List<RecipeItemRequest>();
    }
}

public class UpdateRecipeVersionRequest
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public string VersionNumber { get; set; } = "V1.0";
    public string? VersionName { get; set; }
    public DateTime EffectiveFrom { get; set; } = DateTime.UtcNow.Date;
    public DateTime? EffectiveTo { get; set; }
    public decimal ExpectedOutput { get; set; } = 100m;
    public string? OutputUnit { get; set; } = "KG";
    public decimal ExpectedWastePercentage { get; set; }
    public decimal LaborCost { get; set; }
    public decimal MachineCost { get; set; }
    public decimal OverheadCost { get; set; }
    public string? Notes { get; set; }
    public List<RecipeItemRequest> Items { get; set; } = new();

    public UpdateRecipeVersionRequest() { }
    public UpdateRecipeVersionRequest(int id, int recipeId, string versionNumber, string? versionName, DateTime effectiveFrom, DateTime? effectiveTo, decimal expectedOutput, string? outputUnit, decimal expectedWastePercentage, decimal laborCost, decimal machineCost, decimal overheadCost, string? notes, List<RecipeItemRequest>? items)
    {
        Id = id;
        RecipeId = recipeId;
        VersionNumber = versionNumber;
        VersionName = versionName;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        ExpectedOutput = expectedOutput;
        OutputUnit = outputUnit;
        ExpectedWastePercentage = expectedWastePercentage;
        LaborCost = laborCost;
        MachineCost = machineCost;
        OverheadCost = overheadCost;
        Notes = notes;
        Items = items ?? new List<RecipeItemRequest>();
    }
}

public record RecipeFilterRequest(
    string? Search,
    int? ProductId,
    bool? IsActive);

public record RecipeSummaryDto(
    int TotalRecipes,
    int ActiveRecipes,
    int TotalVersions,
    int ActiveVersions,
    int DraftVersions,
    int ProductsWithRecipesCount);
