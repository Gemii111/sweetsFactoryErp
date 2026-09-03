using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class SupplierService : ISupplierService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public SupplierService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupplierDto>> GetAllSuppliersAsync(
        string? searchTerm = null,
        int? categoryId = null,
        bool? isActive = null)
    {
        var suppliers = await _repositoryManager.SupplierRepository.GetAllSuppliersAsync(searchTerm, categoryId, isActive);
        return _mapper.Map<IEnumerable<SupplierDto>>(suppliers);
    }

    public async Task<SupplierDto?> GetSupplierByIdAsync(int id)
    {
        var supplier = await _repositoryManager.SupplierRepository.GetByIdWithDetailsAsync(id);
        return _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<SupplierDto> CreateSupplierAsync(CreateSupplierRequest request)
    {
        var isUnique = await _repositoryManager.SupplierRepository.IsCodeUniqueAsync(request.Code.Trim());
        if (!isUnique)
        {
            throw new InvalidOperationException($"كود المورد [{request.Code}] مسجل مسبقاً، يرجى استخدام كود فريد.");
        }

        var supplier = _mapper.Map<Supplier>(request);
        supplier.Code = supplier.Code.Trim().ToUpper();
        supplier.Name = supplier.Name.Trim();
        supplier.CreatedAt = DateTime.UtcNow;
        supplier.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.SupplierRepository.Create(supplier);
        await _repositoryManager.SaveAsync();

        return await GetSupplierByIdAsync(supplier.Id) ?? _mapper.Map<SupplierDto>(supplier);
    }

    public async Task<SupplierDto> UpdateSupplierAsync(UpdateSupplierRequest request)
    {
        var supplier = await _repositoryManager.SupplierRepository.GetByIdWithDetailsAsync(request.Id, trackChanges: true);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"المورد برقم #{request.Id} غير موجود.");
        }

        var isUnique = await _repositoryManager.SupplierRepository.IsCodeUniqueAsync(request.Code.Trim(), request.Id);
        if (!isUnique)
        {
            throw new InvalidOperationException($"كود المورد [{request.Code}] مسجل مسبقاً لمورد آخر.");
        }

        _mapper.Map(request, supplier);
        supplier.Code = supplier.Code.Trim().ToUpper();
        supplier.Name = supplier.Name.Trim();
        supplier.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.SupplierRepository.Update(supplier);
        await _repositoryManager.SaveAsync();

        return (await GetSupplierByIdAsync(supplier.Id))!;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var supplier = await _repositoryManager.SupplierRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"المورد برقم #{id} غير موجود.");
        }

        supplier.IsActive = !supplier.IsActive;
        supplier.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.SupplierRepository.Update(supplier);
        await _repositoryManager.SaveAsync();
        return supplier.IsActive;
    }

    public async Task<bool> DeleteSupplierAsync(int id)
    {
        var supplier = await _repositoryManager.SupplierRepository.GetByIdWithDetailsAsync(id);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"المورد برقم #{id} غير موجود.");
        }

        var hasHistory = await _repositoryManager.SupplierRepository.HasPurchasingHistoryAsync(id);
        if (hasHistory)
        {
            throw new InvalidOperationException($"لا يمكن حذف المورد [{supplier.Name}] لوجود حركات وسجلات شراء وفواتير مسجلة باسمه. يمكنك إلغاء تنشيطه بدلاً من ذلك.");
        }

        _repositoryManager.SupplierRepository.Remove(supplier);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<SupplierSummaryDto> GetSummaryAsync()
    {
        var suppliers = await _repositoryManager.SupplierRepository.GetAllSuppliersAsync();
        var categories = await _repositoryManager.SupplierCategoryRepository.GetAllCategoriesAsync();

        return new SupplierSummaryDto
        {
            TotalSuppliers = suppliers.Count(),
            ActiveSuppliers = suppliers.Count(s => s.IsActive),
            InactiveSuppliers = suppliers.Count(s => !s.IsActive),
            TotalCategories = categories.Count()
        };
    }

    // Categories
    public async Task<IEnumerable<SupplierCategoryDto>> GetAllCategoriesAsync(bool onlyActive = false)
    {
        var categories = await _repositoryManager.SupplierCategoryRepository.GetAllCategoriesAsync(onlyActive);
        return _mapper.Map<IEnumerable<SupplierCategoryDto>>(categories);
    }

    public async Task<SupplierCategoryDto?> GetCategoryByIdAsync(int id)
    {
        var category = await _repositoryManager.SupplierCategoryRepository.GetByIdAsync(id);
        return _mapper.Map<SupplierCategoryDto>(category);
    }

    public async Task<SupplierCategoryDto> CreateCategoryAsync(CreateSupplierCategoryRequest request)
    {
        var isUnique = await _repositoryManager.SupplierCategoryRepository.IsCodeUniqueAsync(request.Code.Trim());
        if (!isUnique)
        {
            throw new InvalidOperationException($"كود تصنيف الموردين [{request.Code}] مسجل مسبقاً.");
        }

        var category = _mapper.Map<SupplierCategory>(request);
        category.Code = category.Code.Trim().ToUpper();
        category.Name = category.Name.Trim();
        category.CreatedAt = DateTime.UtcNow;
        category.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.SupplierCategoryRepository.Create(category);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<SupplierCategoryDto>(category);
    }

    // Price History
    public async Task<IEnumerable<SupplierPriceHistoryDto>> GetPriceHistoryAsync(int? supplierId = null, int? materialId = null)
    {
        var histories = await _repositoryManager.SupplierPriceHistoryRepository.GetHistoryAsync(supplierId, materialId);
        return _mapper.Map<IEnumerable<SupplierPriceHistoryDto>>(histories);
    }
}
