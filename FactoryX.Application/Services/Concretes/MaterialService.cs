using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;

namespace FactoryX.Application.Services.Concretes;

public class MaterialService : IMaterialService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateMaterialRequest> _createValidator;
    private readonly IValidator<UpdateMaterialRequest> _updateValidator;

    public MaterialService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IValidator<CreateMaterialRequest> createValidator,
        IValidator<UpdateMaterialRequest> updateValidator)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync(MaterialFilterRequest? filter = null, bool trackChanges = false)
    {
        var materials = await _repositoryManager.MaterialRepository.GetAllWithDetailsAsync(trackChanges);
        var dtos = _mapper.Map<IEnumerable<MaterialDto>>(materials);

        if (filter == null)
            return dtos;

        var query = dtos.AsQueryable();

        // Search by Code, SKU, Name, or ArabicName
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim().ToLower();
            query = query.Where(m =>
                (m.Code != null && m.Code.ToLower().Contains(search)) ||
                (m.SKU != null && m.SKU.ToLower().Contains(search)) ||
                (m.Name != null && m.Name.ToLower().Contains(search)) ||
                (m.ArabicName != null && m.ArabicName.ToLower().Contains(search)));
        }

        // Filter by Category
        if (filter.CategoryId.HasValue && filter.CategoryId.Value > 0)
        {
            query = query.Where(m => m.MaterialCategoryId == filter.CategoryId.Value);
        }

        // Filter by Active Status
        if (filter.IsActive.HasValue)
        {
            query = query.Where(m => m.IsActive == filter.IsActive.Value);
        }

        // Filter by Stock Status
        if (filter.StockStatus.HasValue)
        {
            query = query.Where(m => m.StockStatus == filter.StockStatus.Value);
        }

        // Filter by Expiry Status
        if (!string.IsNullOrWhiteSpace(filter.ExpiryStatus))
        {
            var expStatus = filter.ExpiryStatus.Trim().ToLower();
            if (expStatus == "expired")
            {
                query = query.Where(m => m.IsExpired);
            }
            else if (expStatus == "expiring_soon")
            {
                query = query.Where(m => m.IsExpiringSoon && !m.IsExpired);
            }
            else if (expStatus == "valid")
            {
                query = query.Where(m => !m.IsExpired && !m.IsExpiringSoon);
            }
        }

        return query.ToList();
    }

    public async Task<IEnumerable<MaterialDto>> GetActiveMaterialsAsync()
    {
        return await GetAllMaterialsAsync(new MaterialFilterRequest(null, null, true, null, null));
    }

    public async Task<MaterialDto?> GetMaterialByIdAsync(int id, bool trackChanges = false)
    {
        var material = await _repositoryManager.MaterialRepository.GetByIdWithDetailsAsync(id, trackChanges);
        return material != null ? _mapper.Map<MaterialDto>(material) : null;
    }

    public async Task<MaterialDto> CreateMaterialAsync(CreateMaterialRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        if (await _repositoryManager.MaterialRepository.ExistsByCodeAsync(request.Code))
            throw new InvalidOperationException($"كود الخامة '{request.Code}' مستخدم بالفعل.");

        if (await _repositoryManager.MaterialRepository.ExistsBySKUAsync(request.SKU))
            throw new InvalidOperationException($"رمز SKU '{request.SKU}' مستخدم بالفعل.");

        if (request.WarehouseId.HasValue && request.WarehouseId.Value > 0)
        {
            var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.WarehouseId.Value);
            if (warehouse == null)
                throw new InvalidOperationException("المستودع المحدد للخامة غير موجود.");
        }

        var material = _mapper.Map<Material>(request);
        material.CurrentStock = 0; // Material creation itself does NOT create fake stock
        material.UnitCost = request.CurrentCost;
        material.IsActive = true;
        material.CreatedAt = DateTime.UtcNow;
        material.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.MaterialRepository.Create(material);
        await _repositoryManager.SaveAsync();

        return (await GetMaterialByIdAsync(material.Id))!;
    }

    public async Task<MaterialDto> UpdateMaterialAsync(UpdateMaterialRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            throw new ValidationException(validationResult.Errors);

        var material = await _repositoryManager.MaterialRepository.GetByIdWithDetailsAsync(request.Id, trackChanges: true);
        if (material == null)
            throw new KeyNotFoundException($"الخامة بالمعرف {request.Id} غير موجودة.");

        if (await _repositoryManager.MaterialRepository.ExistsByCodeAsync(request.Code, request.Id))
            throw new InvalidOperationException($"كود الخامة '{request.Code}' مستخدم بالفعل في صنف آخر.");

        if (await _repositoryManager.MaterialRepository.ExistsBySKUAsync(request.SKU, request.Id))
            throw new InvalidOperationException($"رمز SKU '{request.SKU}' مستخدم بالفعل في صنف آخر.");

        if (request.WarehouseId.HasValue && request.WarehouseId.Value > 0)
        {
            var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.WarehouseId.Value);
            if (warehouse == null)
                throw new InvalidOperationException("المستودع المحدد للخامة غير موجود.");
        }

        material.Code = request.Code.Trim();
        material.SKU = request.SKU.Trim();
        material.Name = request.Name.Trim();
        material.ArabicName = request.ArabicName?.Trim() ?? string.Empty;
        material.Description = request.Description?.Trim() ?? string.Empty;
        material.MaterialCategoryId = request.MaterialCategoryId;
        material.Unit = request.Unit.Trim();
        material.PurchaseUnit = request.PurchaseUnit?.Trim() ?? string.Empty;
        material.ConversionFactor = request.ConversionFactor > 0 ? request.ConversionFactor : 1.0m;

        material.MinimumStock = request.MinimumStock;
        material.ReorderLevel = request.ReorderLevel;
        material.MaximumStock = request.MaximumStock;

        material.StandardCost = request.StandardCost;
        material.CurrentCost = request.CurrentCost;
        material.LastPurchaseCost = request.LastPurchaseCost;
        material.UnitCost = request.CurrentCost;

        material.WarehouseId = request.WarehouseId;
        material.BatchNumber = request.BatchNumber;
        material.ManufacturingDate = request.ManufacturingDate;
        material.ExpiryDate = request.ExpiryDate;
        material.IsActive = request.IsActive;
        material.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.MaterialRepository.Update(material);
        await _repositoryManager.SaveAsync();

        return (await GetMaterialByIdAsync(material.Id))!;
    }

    public async Task<bool> ToggleMaterialStatusAsync(int id)
    {
        var material = await _repositoryManager.MaterialRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (material == null) return false;

        material.IsActive = !material.IsActive;
        material.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.MaterialRepository.Update(material);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<bool> DeactivateMaterialAsync(int id)
    {
        var material = await _repositoryManager.MaterialRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (material == null) return false;

        material.IsActive = false;
        material.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.MaterialRepository.Update(material);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteMaterialAsync(int id)
    {
        var material = await _repositoryManager.MaterialRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (material == null) return false;

        // Check if material has stock or transactions
        var stockBalances = await _repositoryManager.StockBalanceRepository.GetStockBalancesAsync(null, null, id, null, null);
        if (stockBalances.Any(s => s.Quantity > 0))
        {
            throw new InvalidOperationException("لا يمكن حذف هذه المادة لوجود رصيد مخزني حالي لها. يفضل تعطيل المادة (إلغاء التنشيط) بدلاً من حذفها.");
        }

        var transactions = await _repositoryManager.InventoryTransactionRepository.GetFilteredTransactionsAsync(null, id, null, null, null, null);
        if (transactions.Any())
        {
            throw new InvalidOperationException("لا يمكن حذف هذه المادة لوجود حركات وسجلات مخزنية سابقة مرتبطة بها. تم تعطيل المادة للحفاظ على سلامة البيانات المالية والمخزنية.");
        }

        _repositoryManager.MaterialRepository.Remove(material);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<IEnumerable<MaterialDto>> GetLowStockMaterialsAsync()
    {
        var materials = await _repositoryManager.MaterialRepository.GetLowStockMaterialsAsync();
        return _mapper.Map<IEnumerable<MaterialDto>>(materials);
    }

    public async Task<IEnumerable<MaterialDto>> GetMaterialsBelowReorderLevelAsync()
    {
        var materials = await _repositoryManager.MaterialRepository.GetMaterialsBelowReorderLevelAsync();
        return _mapper.Map<IEnumerable<MaterialDto>>(materials);
    }

    public async Task<IEnumerable<MaterialDto>> GetExpiredMaterialsAsync()
    {
        var materials = await _repositoryManager.MaterialRepository.GetExpiredMaterialsAsync();
        return _mapper.Map<IEnumerable<MaterialDto>>(materials);
    }

    public async Task<IEnumerable<MaterialDto>> GetMaterialsExpiringSoonAsync(int days = 30)
    {
        var materials = await _repositoryManager.MaterialRepository.GetMaterialsExpiringSoonAsync(days);
        return _mapper.Map<IEnumerable<MaterialDto>>(materials);
    }

    public async Task<MaterialStockSummaryDto> GetStockSummaryAsync()
    {
        var all = (await _repositoryManager.MaterialRepository.GetAllWithDetailsAsync()).ToList();
        var dtos = _mapper.Map<List<MaterialDto>>(all);

        return new MaterialStockSummaryDto(
            TotalMaterials: dtos.Count,
            ActiveMaterials: dtos.Count(m => m.IsActive),
            OutOfStockCount: dtos.Count(m => m.IsActive && m.StockStatus == MaterialStockStatus.OUT_OF_STOCK),
            LowStockCount: dtos.Count(m => m.IsActive && m.StockStatus == MaterialStockStatus.LOW_STOCK),
            ReorderRequiredCount: dtos.Count(m => m.IsActive && m.StockStatus == MaterialStockStatus.REORDER_REQUIRED),
            ExpiredCount: dtos.Count(m => m.IsActive && m.IsExpired),
            ExpiringSoonCount: dtos.Count(m => m.IsActive && m.IsExpiringSoon && !m.IsExpired)
        );
    }

    public decimal ConvertPurchaseToStockQuantity(decimal purchaseQuantity, decimal conversionFactor)
    {
        if (conversionFactor <= 0) conversionFactor = 1.0m;
        return purchaseQuantity * conversionFactor;
    }

    public async Task<IEnumerable<StockBalanceDto>> GetMaterialStockBalancesAsync(int materialId)
    {
        var balances = await _repositoryManager.StockBalanceRepository.GetStockBalancesAsync(null, null, materialId, null, null);
        return _mapper.Map<IEnumerable<StockBalanceDto>>(balances);
    }

    public async Task<IEnumerable<InventoryTransactionDto>> GetMaterialRecentTransactionsAsync(int materialId, int take = 10)
    {
        var transactions = await _repositoryManager.InventoryTransactionRepository.GetFilteredTransactionsAsync(null, materialId, null, null, null, null);
        var recent = transactions.OrderByDescending(t => t.TransactionDate).Take(take);
        return _mapper.Map<IEnumerable<InventoryTransactionDto>>(recent);
    }
}
