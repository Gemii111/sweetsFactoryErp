using FactoryX.Application.DTOs;
using LegacyInsertProductRequest = FactoryX.Application.DTOs.Requests.ProductRequests.InsertProductRequest;
using LegacyUpdateProductRequest = FactoryX.Application.DTOs.Requests.ProductRequests.UpdateProductRequest;
using FactoryX.Application.DTOs.Responses.Product;
using FactoryX.Application.DTOs.Responses.ProductResponses;

namespace FactoryX.Application.Services.Abstracts;

public interface IProductService
{
    // Phase 5 Finished Product Master Data Methods
    Task<IEnumerable<ProductDto>> GetAllProductsAsync(ProductFilterRequest? filter);
    Task<ProductDto?> GetProductByIdAsync(int id);
    Task<ProductDto?> GetProductDetailsAsync(int id);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request);
    Task<ProductDto> UpdateProductAsync(UpdateProductRequest request);
    Task<bool> ToggleActiveAsync(int id);
    Task<bool> DeleteProductAsync(int id);
    Task<ProductSummaryDto> GetProductSummaryAsync();
    Task<IEnumerable<ProductDto>> GetActiveProductsAsync();

    // Legacy Methods for Backwards Compatibility
    Task<IEnumerable<GetAllProductResponse>> GetAllAsync();
    Task<GetProductResponse?> GetByIdAsync(int id);
    Task<InsertProductResponse> CreateAsync(LegacyInsertProductRequest request);
    Task UpdateAsync(LegacyUpdateProductRequest request);
    Task DeleteAsync(FactoryX.Application.DTOs.Requests.ProductRequests.DeleteProductRequest request);
}