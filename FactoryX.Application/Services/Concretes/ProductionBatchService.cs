using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;

namespace FactoryX.Application.Services.Concretes;

public class ProductionBatchService : IProductionBatchService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateProductionBatchRequest>? _validator;

    public ProductionBatchService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IValidator<CreateProductionBatchRequest>? validator = null)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<IEnumerable<ProductionBatchDto>> GetBatchesAsync(ProductionBatchFilterRequest? filter = null)
    {
        var batches = await _repositoryManager.ProductionBatchRepository.GetFilteredBatchesAsync(
            filter?.Search,
            filter?.WorkOrderId,
            filter?.ProductId,
            filter?.Status,
            filter?.FromDate,
            filter?.ToDate,
            trackChanges: false);

        var result = new List<ProductionBatchDto>();
        foreach (var batch in batches)
        {
            var dto = _mapper.Map<ProductionBatchDto>(batch);
            result.Add(dto);
        }

        return result;
    }

    public async Task<ProductionBatchDto?> GetBatchByIdAsync(int id)
    {
        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(id, trackChanges: false);
        if (batch == null) return null;

        var dto = _mapper.Map<ProductionBatchDto>(batch);
        return dto;
    }

    public async Task<ProductionBatchDto> CreateBatchAsync(CreateProductionBatchRequest request)
    {
        if (_validator != null)
        {
            var valResult = await _validator.ValidateAsync(request);
            if (!valResult.IsValid)
            {
                throw new ValidationException(valResult.Errors);
            }
        }

        // Validate Production Order (WorkOrder)
        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(request.WorkOrderId, trackChanges: false);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر الإنتاج بالمعرف #{request.WorkOrderId} غير موجود في النظام.");
        }

        if (order.OrderStatus == ProductionOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("لا يمكن إنشاء دفعة إنتاج لأمر إنتاج ملغي.");
        }

        if (!order.RecipeVersionId.HasValue || order.RecipeVersionId.Value <= 0)
        {
            throw new InvalidOperationException("أمر الإنتاج لا يحتوي على إصدار وصفة معتمد (Recipe Version). يرجى تعيين الوصفة أولاً.");
        }

        // Check/Generate unique batch number
        if (string.IsNullOrWhiteSpace(request.BatchNumber))
        {
            request.BatchNumber = await GenerateBatchNumberAsync();
        }
        else if (!await _repositoryManager.ProductionBatchRepository.IsBatchNumberUniqueAsync(request.BatchNumber))
        {
            throw new InvalidOperationException($"رقم الدفعة '{request.BatchNumber}' مستخدم بالفعل في النظام. يرجى اختيار رقم فريد.");
        }

        var batch = new ProductionBatch
        {
            BatchNumber = request.BatchNumber.Trim(),
            WorkOrderId = order.Id,
            ProductId = order.ProductId,
            RecipeVersionId = order.RecipeVersionId,
            PlannedQuantity = request.PlannedQuantity > 0 ? request.PlannedQuantity : order.PlannedQuantity,
            ActualOutputQuantity = 0m,
            OutputUnit = string.IsNullOrWhiteSpace(request.OutputUnit) ? order.OutputUnit : request.OutputUnit.Trim(),
            ProductionDate = request.ProductionDate.Date,
            ExpiryDate = request.ExpiryDate?.Date,
            Status = ProductionBatchStatus.Planned,
            QualityStatus = "Pending",
            ProductionLineId = request.ProductionLineId > 0 ? request.ProductionLineId : order.ProductionLineId,
            WorkCenterId = request.WorkCenterId > 0 ? request.WorkCenterId : order.WorkCenterId,
            MachineId = request.MachineId > 0 ? request.MachineId : order.MachineId,
            OperatorId = request.OperatorId > 0 ? request.OperatorId : order.OperatorId,
            ShiftId = request.ShiftId > 0 ? request.ShiftId : order.ShiftId,
            TargetWarehouseId = request.TargetWarehouseId > 0 ? request.TargetWarehouseId : null,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryManager.ProductionBatchRepository.Create(batch);
        await _repositoryManager.SaveAsync();

        return (await GetBatchByIdAsync(batch.Id))!;
    }

    public async Task<ProductionBatchSummaryDto> GetSummaryAsync()
    {
        var all = (await _repositoryManager.ProductionBatchRepository.GetAllAsync()).ToList();

        var total = all.Count;
        var planned = all.Count(b => b.Status == ProductionBatchStatus.Planned);
        var inProgress = all.Count(b => b.Status == ProductionBatchStatus.InProgress);
        var paused = all.Count(b => b.Status == ProductionBatchStatus.Paused);
        var completed = all.Count(b => b.Status == ProductionBatchStatus.Completed);
        var cancelled = all.Count(b => b.Status == ProductionBatchStatus.Cancelled);
        var totalPlanned = all.Where(b => b.Status != ProductionBatchStatus.Cancelled).Sum(b => b.PlannedQuantity);
        var totalActual = all.Where(b => b.Status == ProductionBatchStatus.Completed).Sum(b => b.ActualOutputQuantity);

        return new ProductionBatchSummaryDto(total, planned, inProgress, paused, completed, cancelled, totalPlanned, totalActual);
    }

    public async Task<string> GenerateBatchNumberAsync()
    {
        var prefix = $"B-{DateTime.UtcNow:yyyyMMdd}-";
        var count = await _repositoryManager.ProductionBatchRepository.GetBatchCountForPrefixAsync(prefix);
        return $"{prefix}{(count + 1):D4}";
    }
}
