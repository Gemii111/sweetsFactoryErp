using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IRecipeService
{
    Task<IEnumerable<RecipeDto>> GetAllRecipesAsync(RecipeFilterRequest? filter);
    Task<RecipeDto?> GetRecipeByIdAsync(int id);
    Task<RecipeDto?> GetRecipeDetailsAsync(int id);
    Task<RecipeDto> CreateRecipeAsync(CreateRecipeRequest request);
    Task<RecipeDto> UpdateRecipeAsync(UpdateRecipeRequest request);
    Task<bool> ToggleActiveAsync(int id);
    Task<bool> DeleteRecipeAsync(int id);
    Task<RecipeSummaryDto> GetRecipeSummaryAsync();

    // Version Management
    Task<RecipeVersionDto?> GetVersionByIdAsync(int versionId);
    Task<RecipeVersionDto> CreateVersionAsync(CreateRecipeVersionRequest request);
    Task<RecipeVersionDto> UpdateVersionAsync(UpdateRecipeVersionRequest request);
    Task<bool> ActivateVersionAsync(int versionId);
    Task<bool> DeactivateVersionAsync(int versionId);
    Task<bool> DeleteVersionAsync(int versionId);
}
