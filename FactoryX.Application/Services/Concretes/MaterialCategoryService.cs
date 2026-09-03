using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;

namespace FactoryX.Application.Services.Concretes;

public class MaterialCategoryService : IMaterialCategoryService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateMaterialCategoryRequest> _createValidator;
    private readonly IValidator<UpdateMaterialCategoryRequest> _updateValidator;

    public MaterialCategoryService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IValidator<CreateMaterialCategoryRequest> createValidator,
        IValidator<UpdateMaterialCategoryRequest> updateValidator)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<MaterialCategoryDto>> GetAllCategoriesAsync(bool trackChanges = false)
    {
        var categories = await _repositoryManager.MaterialCategoryRepository.GetAllWithMaterialsAsync(trackChanges);
        return _mapper.Map<IEnumerable<MaterialCategoryDto>>(categories);
    }

    public async Task<MaterialCategoryDto?> GetCategoryByIdAsync(int id, bool trackChanges = false)
    {
        var category = await _repositoryManager.MaterialCategoryRepository.GetByIdWithMaterialsAsync(id, trackChanges);
        return category != null ? _mapper.Map<MaterialCategoryDto>(category) : null;
    }

    public async Task<MaterialCategoryDto> CreateCategoryAsync(CreateMaterialCategoryRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _repositoryManager.MaterialCategoryRepository.ExistsByCodeAsync(request.Code))
            throw new InvalidOperationException($"رمز التصنيف '{request.Code}' مستخدم بالفعل.");

        if (await _repositoryManager.MaterialCategoryRepository.ExistsByNameAsync(request.Name))
            throw new InvalidOperationException($"اسم التصنيف '{request.Name}' مستخدم بالفعل.");

        var category = _mapper.Map<MaterialCategory>(request);
        category.IsActive = true;
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.MaterialCategoryRepository.Create(category);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<MaterialCategoryDto>(category);
    }

    public async Task<MaterialCategoryDto> UpdateCategoryAsync(UpdateMaterialCategoryRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var category = await _repositoryManager.MaterialCategoryRepository.GetByIdWithMaterialsAsync(request.Id, trackChanges: true);
        if (category == null)
            throw new KeyNotFoundException($"التصنيف بالمعرف {request.Id} غير موجود.");

        if (await _repositoryManager.MaterialCategoryRepository.ExistsByCodeAsync(request.Code, request.Id))
            throw new InvalidOperationException($"رمز التصنيف '{request.Code}' مستخدم بالفعل في تصنيف آخر.");

        if (await _repositoryManager.MaterialCategoryRepository.ExistsByNameAsync(request.Name, request.Id))
            throw new InvalidOperationException($"اسم التصنيف '{request.Name}' مستخدم بالفعل في تصنيف آخر.");

        category.Code = request.Code.Trim();
        category.Name = request.Name.Trim();
        category.Description = request.Description?.Trim() ?? string.Empty;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.MaterialCategoryRepository.Update(category);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<MaterialCategoryDto>(category);
    }

    public async Task<bool> ToggleCategoryStatusAsync(int id)
    {
        var category = await _repositoryManager.MaterialCategoryRepository.GetByIdWithMaterialsAsync(id, trackChanges: true);
        if (category == null) return false;

        category.IsActive = !category.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.MaterialCategoryRepository.Update(category);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _repositoryManager.MaterialCategoryRepository.GetByIdWithMaterialsAsync(id, trackChanges: true);
        if (category == null) return false;

        if (category.Materials != null && category.Materials.Any())
        {
            throw new InvalidOperationException("لا يمكن حذف التصنيف لوجود خامات ومواد مرتبطة به. يمكنك تعطيله بدلاً من حذفه.");
        }

        _repositoryManager.MaterialCategoryRepository.Remove(category);
        await _repositoryManager.SaveAsync();
        return true;
    }
}
