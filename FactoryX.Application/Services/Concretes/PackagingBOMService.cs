using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class PackagingBOMService : IPackagingBOMService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IPackagingCostService _costService;

    public PackagingBOMService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IPackagingCostService costService)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _costService = costService;
    }

    public async Task<IEnumerable<PackagingBOMDto>> GetAllBOMsAsync(bool onlyActive = false, int? productId = null)
    {
        var boms = await _repositoryManager.PackagingBOMRepository.GetAllWithDetailsAsync(onlyActive, productId);
        var dtos = new List<PackagingBOMDto>();

        foreach (var bom in boms)
        {
            var dto = MapToDto(bom);
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<PackagingBOMDto> GetBOMByIdAsync(int id)
    {
        var bom = await _repositoryManager.PackagingBOMRepository.GetByIdWithDetailsAsync(id);
        if (bom == null)
        {
            throw new KeyNotFoundException($"مواصفة التعبئة والتغليف بالمعرف #{id} غير موجودة.");
        }

        return MapToDto(bom);
    }

    public async Task<PackagingBOMDto> CreateBOMAsync(CreatePackagingBOMRequest request, int? userId = null)
    {
        var isCodeUnique = await _repositoryManager.PackagingBOMRepository.IsCodeUniqueAsync(request.Code);
        if (!isCodeUnique)
        {
            throw new InvalidOperationException($"كود مواصفة التعبئة والتغليف '{request.Code}' مستخدم بالفعل.");
        }

        var product = await _repositoryManager.ProductRepository.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            throw new InvalidOperationException($"المنتج التام بالمعرف #{request.ProductId} غير موجود.");
        }

        // Validate packaging materials
        if (request.Items != null)
        {
            foreach (var item in request.Items)
            {
                var material = await _repositoryManager.MaterialRepository.GetByIdAsync(item.MaterialId);
                if (material == null || !material.IsActive)
                {
                    throw new InvalidOperationException($"مادة التعبئة بالمعرف #{item.MaterialId} غير موجودة أو معطلة.");
                }

                if (!IsPackagingMaterial(material))
                {
                    throw new InvalidOperationException($"المادة '{material.Name}' ليست مادة تعبئة وتغليف مصنفة. لا يمكن إدراج المواد الخام في مواصفة التعبئة.");
                }
            }
        }

        var bom = new PackagingBOM
        {
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name.Trim(),
            ProductId = request.ProductId,
            PackSize = request.PackSize,
            PackSizeKg = request.PackSizeKg > 0 ? request.PackSizeKg : request.PackSize,
            PackUnit = string.IsNullOrWhiteSpace(request.PackUnit) ? "Box" : request.PackUnit.Trim(),
            OutputProductQuantity = request.PackSizeKg > 0 ? request.PackSizeKg : request.PackSize,
            Unit = string.IsNullOrWhiteSpace(request.PackUnit) ? "Box" : request.PackUnit.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Create initial active version v1
        var version = new PackagingBOMVersion
        {
            VersionNumber = 1,
            VersionName = "الإصدار القياسي v1",
            EffectiveFrom = DateTime.UtcNow,
            Status = PackagingBOMStatus.Active,
            Notes = "تم إنشاء الإصدار تلقائياً عند اعتماد مواصفة التعبئة",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.Items != null && request.Items.Any())
        {
            int seq = 1;
            foreach (var itemReq in request.Items)
            {
                var item = new PackagingItem
                {
                    MaterialId = itemReq.MaterialId,
                    QuantityRequired = itemReq.QuantityRequired,
                    Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? "Pcs" : itemReq.Unit.Trim(),
                    Sequence = itemReq.Sequence > 0 ? itemReq.Sequence : seq++,
                    IsOptional = itemReq.IsOptional,
                    Notes = itemReq.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                version.Items.Add(item);
            }
        }

        bom.Versions.Add(version);

        _repositoryManager.PackagingBOMRepository.Create(bom);
        await _repositoryManager.SaveAsync();

        return await GetBOMByIdAsync(bom.Id);
    }

    public async Task<PackagingBOMDto> UpdateBOMAsync(UpdatePackagingBOMRequest request, int? userId = null)
    {
        var bom = await _repositoryManager.PackagingBOMRepository.GetByIdWithDetailsAsync(request.Id, trackChanges: true);
        if (bom == null)
        {
            throw new KeyNotFoundException($"مواصفة التعبئة والتغليف بالمعرف #{request.Id} غير موجودة.");
        }

        var isCodeUnique = await _repositoryManager.PackagingBOMRepository.IsCodeUniqueAsync(request.Code, request.Id);
        if (!isCodeUnique)
        {
            throw new InvalidOperationException($"كود مواصفة التعبئة والتغليف '{request.Code}' مستخدم بالفعل.");
        }

        bom.Code = request.Code.Trim().ToUpper();
        bom.Name = request.Name.Trim();
        bom.ProductId = request.ProductId;
        bom.PackSize = request.PackSize;
        bom.PackSizeKg = request.PackSizeKg > 0 ? request.PackSizeKg : request.PackSize;
        bom.PackUnit = string.IsNullOrWhiteSpace(request.PackUnit) ? "Box" : request.PackUnit.Trim();
        bom.Description = request.Description?.Trim() ?? string.Empty;
        bom.IsActive = request.IsActive;
        bom.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PackagingBOMRepository.Update(bom);
        await _repositoryManager.SaveAsync();

        return await GetBOMByIdAsync(bom.Id);
    }

    public async Task<bool> DeleteBOMAsync(int id)
    {
        var bom = await _repositoryManager.PackagingBOMRepository.GetByIdWithDetailsAsync(id, trackChanges: true);
        if (bom == null) return false;

        // Check if referenced in packaging orders
        var orders = await _repositoryManager.PackagingOrderRepository.GetAllWithDetailsAsync(bomId: id);
        if (orders.Any())
        {
            throw new InvalidOperationException("لا يمكن حذف مواصفة التعبئة والتغليف لأنها مرتبطة بأوامر تعبئة سابقة. يمكنك تعطيلها بدلاً من ذلك.");
        }

        _repositoryManager.PackagingBOMRepository.Remove(bom);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<PackagingBOMVersionDto> CreateVersionAsync(CreatePackagingBOMVersionRequest request, int? userId = null)
    {
        var bom = await _repositoryManager.PackagingBOMRepository.GetByIdWithDetailsAsync(request.PackagingBOMId, trackChanges: true);
        if (bom == null)
        {
            throw new KeyNotFoundException($"مواصفة التعبئة والتغليف بالمعرف #{request.PackagingBOMId} غير موجودة.");
        }

        // If activating, verify no overlapping active versions
        if (request.Status == PackagingBOMStatus.Active)
        {
            var hasOverlap = await _repositoryManager.PackagingBOMRepository.HasOverlappingActiveVersionAsync(
                request.PackagingBOMId, request.EffectiveFrom, request.EffectiveTo);
            if (hasOverlap)
            {
                throw new InvalidOperationException("يوجد بالفعل إصدار نشط لمواصفة التعبئة خلال هذه الفترة الزمنية. يجب تعطيل الإصدار السابق أولاً أو تعديل تواريخ السريان.");
            }
        }

        var maxVersionNumber = bom.Versions.Any() ? bom.Versions.Max(v => v.VersionNumber) : 0;
        var newVersion = new PackagingBOMVersion
        {
            PackagingBOMId = request.PackagingBOMId,
            VersionNumber = maxVersionNumber + 1,
            VersionName = request.VersionName.Trim(),
            EffectiveFrom = request.EffectiveFrom,
            EffectiveTo = request.EffectiveTo,
            Status = request.Status,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (request.Items != null && request.Items.Any())
        {
            int seq = 1;
            foreach (var itemReq in request.Items)
            {
                var material = await _repositoryManager.MaterialRepository.GetByIdAsync(itemReq.MaterialId);
                if (material == null || !material.IsActive)
                {
                    throw new InvalidOperationException($"مادة التعبئة بالمعرف #{itemReq.MaterialId} غير موجودة أو معطلة.");
                }

                if (!IsPackagingMaterial(material))
                {
                    throw new InvalidOperationException($"المادة '{material.Name}' ليست مادة تعبئة وتغليف مصنفة.");
                }

                newVersion.Items.Add(new PackagingItem
                {
                    MaterialId = itemReq.MaterialId,
                    QuantityRequired = itemReq.QuantityRequired,
                    Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? "Pcs" : itemReq.Unit.Trim(),
                    Sequence = itemReq.Sequence > 0 ? itemReq.Sequence : seq++,
                    IsOptional = itemReq.IsOptional,
                    Notes = itemReq.Notes?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
        }

        bom.Versions.Add(newVersion);
        await _repositoryManager.SaveAsync();

        var created = await _repositoryManager.PackagingBOMRepository.GetVersionWithItemsAsync(newVersion.Id);
        return MapVersionToDto(created!);
    }

    public async Task<PackagingBOMVersionDto> ActivateVersionAsync(int versionId, int? userId = null)
    {
        var version = await _repositoryManager.PackagingBOMRepository.GetVersionWithItemsAsync(versionId, trackChanges: true);
        if (version == null)
        {
            throw new KeyNotFoundException($"إصدار مواصفة التعبئة بالمعرف #{versionId} غير موجود.");
        }

        var hasOverlap = await _repositoryManager.PackagingBOMRepository.HasOverlappingActiveVersionAsync(
            version.PackagingBOMId, version.EffectiveFrom, version.EffectiveTo, version.Id);
        if (hasOverlap)
        {
            throw new InvalidOperationException("لا يمكن تفعيل هذا الإصدار لوجود إصدار نشط آخر متداخل في الفترة الزمنية.");
        }

        version.Status = PackagingBOMStatus.Active;
        version.UpdatedAt = DateTime.UtcNow;
        await _repositoryManager.SaveAsync();

        return MapVersionToDto(version);
    }

    public async Task<PackagingBOMVersionDto> DeactivateVersionAsync(int versionId, int? userId = null)
    {
        var version = await _repositoryManager.PackagingBOMRepository.GetVersionWithItemsAsync(versionId, trackChanges: true);
        if (version == null)
        {
            throw new KeyNotFoundException($"إصدار مواصفة التعبئة بالمعرف #{versionId} غير موجود.");
        }

        version.Status = PackagingBOMStatus.Inactive;
        version.UpdatedAt = DateTime.UtcNow;
        await _repositoryManager.SaveAsync();

        return MapVersionToDto(version);
    }

    public async Task<IEnumerable<MaterialDto>> GetAvailablePackagingMaterialsAsync()
    {
        var allMaterials = await _repositoryManager.MaterialRepository.GetAllWithDetailsAsync(trackChanges: false);
        var packagingMaterials = allMaterials.Where(m => m.IsActive && IsPackagingMaterial(m));
        return _mapper.Map<IEnumerable<MaterialDto>>(packagingMaterials);
    }

    private static bool IsPackagingMaterial(Material m)
    {
        if (m.IsPackagingMaterial) return true;
        if (m.PackagingType != PackagingMaterialType.None) return true;
        if (m.MaterialCategory != null && m.MaterialCategory.CategoryType == MaterialCategoryType.PackagingMaterial) return true;
        if (m.MaterialCategory != null && (m.MaterialCategory.Name.Contains("تعبئة") || m.MaterialCategory.Name.Contains("تغليف") || m.MaterialCategory.Name.ToLower().Contains("packaging") || m.MaterialCategory.Name.ToLower().Contains("pack"))) return true;
        if (m.Name.Contains("كرتون") || m.Name.Contains("علبة") || m.Name.Contains("كيس") || m.Name.Contains("استيكر") || m.Name.Contains("ملصق") || m.Name.Contains("شريط") || m.Name.ToLower().Contains("box") || m.Name.ToLower().Contains("bag") || m.Name.ToLower().Contains("label") || m.Name.ToLower().Contains("sticker") || m.Name.ToLower().Contains("carton")) return true;
        return false;
    }

    private PackagingBOMDto MapToDto(PackagingBOM bom)
    {
        var dto = new PackagingBOMDto
        {
            Id = bom.Id,
            Code = bom.Code,
            Name = bom.Name,
            ProductId = bom.ProductId,
            ProductName = bom.Product?.Name ?? $"Product #{bom.ProductId}",
            ProductCode = bom.Product?.Code,
            PackSize = bom.PackSize,
            PackSizeKg = bom.PackSizeKg,
            PackUnit = bom.PackUnit,
            Description = bom.Description,
            IsActive = bom.IsActive
        };

        if (bom.Versions != null && bom.Versions.Any())
        {
            foreach (var v in bom.Versions.OrderByDescending(v => v.VersionNumber))
            {
                var vDto = MapVersionToDto(v);
                dto.Versions.Add(vDto);
            }

            var activeVersion = dto.Versions.FirstOrDefault(v => v.Status == PackagingBOMStatus.Active) ?? dto.Versions.First();
            dto.ActiveVersionNumber = activeVersion.VersionNumber;
            dto.TotalPackagingMaterialCost = activeVersion.PackagingCost;
            dto.CurrentItems = activeVersion.Items;
        }
        else if (bom.Items != null && bom.Items.Any())
        {
            decimal cost = 0m;
            foreach (var item in bom.Items.OrderBy(i => i.Sequence))
            {
                var itemDto = MapItemToDto(item);
                dto.CurrentItems.Add(itemDto);
                cost += itemDto.LineCost;
            }
            dto.TotalPackagingMaterialCost = Math.Round(cost, 4);
        }

        return dto;
    }

    private PackagingBOMVersionDto MapVersionToDto(PackagingBOMVersion version)
    {
        var dto = new PackagingBOMVersionDto
        {
            Id = version.Id,
            PackagingBOMId = version.PackagingBOMId,
            VersionNumber = version.VersionNumber,
            VersionName = version.VersionName,
            EffectiveFrom = version.EffectiveFrom,
            EffectiveTo = version.EffectiveTo,
            Status = version.Status,
            Notes = version.Notes
        };

        decimal cost = 0m;
        if (version.Items != null)
        {
            foreach (var item in version.Items.OrderBy(i => i.Sequence))
            {
                var itemDto = MapItemToDto(item);
                dto.Items.Add(itemDto);
                cost += itemDto.LineCost;
            }
        }
        dto.PackagingCost = Math.Round(cost, 4);

        return dto;
    }

    private static PackagingItemDto MapItemToDto(PackagingItem item)
    {
        var unitCost = item.Material != null ? (item.Material.CurrentCost > 0 ? item.Material.CurrentCost : (item.Material.StandardCost > 0 ? item.Material.StandardCost : item.Material.UnitCost)) : 0m;
        return new PackagingItemDto
        {
            Id = item.Id,
            PackagingBOMId = item.PackagingBOMId,
            PackagingBOMVersionId = item.PackagingBOMVersionId,
            MaterialId = item.MaterialId,
            MaterialName = item.Material?.Name ?? $"Material #{item.MaterialId}",
            MaterialCode = item.Material?.Code,
            MaterialArabicName = item.Material?.ArabicName,
            MaterialUnitCost = unitCost,
            QuantityRequired = item.QuantityRequired,
            Unit = item.Unit,
            Sequence = item.Sequence,
            IsOptional = item.IsOptional,
            Notes = item.Notes
        };
    }
}
