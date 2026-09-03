using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;

namespace FactoryX.Application.Services.Concretes;

public class ProductCategoryService : IProductCategoryService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProductCategoryRequest> _createValidator;
    private readonly IValidator<UpdateProductCategoryRequest> _updateValidator;

    public ProductCategoryService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IValidator<CreateProductCategoryRequest> createValidator,
        IValidator<UpdateProductCategoryRequest> updateValidator)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<ProductCategoryDto>> GetAllCategoriesAsync(bool trackChanges = false)
    {
        var categories = await _repositoryManager.ProductCategoryRepository.GetAllWithProductsAsync(trackChanges);
        return _mapper.Map<IEnumerable<ProductCategoryDto>>(categories);
    }

    public async Task<ProductCategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _repositoryManager.ProductCategoryRepository.GetCategoryWithProductsAsync(id);
        return category == null ? null : _mapper.Map<ProductCategoryDto>(category);
    }

    public async Task<ProductCategoryDto> CreateCategoryAsync(CreateProductCategoryRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Uniqueness checks
        if (!await _repositoryManager.ProductCategoryRepository.IsCodeUniqueAsync(request.Code))
        {
            throw new InvalidOperationException($"كود التصنيف '{request.Code}' مستخدم بالفعل. يرجى اختيار كود آخر.");
        }

        if (!await _repositoryManager.ProductCategoryRepository.IsNameUniqueAsync(request.Name))
        {
            throw new InvalidOperationException($"اسم التصنيف '{request.Name}' مستخدم بالفعل. يرجى اختيار اسم آخر.");
        }

        var category = _mapper.Map<ProductCategory>(request);
        category.IsActive = true;
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductCategoryRepository.Create(category);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<ProductCategoryDto>(category);
    }

    public async Task<ProductCategoryDto> UpdateCategoryAsync(UpdateProductCategoryRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var category = await _repositoryManager.ProductCategoryRepository.GetByIdAsync(request.Id, trackChanges: true);
        if (category == null)
        {
            throw new KeyNotFoundException($"تصنيف المنتج بالمعرف {request.Id} غير موجود.");
        }

        if (!await _repositoryManager.ProductCategoryRepository.IsCodeUniqueAsync(request.Code, request.Id))
        {
            throw new InvalidOperationException($"كود التصنيف '{request.Code}' مستخدم لتصنيف آخر.");
        }

        if (!await _repositoryManager.ProductCategoryRepository.IsNameUniqueAsync(request.Name, request.Id))
        {
            throw new InvalidOperationException($"اسم التصنيف '{request.Name}' مستخدم لتصنيف آخر.");
        }

        _mapper.Map(request, category);
        category.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductCategoryRepository.Update(category);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<ProductCategoryDto>(category);
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var category = await _repositoryManager.ProductCategoryRepository.GetByIdAsync(id, trackChanges: true);
        if (category == null) return false;

        category.IsActive = !category.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductCategoryRepository.Update(category);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _repositoryManager.ProductCategoryRepository.GetCategoryWithProductsAsync(id);
        if (category == null) return false;

        if (category.Products != null && category.Products.Any())
        {
            throw new InvalidOperationException("لا يمكن حذف هذا التصنيف لوجود منتجات مرتبطة به. يمكنك تعطيله بدلاً من حذفه.");
        }

        _repositoryManager.ProductCategoryRepository.Remove(category);
        await _repositoryManager.SaveAsync();
        return true;
    }
}
