using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IProductCategoryService
{
    Task<IEnumerable<ProductCategoryDto>> GetAllCategoriesAsync(bool trackChanges = false);
    Task<ProductCategoryDto?> GetCategoryByIdAsync(int id);
    Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryRequest request);
    Task<ProductCategoryDto> UpdateCategoryAsync(UpdateProductCategoryRequest request);
    Task<bool> ToggleActiveAsync(int id);
    Task<bool> DeleteCategoryAsync(int id);
}
