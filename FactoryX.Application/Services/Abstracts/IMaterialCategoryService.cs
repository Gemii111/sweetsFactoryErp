using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IMaterialCategoryService
{
    Task<IEnumerable<MaterialCategoryDto>> GetAllCategoriesAsync(bool trackChanges = false);
    Task<MaterialCategoryDto?> GetCategoryByIdAsync(int id, bool trackChanges = false);
    Task<MaterialCategoryDto> CreateCategoryAsync(CreateMaterialCategoryRequest request);
    Task<MaterialCategoryDto> UpdateCategoryAsync(UpdateMaterialCategoryRequest request);
    Task<bool> ToggleCategoryStatusAsync(int id);
    Task<bool> DeleteCategoryAsync(int id);
}
