using AutoMapper;
using FactoryX.Application.DTOs;
using LegacyInsertProductRequest = FactoryX.Application.DTOs.Requests.ProductRequests.InsertProductRequest;
using LegacyUpdateProductRequest = FactoryX.Application.DTOs.Requests.ProductRequests.UpdateProductRequest;
using FactoryX.Application.DTOs.Responses.Product;
using FactoryX.Application.DTOs.Responses.ProductResponses;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;

namespace FactoryX.Application.Services.Concretes;

public class ProductService : IProductService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProductRequest>? _createValidator;
    private readonly IValidator<UpdateProductRequest>? _updateValidator;

    public ProductService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IValidator<CreateProductRequest>? createValidator = null,
        IValidator<UpdateProductRequest>? updateValidator = null)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    #region Phase 5 Finished Product Master Data Methods

    public async Task<IEnumerable<ProductDto>> GetAllProductsAsync(ProductFilterRequest? filter)
    {
        var products = await _repositoryManager.ProductRepository.GetFilteredProductsAsync(
            filter?.Search,
            filter?.CategoryId,
            filter?.IsActive,
            filter?.ProductType);

        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        var product = await _repositoryManager.ProductRepository.GetProductWithDetailsAsync(id);
        return product == null ? null : _mapper.Map<ProductDto>(product);
    }

    public async Task<ProductDto?> GetProductDetailsAsync(int id)
    {
        var product = await _repositoryManager.ProductRepository.GetProductWithDetailsAsync(id);
        if (product == null) return null;

        var dto = _mapper.Map<ProductDto>(product);
        dto.WorkOrderCount = product.WorkOrders?.Count ?? 0;
        dto.ProductionRecordCount = product.WorkOrders?.SelectMany(w => w.ProductionRecords ?? Enumerable.Empty<ProductionRecord>()).Count() ?? 0;
        return dto;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductRequest request)
    {
        if (_createValidator != null)
        {
            var validationResult = await _createValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        // Uniqueness checks
        if (!await _repositoryManager.ProductRepository.IsCodeUniqueAsync(request.Code))
        {
            throw new InvalidOperationException($"كود المنتج '{request.Code}' مستخدم بالفعل في النظام. يرجى اختيار كود فريد آخر.");
        }

        if (!await _repositoryManager.ProductRepository.IsSkuUniqueAsync(request.SKU))
        {
            throw new InvalidOperationException($"رمز الصنف المخزني (SKU) '{request.SKU}' مستخدم بالفعل لمنتج آخر. يجب أن يكون الـ SKU فريداً.");
        }

        if (!string.IsNullOrWhiteSpace(request.Barcode) &&
            !await _repositoryManager.ProductRepository.IsBarcodeUniqueAsync(request.Barcode))
        {
            throw new InvalidOperationException($"الباركود '{request.Barcode}' مسجل مسبقاً لمنتج آخر في النظام.");
        }

        var product = _mapper.Map<Product>(request);

        // Calculate legacy UnitWeightKg
        product.UnitWeightKg = (request.WeightUnit?.ToUpperInvariant()) switch
        {
            "GRAM" => request.Weight / 1000m,
            "KG" => request.Weight,
            _ => request.Weight > 0 ? request.Weight : 1.0m
        };

        // Calculate legacy ExpiryPeriodDays
        product.ExpiryPeriodDays = (request.ExpiryUnit?.ToLowerInvariant()) switch
        {
            "months" => request.ExpiryPeriod * 30,
            "years" => request.ExpiryPeriod * 365,
            _ => request.ExpiryPeriod
        };

        product.IsActive = true;
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductRepository.Create(product);
        await _repositoryManager.SaveAsync();

        return (await GetProductByIdAsync(product.Id))!;
    }

    public async Task<ProductDto> UpdateProductAsync(UpdateProductRequest request)
    {
        if (_updateValidator != null)
        {
            var validationResult = await _updateValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }

        var product = await _repositoryManager.ProductRepository.GetByIdAsync(request.Id, trackChanges: true);
        if (product == null)
        {
            throw new KeyNotFoundException($"المنتج بالمعرف {request.Id} غير موجود.");
        }

        // Uniqueness checks excluding current product
        if (!await _repositoryManager.ProductRepository.IsCodeUniqueAsync(request.Code, request.Id))
        {
            throw new InvalidOperationException($"كود المنتج '{request.Code}' مستخدم لمنتج آخر في النظام.");
        }

        if (!await _repositoryManager.ProductRepository.IsSkuUniqueAsync(request.SKU, request.Id))
        {
            throw new InvalidOperationException($"رمز الصنف المخزني (SKU) '{request.SKU}' مستخدم لمنتج آخر.");
        }

        if (!string.IsNullOrWhiteSpace(request.Barcode) &&
            !await _repositoryManager.ProductRepository.IsBarcodeUniqueAsync(request.Barcode, request.Id))
        {
            throw new InvalidOperationException($"الباركود '{request.Barcode}' مسجل مسبقاً لمنتج آخر في النظام.");
        }

        _mapper.Map(request, product);

        // Calculate legacy UnitWeightKg
        product.UnitWeightKg = (request.WeightUnit?.ToUpperInvariant()) switch
        {
            "GRAM" => request.Weight / 1000m,
            "KG" => request.Weight,
            _ => request.Weight > 0 ? request.Weight : 1.0m
        };

        // Calculate legacy ExpiryPeriodDays
        product.ExpiryPeriodDays = (request.ExpiryUnit?.ToLowerInvariant()) switch
        {
            "months" => request.ExpiryPeriod * 30,
            "years" => request.ExpiryPeriod * 365,
            _ => request.ExpiryPeriod
        };

        product.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductRepository.Update(product);
        await _repositoryManager.SaveAsync();

        return (await GetProductByIdAsync(product.Id))!;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var product = await _repositoryManager.ProductRepository.GetByIdAsync(id, trackChanges: true);
        if (product == null) return false;

        product.IsActive = !product.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductRepository.Update(product);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        var product = await _repositoryManager.ProductRepository.GetByIdAsync(id, trackChanges: true);
        if (product == null) return false;

        var hasReferences = await _repositoryManager.ProductRepository.HasWorkOrdersOrRecordsAsync(id);
        if (hasReferences)
        {
            throw new InvalidOperationException("لا يمكن حذف هذا المنتج لوجود أوامر تشغيل أو سجلات إنتاج سابقة مرتبطة به. تم إلغاء تفعيل المنتج بدلاً من حذفه للحفاظ على سلامة البيانات التاريخية.");
        }

        _repositoryManager.ProductRepository.Remove(product);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<ProductSummaryDto> GetProductSummaryAsync()
    {
        var all = await _repositoryManager.ProductRepository.GetAllAsync();
        var total = all.Count();
        var active = all.Count(p => p.IsActive);
        var inactive = total - active;
        var finished = all.Count(p => p.ProductType == ProductType.FinishedProduct);
        var boxes = all.Count(p => p.ProductType == ProductType.AssortedBox);
        var categories = (await _repositoryManager.ProductCategoryRepository.GetAllAsync()).Count();

        return new ProductSummaryDto(total, active, inactive, finished, boxes, categories);
    }

    public async Task<IEnumerable<ProductDto>> GetActiveProductsAsync()
    {
        var products = await _repositoryManager.ProductRepository.GetActiveProductsAsync();
        return _mapper.Map<IEnumerable<ProductDto>>(products);
    }

    #endregion

    #region Legacy Methods for Backwards Compatibility

    public async Task<IEnumerable<GetAllProductResponse>> GetAllAsync()
    {
        var products = await _repositoryManager.ProductRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<GetAllProductResponse>>(products);
    }

    public async Task<GetProductResponse?> GetByIdAsync(int id)
    {
        var product = await _repositoryManager.ProductRepository.GetByIdAsync(id);
        return product == null ? null : _mapper.Map<GetProductResponse>(product);
    }

    public async Task<InsertProductResponse> CreateAsync(LegacyInsertProductRequest request)
    {
        var entity = _mapper.Map<Product>(request);
        entity.IsActive = true;
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductRepository.Create(entity);
        await _repositoryManager.SaveAsync();
        return _mapper.Map<InsertProductResponse>(entity);
    }

    public async Task UpdateAsync(LegacyUpdateProductRequest request)
    {
        var entity = _mapper.Map<Product>(request);
        entity.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductRepository.Update(entity);
        await _repositoryManager.SaveAsync();
    }

    public async Task DeleteAsync(FactoryX.Application.DTOs.Requests.ProductRequests.DeleteProductRequest request)
    {
        var entity = await _repositoryManager.ProductRepository.GetByIdAsync(request.Id, trackChanges: true);
        if (entity != null)
        {
            var hasReferences = await _repositoryManager.ProductRepository.HasWorkOrdersOrRecordsAsync(request.Id);
            if (hasReferences)
            {
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.ProductRepository.Update(entity);
            }
            else
            {
                _repositoryManager.ProductRepository.Remove(entity);
            }
            await _repositoryManager.SaveAsync();
        }
    }

    #endregion
}