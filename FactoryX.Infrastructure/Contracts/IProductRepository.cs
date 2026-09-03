using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IProductRepository : IBaseRepository<Product>
{
    Task<IEnumerable<Product>> GetFilteredProductsAsync(
        string? search,
        int? categoryId,
        bool? isActive,
        ProductType? productType);

    Task<Product?> GetProductWithDetailsAsync(int id);
    Task<IEnumerable<Product>> GetActiveProductsAsync();
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    Task<bool> IsSkuUniqueAsync(string sku, int? excludeId = null);
    Task<bool> IsBarcodeUniqueAsync(string barcode, int? excludeId = null);
    Task<bool> HasWorkOrdersOrRecordsAsync(int productId);
}
