using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class ProductionExecutionService : IProductionExecutionService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IInventoryService _inventoryService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;
    private readonly IValidator<StartBatchRequest>? _startValidator;
    private readonly IValidator<CompleteBatchRequest>? _completeValidator;
    private readonly IValidator<CancelBatchRequest>? _cancelValidator;

    public ProductionExecutionService(
        IRepositoryManager repositoryManager,
        IInventoryService inventoryService,
        IMapper mapper,
        AppDbContext context,
        IValidator<StartBatchRequest>? startValidator = null,
        IValidator<CompleteBatchRequest>? completeValidator = null,
        IValidator<CancelBatchRequest>? cancelValidator = null)
    {
        _repositoryManager = repositoryManager;
        _inventoryService = inventoryService;
        _mapper = mapper;
        _context = context;
        _startValidator = startValidator;
        _completeValidator = completeValidator;
        _cancelValidator = cancelValidator;
    }

    public async Task<BatchExecutionDetailsDto> GetExecutionDetailsAsync(int batchId)
    {
        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(batchId, trackChanges: false);
        if (batch == null)
        {
            throw new KeyNotFoundException($"دفعة الإنتاج بالمعرف #{batchId} غير موجودة.");
        }

        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(batch.WorkOrderId, trackChanges: false);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر الإنتاج المرتبط بالدفعة غير موجود.");
        }

        var batchDto = _mapper.Map<ProductionBatchDto>(batch);
        var result = new BatchExecutionDetailsDto
        {
            Batch = batchDto,
            MaterialRequirements = new List<BatchMaterialRequirementItemDto>()
        };

        // 1. Get frozen BOM material requirements from the Production Order
        var frozenRequirements = order.MaterialRequirements?.ToList() ?? new List<WorkOrderMaterialRequirement>();

        // If for any reason order snapshot wasn't frozen yet, fallback to active recipe version items
        if (!frozenRequirements.Any() && order.RecipeVersion != null && order.RecipeVersion.Items != null)
        {
            var expectedOutput = order.RecipeVersion.ExpectedOutput > 0 ? order.RecipeVersion.ExpectedOutput : 100m;
            foreach (var item in order.RecipeVersion.Items)
            {
                var reqQty = Math.Round(item.Quantity * (batch.PlannedQuantity / expectedOutput), 4);
                frozenRequirements.Add(new WorkOrderMaterialRequirement
                {
                    MaterialId = item.MaterialId,
                    Material = item.Material,
                    MaterialCode = item.Material?.Code ?? string.Empty,
                    MaterialName = item.Material?.Name ?? string.Empty,
                    MaterialArabicName = item.Material?.ArabicName,
                    StockUnit = item.Unit ?? "KG",
                    RecipeQuantity = item.Quantity,
                    ExpectedOutputQuantity = expectedOutput,
                    PlannedProductionQuantity = batch.PlannedQuantity,
                    RequiredQuantity = reqQty
                });
            }
        }

        // 2. For each raw material, load available stock balances with FEFO ordering
        var allBalances = (await _repositoryManager.StockBalanceRepository.GetStockBalancesAsync(null, null, null, null, null))
            .Where(b => b.MaterialId.HasValue && b.Quantity > 0)
            .ToList();

        var warehouses = (await _repositoryManager.WarehouseRepository.GetAllAsync()).ToDictionary(w => w.Id, w => w);
        var locations = (await _repositoryManager.WarehouseLocationRepository.GetAllAsync()).ToDictionary(l => l.Id, l => l);

        foreach (var req in frozenRequirements)
        {
            // Scale requirement to this specific batch quantity
            var batchPlannedQty = req.ExpectedOutputQuantity > 0
                ? Math.Round(req.RecipeQuantity * (batch.PlannedQuantity / req.ExpectedOutputQuantity), 4)
                : req.RequiredQuantity;

            var matchingBalances = allBalances
                .Where(b => b.MaterialId == req.MaterialId)
                .OrderBy(b => b.ExpiryDate ?? DateTime.MaxValue) // FEFO Sort
                .ToList();

            var availableBatches = matchingBalances.Select(b => new AvailableMaterialBatchDto
            {
                WarehouseId = b.WarehouseId,
                WarehouseName = warehouses.TryGetValue(b.WarehouseId, out var wh) ? wh.Name : $"مستودع #{b.WarehouseId}",
                LocationId = b.LocationId,
                LocationName = (b.LocationId.HasValue && locations.TryGetValue(b.LocationId.Value, out var loc)) ? loc.Name : null,
                BatchNumber = b.BatchNumber,
                AvailableQuantity = b.Quantity,
                Unit = b.Unit,
                ExpiryDate = b.ExpiryDate
            }).ToList();

            var totalValidStock = availableBatches.Where(b => !b.IsExpired).Sum(b => b.AvailableQuantity);

            // Best default batch for FEFO (first non-expired with stock)
            var bestBatch = availableBatches.FirstOrDefault(b => !b.IsExpired && b.AvailableQuantity > 0);

            result.MaterialRequirements.Add(new BatchMaterialRequirementItemDto
            {
                MaterialId = req.MaterialId,
                MaterialCode = req.MaterialCode,
                MaterialName = req.MaterialName,
                MaterialArabicName = req.MaterialArabicName,
                Unit = req.StockUnit,
                RecipeQuantity = req.RecipeQuantity,
                PlannedQuantity = batchPlannedQty,
                TotalAvailableStock = totalValidStock,
                AvailableBatches = availableBatches,
                SelectedWarehouseId = bestBatch?.WarehouseId ?? 0,
                SelectedLocationId = bestBatch?.LocationId,
                SelectedBatchNumber = bestBatch?.BatchNumber ?? string.Empty,
                ConsumedQuantity = batchPlannedQty
            });
        }

        return result;
    }

    public async Task<ProductionBatchDto> StartBatchAsync(StartBatchRequest request, int userId)
    {
        if (_startValidator != null)
        {
            var valResult = await _startValidator.ValidateAsync(request);
            if (!valResult.IsValid)
            {
                throw new ValidationException(valResult.Errors);
            }
        }

        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(request.BatchId, trackChanges: true);
        if (batch == null)
        {
            throw new KeyNotFoundException($"دفعة الإنتاج بالمعرف #{request.BatchId} غير موجودة.");
        }

        if (batch.Status != ProductionBatchStatus.Planned && batch.Status != ProductionBatchStatus.Paused)
        {
            throw new InvalidOperationException($"لا يمكن بدء تشغيل الدفعة في حالتها الحالية '{batch.Status}'. التشغيل متاح فقط للدفعات المخططة (Planned) أو المتوقفة مؤقتاً (Paused).");
        }

        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(batch.WorkOrderId, trackChanges: true);
        if (order == null || order.OrderStatus == ProductionOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("أمر الإنتاج المرتبط بهذه الدفعة غير صالح أو تم إلغاؤه.");
        }

        // ATOMIC TRANSACTION: Consume materials and update batch status
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            batch.Consumptions ??= new List<ProductionConsumption>();

            foreach (var item in request.Consumptions)
            {
                var material = await _repositoryManager.MaterialRepository.GetByIdAsync(item.MaterialId);
                if (material == null)
                {
                    throw new InvalidOperationException($"المادة الخام بالمعرف #{item.MaterialId} غير موجودة.");
                }

                // 1. Centralized Inventory Deduction & InventoryTransaction creation
                var inventoryTx = await _inventoryService.ConsumeStockForProductionAsync(
                    warehouseId: item.WarehouseId,
                    locationId: item.LocationId,
                    materialId: item.MaterialId,
                    rawMaterialBatchNumber: item.RawMaterialBatchNumber,
                    quantity: item.ActualQuantity,
                    unit: material.Unit,
                    referenceDoc: batch.BatchNumber,
                    userId: userId,
                    notes: $"استهلاك خامات لتشغيل دفعة الإنتاج {batch.BatchNumber} ({order.OrderNumber})");

                // 2. Find planned requirement from BOM snapshot for variance calculation
                var reqSnapshot = order.MaterialRequirements?.FirstOrDefault(r => r.MaterialId == item.MaterialId);
                decimal plannedQty = reqSnapshot != null && reqSnapshot.ExpectedOutputQuantity > 0
                    ? Math.Round(reqSnapshot.RecipeQuantity * (batch.PlannedQuantity / reqSnapshot.ExpectedOutputQuantity), 4)
                    : item.ActualQuantity;

                var variance = item.ActualQuantity - plannedQty;

                // 3. Record ProductionConsumption audit entry
                var consumption = new ProductionConsumption
                {
                    ProductionBatchId = batch.Id,
                    MaterialId = item.MaterialId,
                    WarehouseId = item.WarehouseId,
                    LocationId = item.LocationId,
                    RawMaterialBatchNumber = item.RawMaterialBatchNumber ?? string.Empty,
                    ExpiryDate = inventoryTx.TransactionDate,
                    PlannedQuantity = plannedQty,
                    ActualQuantity = item.ActualQuantity,
                    Variance = variance,
                    Unit = material.Unit,
                    UnitCost = material.StandardCost,
                    TotalCost = Math.Round(material.StandardCost * item.ActualQuantity, 2),
                    InventoryTransactionId = inventoryTx.Id,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                batch.Consumptions.Add(consumption);
            }

            // Update Batch Status & Timestamps
            batch.Status = ProductionBatchStatus.InProgress;
            if (!batch.StartTime.HasValue)
            {
                batch.StartTime = DateTime.UtcNow;
            }
            batch.PauseTime = null;
            batch.UpdatedAt = DateTime.UtcNow;

            // Update WorkOrder Status if needed
            if (order.OrderStatus == ProductionOrderStatus.Planned || order.OrderStatus == ProductionOrderStatus.Released)
            {
                order.OrderStatus = ProductionOrderStatus.InProgress;
                order.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.WorkOrderRepository.Update(order);
            }

            _repositoryManager.ProductionBatchRepository.Update(batch);
            await _repositoryManager.SaveAsync();

            await dbTransaction.CommitAsync();

            return (await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(batch.Id, trackChanges: false)).MapToDto(_mapper);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ProductionBatchDto> PauseBatchAsync(int batchId, string? reason, int userId)
    {
        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(batchId, trackChanges: true);
        if (batch == null)
        {
            throw new KeyNotFoundException($"دفعة الإنتاج بالمعرف #{batchId} غير موجودة.");
        }

        if (batch.Status != ProductionBatchStatus.InProgress)
        {
            throw new InvalidOperationException($"لا يمكن إيقاف الدفعة مؤقتاً إلا إذا كانت قيد التشغيل (In Progress). الحالة الحالية: '{batch.Status}'.");
        }

        batch.Status = ProductionBatchStatus.Paused;
        batch.PauseTime = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            batch.Notes = string.IsNullOrWhiteSpace(batch.Notes)
                ? $"[إيقاف مؤقت: {reason}]"
                : $"{batch.Notes} | [إيقاف مؤقت: {reason}]";
        }
        batch.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductionBatchRepository.Update(batch);
        await _repositoryManager.SaveAsync();

        return batch.MapToDto(_mapper);
    }

    public async Task<ProductionBatchDto> ResumeBatchAsync(int batchId, int userId)
    {
        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(batchId, trackChanges: true);
        if (batch == null)
        {
            throw new KeyNotFoundException($"دفعة الإنتاج بالمعرف #{batchId} غير موجودة.");
        }

        if (batch.Status != ProductionBatchStatus.Paused)
        {
            throw new InvalidOperationException($"لا يمكن استئناف الدفعة إلا إذا كانت متوقفة مؤقتاً (Paused). الحالة الحالية: '{batch.Status}'.");
        }

        batch.Status = ProductionBatchStatus.InProgress;
        batch.PauseTime = null;
        batch.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductionBatchRepository.Update(batch);
        await _repositoryManager.SaveAsync();

        return batch.MapToDto(_mapper);
    }

    public async Task<ProductionBatchDto> CompleteBatchAsync(CompleteBatchRequest request, int userId)
    {
        if (_completeValidator != null)
        {
            var valResult = await _completeValidator.ValidateAsync(request);
            if (!valResult.IsValid)
            {
                throw new ValidationException(valResult.Errors);
            }
        }

        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(request.BatchId, trackChanges: true);
        if (batch == null)
        {
            throw new KeyNotFoundException($"دفعة الإنتاج بالمعرف #{request.BatchId} غير موجودة.");
        }

        if (batch.Status != ProductionBatchStatus.InProgress && batch.Status != ProductionBatchStatus.Paused)
        {
            throw new InvalidOperationException($"لا يمكن إكمال الدفعة وهي في حالة '{batch.Status}'. الإكمال متاح فقط للدفعات قيد التشغيل.");
        }

        batch.ActualOutputQuantity = request.ActualOutputQuantity;
        batch.EndTime = DateTime.UtcNow;
        batch.Status = ProductionBatchStatus.Completed;
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            batch.Notes = string.IsNullOrWhiteSpace(batch.Notes)
                ? request.Notes
                : $"{batch.Notes} | {request.Notes}";
        }
        batch.UpdatedAt = DateTime.UtcNow;

        // Integration with ProductionRecord
        var firstOp = (await _repositoryManager.OperatorRepository.GetAllAsync()).FirstOrDefault();
        int? opId = batch.OperatorId.HasValue && batch.OperatorId.Value > 0
            ? batch.OperatorId.Value
            : (firstOp?.Id);

        if (opId.HasValue && opId.Value > 0)
        {
            var prodRecord = new ProductionRecord
            {
                WorkOrderId = batch.WorkOrderId,
                ProductionBatchId = batch.Id,
                OperatorId = opId.Value,
                QuantityProduced = (int)Math.Round(request.ActualOutputQuantity),
                Timestamp = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _repositoryManager.ProductionRecordRepository.Create(prodRecord);
        }

        // Update WorkOrder Actual Quantity
        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(batch.WorkOrderId, trackChanges: true);
        if (order != null)
        {
            order.ActualQuantityDecimal += request.ActualOutputQuantity;
            order.UpdatedAt = DateTime.UtcNow;
            _repositoryManager.WorkOrderRepository.Update(order);
        }

        _repositoryManager.ProductionBatchRepository.Update(batch);
        await _repositoryManager.SaveAsync();

        return batch.MapToDto(_mapper);
    }

    public async Task<ProductionBatchDto> CancelBatchAsync(CancelBatchRequest request, int userId)
    {
        if (_cancelValidator != null)
        {
            var valResult = await _cancelValidator.ValidateAsync(request);
            if (!valResult.IsValid)
            {
                throw new ValidationException(valResult.Errors);
            }
        }

        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(request.BatchId, trackChanges: true);
        if (batch == null)
        {
            throw new KeyNotFoundException($"دفعة الإنتاج بالمعرف #{request.BatchId} غير موجودة.");
        }

        if (batch.Status == ProductionBatchStatus.Completed)
        {
            throw new InvalidOperationException("لا يمكن إلغاء دفعة إنتاج مكتملة بالفعل.");
        }

        if (batch.Status == ProductionBatchStatus.Cancelled)
        {
            throw new InvalidOperationException("دفعة الإنتاج ملغاة بالفعل.");
        }

        batch.Status = ProductionBatchStatus.Cancelled;
        batch.CancellationReason = request.CancellationReason;
        batch.Notes = string.IsNullOrWhiteSpace(batch.Notes)
            ? $"[تم الإلغاء: {request.CancellationReason}]"
            : $"{batch.Notes} | [تم الإلغاء: {request.CancellationReason}]";
        batch.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.ProductionBatchRepository.Update(batch);
        await _repositoryManager.SaveAsync();

        return batch.MapToDto(_mapper);
    }
}

public static class ProductionBatchExtensions
{
    public static ProductionBatchDto MapToDto(this ProductionBatch? batch, IMapper mapper)
    {
        if (batch == null) return null!;
        return mapper.Map<ProductionBatchDto>(batch);
    }
}
