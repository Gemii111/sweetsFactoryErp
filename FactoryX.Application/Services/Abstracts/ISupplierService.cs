using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface ISupplierService
{
    Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync(
        string? searchTerm = null,
        int? categoryId = null,
        bool? isActive = null);

    Task<SupplierDto?> GetSupplierByIdAsync(int id);
    Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request);
    Task<SupplierDto> UpdateSupplierAsync(UpdateSupplierRequest request);
    Task<bool> ToggleActiveAsync(int id);
    Task<bool> DeleteSupplierAsync(int id);
    Task<SupplierSummaryDto> GetSummaryAsync();

    // Categories
    Task<IEnumerable<SupplierCategoryDto>> GetAllCategoriesAsync(bool onlyActive = false);
    Task<SupplierCategoryDto?> GetCategoryByIdAsync(int id);
    Task<SupplierCategoryDto> CreateCategoryAsync(CreateSupplierCategoryRequest request);

    // Price History
    Task<IEnumerable<SupplierPriceHistoryDto>> GetPriceHistoryAsync(int? supplierId = null, int? materialId = null);
}
