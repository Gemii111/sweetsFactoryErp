using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class WasteService : IWasteService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IInventoryService _inventoryService;
    private readonly IAccountingPostingService _postingService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;
    private readonly IValidator<CreateWasteRequest> _createValidator;
    private readonly IValidator<UpdateWasteRequest> _updateValidator;
    private readonly IValidator<RejectWasteRequest> _rejectValidator;

    public WasteService(
        IRepositoryManager repositoryManager,
        IInventoryService inventoryService,
        IAccountingPostingService postingService,
        IMapper mapper,
        AppDbContext context,
        IValidator<CreateWasteRequest> createValidator,
        IValidator<UpdateWasteRequest> updateValidator,
        IValidator<RejectWasteRequest> rejectValidator)
    {
        _repositoryManager = repositoryManager;
        _inventoryService = inventoryService;
        _postingService = postingService;
        _mapper = mapper;
        _context = context;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _rejectValidator = rejectValidator;
    }

    public async Task<IEnumerable<WasteDto>> GetAllAsync(
        WasteType? wasteType = null,
        WasteStatus? status = null,
        int? batchId = null,
        int? productId = null,
        int? materialId = null,
        int? reasonId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var wastes = await _repositoryManager.WasteRepository.GetAllWastesWithDetailsAsync(
            wasteType, status, batchId, productId, materialId, reasonId, fromDate, toDate, searchTerm);

        return _mapper.Map<IEnumerable<WasteDto>>(wastes);
    }

    public async Task<WasteDto?> GetByIdAsync(int id)
    {
        var waste = await _repositoryManager.WasteRepository.GetWasteWithDetailsAsync(id);
        return waste == null ? null : _mapper.Map<WasteDto>(waste);
    }

    public async Task<WasteDto?> GetByNumberAsync(string wasteNumber)
    {
        var waste = await _repositoryManager.WasteRepository.GetWasteByNumberAsync(wasteNumber);
        return waste == null ? null : _mapper.Map<WasteDto>(waste);
    }

    public async Task<WasteSummaryDto> GetSummaryAsync()
    {
        var wastes = await _repositoryManager.WasteRepository.GetAllWastesWithDetailsAsync();
        var list = wastes.ToList();

        return new WasteSummaryDto
        {
            TotalWastesCount = list.Count,
            PendingApprovalsCount = list.Count(w => w.Status == WasteStatus.PendingApproval),
            ApprovedCount = list.Count(w => w.Status == WasteStatus.Approved),
            RejectedCount = list.Count(w => w.Status == WasteStatus.Rejected),
            TotalWasteCost = list.Where(w => w.Status == WasteStatus.Approved).Sum(w => w.TotalCost),
            RawMaterialWasteCost = list.Where(w => w.Status == WasteStatus.Approved && w.WasteType == WasteType.RawMaterialWaste).Sum(w => w.TotalCost),
            ProcessWasteCost = list.Where(w => w.Status == WasteStatus.Approved && w.WasteType == WasteType.ProductionProcessWaste).Sum(w => w.TotalCost),
            OutputRejectionCost = list.Where(w => w.Status == WasteStatus.Approved && w.WasteType == WasteType.OutputRejection).Sum(w => w.TotalCost)
        };
    }

    public async Task<string> GenerateWasteNumberAsync(DateTime date)
    {
        var prefix = $"W-{date:yyyyMMdd}-";
        var countToday = await _repositoryManager.WasteRepository.GetCountForDateAsync(date);
        var sequence = countToday + 1;

        var wasteNumber = $"{prefix}{sequence:D4}";
        while (!await _repositoryManager.WasteRepository.IsWasteNumberUniqueAsync(wasteNumber))
        {
            sequence++;
            wasteNumber = $"{prefix}{sequence:D4}";
        }

        return wasteNumber;
    }

    public async Task<decimal> EstimateUnitCostAsync(WasteType wasteType, int? materialId, int? productId, int? batchId)
    {
        if (materialId.HasValue && materialId.Value > 0)
        {
            var material = await _repositoryManager.MaterialRepository.GetByIdAsync(materialId.Value);
            if (material != null)
            {
                return material.CurrentCost > 0 ? material.CurrentCost : material.StandardCost;
            }
        }

        if (productId.HasValue && productId.Value > 0)
        {
            var product = await _repositoryManager.ProductRepository.GetByIdAsync(productId.Value);
            if (product != null)
            {
                return product.StandardCost;
            }
        }

        if (batchId.HasValue && batchId.Value > 0)
        {
            var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(batchId.Value);
            if (batch?.Product != null)
            {
                return batch.Product.StandardCost;
            }
        }

        return 0m;
    }

    public async Task<WasteDto> CreateAsync(CreateWasteRequest request, int userId)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Output Rejection Validation against Batch Actual Output
        if (request.WasteType == WasteType.OutputRejection && request.ProductionBatchId.HasValue)
        {
            var batch = await _repositoryManager.ProductionBatchRepository.GetByIdAsync(request.ProductionBatchId.Value);
            if (batch == null)
            {
                throw new InvalidOperationException($"دفعة الإنتاج برقم #{request.ProductionBatchId} غير موجودة.");
            }

            if (request.Quantity > batch.ActualOutputQuantity && batch.ActualOutputQuantity > 0)
            {
                throw new InvalidOperationException(
                    $"كمية المرفوضات ({request.Quantity:N2} {request.Unit}) لا يمكن أن تتجاوز كمية الإنتاج الفعلي للدفعة ({batch.ActualOutputQuantity:N2} {batch.OutputUnit}).");
            }

            if (request.ProductId == null || request.ProductId == 0)
            {
                request.ProductId = batch.ProductId;
            }
        }

        // Process Waste Linkage
        if (request.WasteType == WasteType.ProductionProcessWaste && request.ProductionBatchId.HasValue)
        {
            var batch = await _repositoryManager.ProductionBatchRepository.GetByIdAsync(request.ProductionBatchId.Value);
            if (batch == null)
            {
                throw new InvalidOperationException($"دفعة الإنتاج برقم #{request.ProductionBatchId} غير موجودة.");
            }

            if (request.ProductId == null || request.ProductId == 0)
            {
                request.ProductId = batch.ProductId;
            }
        }

        // Unit Cost Calculation
        if (request.UnitCost <= 0)
        {
            request.UnitCost = await EstimateUnitCostAsync(request.WasteType, request.MaterialId, request.ProductId, request.ProductionBatchId);
        }

        var totalCost = Math.Round(request.Quantity * request.UnitCost, 2);
        var wasteNumber = await GenerateWasteNumberAsync(request.WasteDate);

        var waste = new Waste
        {
            WasteNumber = wasteNumber,
            WasteType = request.WasteType,
            Status = request.SubmitDirectly ? WasteStatus.PendingApproval : WasteStatus.Draft,
            ProductionBatchId = request.ProductionBatchId,
            MaterialId = request.MaterialId,
            ProductId = request.ProductId,
            RawMaterialBatchNumber = request.RawMaterialBatchNumber?.Trim(),
            WarehouseId = request.WarehouseId,
            LocationId = request.LocationId,
            Quantity = request.Quantity,
            Unit = request.Unit,
            UnitCost = request.UnitCost,
            TotalCost = totalCost,
            WasteReasonId = request.WasteReasonId,
            ReasonDescription = request.ReasonDescription?.Trim() ?? string.Empty,
            WasteDate = request.WasteDate,
            Notes = request.Notes?.Trim(),
            CreatedByUserId = userId,
            ApprovalStatus = request.SubmitDirectly ? "Pending" : "Draft",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryManager.WasteRepository.Create(waste);
        await _repositoryManager.SaveAsync();

        return (await GetByIdAsync(waste.Id))!;
    }

    public async Task<WasteDto> UpdateAsync(UpdateWasteRequest request, int userId)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var waste = await _repositoryManager.WasteRepository.GetWasteWithDetailsAsync(request.Id, trackChanges: true);
        if (waste == null)
        {
            throw new KeyNotFoundException($"سجل الهالك برقم #{request.Id} غير موجود.");
        }

        if (waste.Status != WasteStatus.Draft)
        {
            throw new InvalidOperationException($"لا يمكن تعديل سجل الهالك وهو في حالة '{waste.Status}'. التعديل متاح فقط للمسودات.");
        }

        if (request.UnitCost <= 0)
        {
            request.UnitCost = await EstimateUnitCostAsync(request.WasteType, request.MaterialId, request.ProductId, request.ProductionBatchId);
        }

        waste.WasteType = request.WasteType;
        waste.ProductionBatchId = request.ProductionBatchId;
        waste.MaterialId = request.MaterialId;
        waste.ProductId = request.ProductId;
        waste.RawMaterialBatchNumber = request.RawMaterialBatchNumber?.Trim();
        waste.WarehouseId = request.WarehouseId;
        waste.LocationId = request.LocationId;
        waste.Quantity = request.Quantity;
        waste.Unit = request.Unit;
        waste.UnitCost = request.UnitCost;
        waste.TotalCost = Math.Round(request.Quantity * request.UnitCost, 2);
        waste.WasteReasonId = request.WasteReasonId;
        waste.ReasonDescription = request.ReasonDescription?.Trim() ?? string.Empty;
        waste.WasteDate = request.WasteDate;
        waste.Notes = request.Notes?.Trim();
        waste.UpdatedAt = DateTime.UtcNow;

        if (request.SubmitDirectly)
        {
            waste.Status = WasteStatus.PendingApproval;
            waste.ApprovalStatus = "Pending";
        }

        _repositoryManager.WasteRepository.Update(waste);
        await _repositoryManager.SaveAsync();

        return (await GetByIdAsync(waste.Id))!;
    }

    public async Task<WasteDto> SubmitForApprovalAsync(int id, int userId)
    {
        var waste = await _repositoryManager.WasteRepository.GetWasteWithDetailsAsync(id, trackChanges: true);
        if (waste == null)
        {
            throw new KeyNotFoundException($"سجل الهالك برقم #{id} غير موجود.");
        }

        if (waste.Status != WasteStatus.Draft)
        {
            throw new InvalidOperationException($"لا يمكن تقديم الهالك للاعتماد وهو في حالة '{waste.Status}'. التقديم متاح فقط لحالة المسودة.");
        }

        waste.Status = WasteStatus.PendingApproval;
        waste.ApprovalStatus = "Pending";
        waste.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WasteRepository.Update(waste);
        await _repositoryManager.SaveAsync();

        return (await GetByIdAsync(waste.Id))!;
    }

    public async Task<WasteDto> ApproveWasteAsync(ApproveWasteRequest request, int userId)
    {
        var waste = await _repositoryManager.WasteRepository.GetWasteWithDetailsAsync(request.WasteId, trackChanges: true);
        if (waste == null)
        {
            throw new KeyNotFoundException($"سجل الهالك برقم #{request.WasteId} غير موجود.");
        }

        if (waste.Status != WasteStatus.PendingApproval)
        {
            throw new InvalidOperationException($"لا يمكن اعتماد سجل الهالك وهو في حالة '{waste.Status}'. الاعتماد متاح فقط للسجلات المعلقة (Pending Approval).");
        }

        // Atomic Transaction Handling for Inventory Deduction
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            if (waste.WasteType == WasteType.RawMaterialWaste)
            {
                if (!waste.MaterialId.HasValue || !waste.WarehouseId.HasValue)
                {
                    throw new InvalidOperationException("هالك المواد الخام يتطلب تحديد المادة الخام والمستودع بدقة قبل الاعتماد.");
                }

                // Centralized Inventory Service Stock Deduction
                var tx = await _inventoryService.ConsumeStockForWasteAsync(
                    warehouseId: waste.WarehouseId.Value,
                    locationId: waste.LocationId,
                    materialId: waste.MaterialId.Value,
                    rawMaterialBatchNumber: waste.RawMaterialBatchNumber,
                    quantity: waste.Quantity,
                    unit: waste.Unit,
                    referenceWasteNumber: waste.WasteNumber,
                    userId: userId,
                    notes: $"إسقاط هالك مواد خام معتمد [{waste.WasteNumber}] - سبب: {waste.WasteReason?.Reason ?? waste.ReasonDescription}");

                waste.InventoryTransactionId = tx.Id;
            }

            waste.Status = WasteStatus.Approved;
            waste.ApprovalStatus = "Approved";
            waste.ApprovedByUserId = userId;
            waste.ApprovedAt = DateTime.UtcNow;
            waste.ApprovalNotes = request.ApprovalNotes?.Trim();
            waste.UpdatedAt = DateTime.UtcNow;

            _repositoryManager.WasteRepository.Update(waste);
            await _repositoryManager.SaveAsync();
            await dbTransaction.CommitAsync();

            // Automatic Accounting Posting: Dr Waste Expense, Cr Inventory
            await _postingService.PostWasteAsync(waste.Id, userId);

            return (await GetByIdAsync(waste.Id))!;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<WasteDto> RejectWasteAsync(RejectWasteRequest request, int userId)
    {
        var validationResult = await _rejectValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var waste = await _repositoryManager.WasteRepository.GetWasteWithDetailsAsync(request.WasteId, trackChanges: true);
        if (waste == null)
        {
            throw new KeyNotFoundException($"سجل الهالك برقم #{request.WasteId} غير موجود.");
        }

        if (waste.Status != WasteStatus.PendingApproval)
        {
            throw new InvalidOperationException($"لا يمكن رفض سجل الهالك وهو في حالة '{waste.Status}'. الرفض متاح فقط للسجلات المعلقة (Pending Approval).");
        }

        waste.Status = WasteStatus.Rejected;
        waste.ApprovalStatus = "Rejected";
        waste.ApprovedByUserId = userId;
        waste.ApprovedAt = DateTime.UtcNow;
        waste.ApprovalNotes = request.RejectionReason.Trim();
        waste.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WasteRepository.Update(waste);
        await _repositoryManager.SaveAsync();

        return (await GetByIdAsync(waste.Id))!;
    }

    public async Task<WasteDto> CancelWasteAsync(int id, int userId, string? reason = null)
    {
        var waste = await _repositoryManager.WasteRepository.GetWasteWithDetailsAsync(id, trackChanges: true);
        if (waste == null)
        {
            throw new KeyNotFoundException($"سجل الهالك برقم #{id} غير موجود.");
        }

        if (waste.Status == WasteStatus.Approved)
        {
            throw new InvalidOperationException("لا يمكن إلغاء سجل هالك معتمد تم إسقاطه من المخزون.");
        }

        if (waste.Status == WasteStatus.Cancelled)
        {
            throw new InvalidOperationException("سجل الهالك ملغى بالفعل.");
        }

        waste.Status = WasteStatus.Cancelled;
        waste.ApprovalStatus = "Cancelled";
        if (!string.IsNullOrWhiteSpace(reason))
        {
            waste.Notes = string.IsNullOrWhiteSpace(waste.Notes)
                ? $"سبب الإلغاء: {reason}"
                : $"{waste.Notes} | سبب الإلغاء: {reason}";
        }
        waste.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WasteRepository.Update(waste);
        await _repositoryManager.SaveAsync();

        return (await GetByIdAsync(waste.Id))!;
    }
}
