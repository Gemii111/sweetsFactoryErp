using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.DTOs.Requests.WorkOrderRequests;
using FactoryX.Application.DTOs.Responses.WorkOrder;
using FactoryX.Application.DTOs.Responses.WorkOrderResponses;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;

namespace FactoryX.Application.Services.Concretes;

public class WorkOrderService : IWorkOrderService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IProductionPlanningService _planningService;
    private readonly IValidator<CreateProductionOrderRequest>? _createValidator;
    private readonly IValidator<UpdateProductionOrderRequest>? _updateValidator;

    public WorkOrderService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IProductionPlanningService planningService,
        IValidator<CreateProductionOrderRequest>? createValidator = null,
        IValidator<UpdateProductionOrderRequest>? updateValidator = null)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _planningService = planningService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    #region Phase 7 Modern Production Orders

    public async Task<IEnumerable<ProductionOrderDto>> GetProductionOrdersAsync(ProductionOrderFilterRequest? filter = null)
    {
        var orders = await _repositoryManager.WorkOrderRepository.GetFilteredOrdersAsync(
            filter?.Search,
            filter?.ProductId,
            filter?.Status,
            filter?.Priority,
            filter?.FromDate,
            filter?.ToDate,
            trackChanges: false);

        var result = new List<ProductionOrderDto>();
        foreach (var order in orders)
        {
            var dto = _mapper.Map<ProductionOrderDto>(order);
            result.Add(dto);
        }

        return result;
    }

    public async Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id)
    {
        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(id, trackChanges: false);
        if (order == null) return null;

        var dto = _mapper.Map<ProductionOrderDto>(order);

        // Compute live material requirements comparison for display
        if (order.RecipeVersionId.HasValue && order.PlannedQuantity > 0)
        {
            dto.LiveMaterialRequirements = await _planningService.CalculateMaterialRequirementsAsync(
                order.RecipeVersionId.Value,
                order.PlannedQuantity);
        }

        // Also map persisted requirements with live current stock comparison
        if (order.MaterialRequirements != null && order.MaterialRequirements.Any())
        {
            var matIds = order.MaterialRequirements.Select(m => m.MaterialId).Distinct().ToList();
            var balances = (await _repositoryManager.StockBalanceRepository.GetStockBalancesAsync(null, null, null, null, null))
                .Where(b => b.MaterialId.HasValue && matIds.Contains(b.MaterialId.Value))
                .GroupBy(b => b.MaterialId!.Value)
                .ToDictionary(g => g.Key, g => g.Sum(b => b.Quantity));

            var materials = (await _repositoryManager.MaterialRepository.GetAllAsync())
                .Where(m => matIds.Contains(m.Id))
                .ToDictionary(m => m.Id, m => m);

            foreach (var reqDto in dto.MaterialRequirements)
            {
                if (balances.TryGetValue(reqDto.MaterialId, out var stockQty))
                {
                    reqDto.CurrentStock = stockQty;
                }
                else if (materials.TryGetValue(reqDto.MaterialId, out var matEntity))
                {
                    reqDto.CurrentStock = matEntity.CurrentStock;
                }
            }
        }

        return dto;
    }

    public async Task<ProductionOrderDto> CreateProductionOrderAsync(CreateProductionOrderRequest request)
    {
        if (_createValidator != null)
        {
            var valResult = await _createValidator.ValidateAsync(request);
            if (!valResult.IsValid)
            {
                throw new ValidationException(valResult.Errors);
            }
        }

        // Validate active recipe version and product alignment
        await _planningService.ValidateRecipeVersionForPlanningAsync(
            request.ProductId,
            request.RecipeVersionId,
            request.PlannedDate);

        // Generate or validate order number
        if (string.IsNullOrWhiteSpace(request.OrderNumber))
        {
            request.OrderNumber = await GenerateOrderNumberAsync();
        }
        else if (!await _repositoryManager.WorkOrderRepository.IsOrderNumberUniqueAsync(request.OrderNumber))
        {
            throw new InvalidOperationException($"رقم أمر الإنتاج '{request.OrderNumber}' مستخدم بالفعل في النظام. يرجى اختيار رقم فريد.");
        }

        var version = await _repositoryManager.RecipeVersionRepository.GetByIdAsync(request.RecipeVersionId);
        var order = new WorkOrder
        {
            OrderNumber = request.OrderNumber.Trim(),
            ProductId = request.ProductId,
            RecipeId = version?.RecipeId,
            RecipeVersionId = request.RecipeVersionId,
            PlannedQuantity = request.PlannedQuantity,
            OutputUnit = string.IsNullOrWhiteSpace(request.OutputUnit) ? (version?.OutputUnit ?? "KG") : request.OutputUnit.Trim(),
            PlannedDate = request.PlannedDate.Date,
            DueDate = request.DueDate?.Date,
            Priority = request.Priority,
            OrderStatus = request.InitialStatus == ProductionOrderStatus.Released ? ProductionOrderStatus.Released :
                          request.InitialStatus == ProductionOrderStatus.Planned ? ProductionOrderStatus.Planned :
                          ProductionOrderStatus.Draft,
            ProductionAreaId = request.ProductionAreaId > 0 ? request.ProductionAreaId : null,
            ProductionLineId = request.ProductionLineId > 0 ? request.ProductionLineId : null,
            WorkCenterId = request.WorkCenterId > 0 ? request.WorkCenterId : null,
            MachineId = request.MachineId > 0 ? request.MachineId : null,
            OperatorId = request.OperatorId > 0 ? request.OperatorId : null,
            ShiftId = request.ShiftId > 0 ? request.ShiftId : null,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // If created directly as Released, populate BOM material requirement snapshot
        if (order.OrderStatus == ProductionOrderStatus.Released)
        {
            var calculated = await _planningService.CalculateMaterialRequirementsAsync(request.RecipeVersionId, request.PlannedQuantity);
            order.MaterialRequirements = calculated.Select(c => new WorkOrderMaterialRequirement
            {
                MaterialId = c.MaterialId,
                MaterialCode = c.MaterialCode,
                MaterialName = c.MaterialName,
                MaterialArabicName = c.MaterialArabicName,
                StockUnit = c.StockUnit,
                RecipeQuantity = c.RecipeQuantity,
                ExpectedOutputQuantity = c.ExpectedOutputQuantity,
                PlannedProductionQuantity = c.PlannedProductionQuantity,
                RequiredQuantity = c.RequiredQuantity,
                AllocatedQuantity = 0m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();
        }

        _repositoryManager.WorkOrderRepository.Create(order);
        await _repositoryManager.SaveAsync();

        return (await GetProductionOrderByIdAsync(order.Id))!;
    }

    public async Task<ProductionOrderDto> UpdateProductionOrderAsync(UpdateProductionOrderRequest request)
    {
        if (_updateValidator != null)
        {
            var valResult = await _updateValidator.ValidateAsync(request);
            if (!valResult.IsValid)
            {
                throw new ValidationException(valResult.Errors);
            }
        }

        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(request.Id, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر الإنتاج بالمعرف {request.Id} غير موجود.");
        }

        if (order.OrderStatus != ProductionOrderStatus.Draft && order.OrderStatus != ProductionOrderStatus.Planned)
        {
            throw new InvalidOperationException($"لا يمكن تعديل أمر الإنتاج في حالة '{order.StatusName()}'. التعديل مسموح فقط للأوامر في حالة مسودة (Draft) أو مخطط (Planned).");
        }

        if (!await _repositoryManager.WorkOrderRepository.IsOrderNumberUniqueAsync(request.OrderNumber, request.Id))
        {
            throw new InvalidOperationException($"رقم أمر الإنتاج '{request.OrderNumber}' مستخدم بالفعل لأمر آخر.");
        }

        // Validate recipe version if changed
        await _planningService.ValidateRecipeVersionForPlanningAsync(
            request.ProductId,
            request.RecipeVersionId,
            request.PlannedDate);

        var version = await _repositoryManager.RecipeVersionRepository.GetByIdAsync(request.RecipeVersionId);

        order.OrderNumber = request.OrderNumber.Trim();
        order.ProductId = request.ProductId;
        order.RecipeId = version?.RecipeId;
        order.RecipeVersionId = request.RecipeVersionId;
        order.PlannedQuantity = request.PlannedQuantity;
        order.OutputUnit = string.IsNullOrWhiteSpace(request.OutputUnit) ? "KG" : request.OutputUnit.Trim();
        order.PlannedDate = request.PlannedDate.Date;
        order.DueDate = request.DueDate?.Date;
        order.Priority = request.Priority;
        order.ProductionAreaId = request.ProductionAreaId > 0 ? request.ProductionAreaId : null;
        order.ProductionLineId = request.ProductionLineId > 0 ? request.ProductionLineId : null;
        order.WorkCenterId = request.WorkCenterId > 0 ? request.WorkCenterId : null;
        order.MachineId = request.MachineId > 0 ? request.MachineId : null;
        order.OperatorId = request.OperatorId > 0 ? request.OperatorId : null;
        order.ShiftId = request.ShiftId > 0 ? request.ShiftId : null;
        order.Notes = request.Notes;
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WorkOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return (await GetProductionOrderByIdAsync(order.Id))!;
    }

    public async Task<ProductionOrderDto> ReleaseProductionOrderAsync(int id)
    {
        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(id, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر الإنتاج بالمعرف {id} غير موجود.");
        }

        if (order.OrderStatus != ProductionOrderStatus.Draft && order.OrderStatus != ProductionOrderStatus.Planned)
        {
            throw new InvalidOperationException($"لا يمكن إطلاق أمر الإنتاج في حالة '{order.OrderStatus}'. الإطلاق مسموح فقط للأوامر في حالة مسودة (Draft) أو مخطط (Planned).");
        }

        if (!order.RecipeVersionId.HasValue)
        {
            throw new InvalidOperationException("لا يمكن إطلاق أمر إنتاج بدون تحديد إصدار وصفة معتمد.");
        }

        // Revalidate active recipe version
        await _planningService.ValidateRecipeVersionForPlanningAsync(
            order.ProductId,
            order.RecipeVersionId.Value,
            order.PlannedDate);

        // Generate and freeze BOM material requirements snapshot
        var calculated = await _planningService.CalculateMaterialRequirementsAsync(
            order.RecipeVersionId.Value,
            order.PlannedQuantity);

        order.MaterialRequirements ??= new List<WorkOrderMaterialRequirement>();
        order.MaterialRequirements.Clear();

        foreach (var c in calculated)
        {
            order.MaterialRequirements.Add(new WorkOrderMaterialRequirement
            {
                WorkOrderId = order.Id,
                MaterialId = c.MaterialId,
                MaterialCode = c.MaterialCode,
                MaterialName = c.MaterialName,
                MaterialArabicName = c.MaterialArabicName,
                StockUnit = c.StockUnit,
                RecipeQuantity = c.RecipeQuantity,
                ExpectedOutputQuantity = c.ExpectedOutputQuantity,
                PlannedProductionQuantity = c.PlannedProductionQuantity,
                RequiredQuantity = c.RequiredQuantity,
                AllocatedQuantity = 0m,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        order.OrderStatus = ProductionOrderStatus.Released;
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WorkOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return (await GetProductionOrderByIdAsync(order.Id))!;
    }

    public async Task<ProductionOrderDto> StartProductionOrderAsync(int id)
    {
        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(id, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر الإنتاج بالمعرف {id} غير موجود.");
        }

        if (order.OrderStatus != ProductionOrderStatus.Released)
        {
            throw new InvalidOperationException($"لا يمكن بدء تشغيل أمر الإنتاج إلا بعد إطلاقه واعتماده (Released). الحالة الحالية: '{order.OrderStatus}'.");
        }

        order.OrderStatus = ProductionOrderStatus.InProgress;
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WorkOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return (await GetProductionOrderByIdAsync(order.Id))!;
    }

    public async Task<ProductionOrderDto> CompleteProductionOrderAsync(int id)
    {
        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(id, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر الإنتاج بالمعرف {id} غير موجود.");
        }

        if (order.OrderStatus != ProductionOrderStatus.InProgress && order.OrderStatus != ProductionOrderStatus.Released)
        {
            throw new InvalidOperationException($"لا يمكن إكمال أمر الإنتاج وهو في حالة '{order.OrderStatus}'.");
        }

        order.OrderStatus = ProductionOrderStatus.Completed;
        order.ActualCompletionDate = DateTime.UtcNow;
        if (order.ActualQuantityDecimal <= 0)
        {
            order.ActualQuantityDecimal = order.PlannedQuantity;
        }
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WorkOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return (await GetProductionOrderByIdAsync(order.Id))!;
    }

    public async Task<ProductionOrderDto> CancelProductionOrderAsync(int id, string? cancellationReason = null)
    {
        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(id, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر الإنتاج بالمعرف {id} غير موجود.");
        }

        if (order.OrderStatus == ProductionOrderStatus.Completed)
        {
            throw new InvalidOperationException("لا يمكن إلغاء أمر إنتاج مكتمل بالفعل.");
        }

        if (order.OrderStatus == ProductionOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("أمر الإنتاج ملغي بالفعل.");
        }

        order.OrderStatus = ProductionOrderStatus.Cancelled;
        if (!string.IsNullOrWhiteSpace(cancellationReason))
        {
            order.Notes = string.IsNullOrWhiteSpace(order.Notes)
                ? $"[تم الإلغاء: {cancellationReason}]"
                : $"{order.Notes} | [تم الإلغاء: {cancellationReason}]";
        }
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WorkOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return (await GetProductionOrderByIdAsync(order.Id))!;
    }

    public async Task<bool> DeleteProductionOrderAsync(int id)
    {
        var order = await _repositoryManager.WorkOrderRepository.GetOrderWithDetailsAsync(id, trackChanges: true);
        if (order == null) return false;

        if (order.OrderStatus != ProductionOrderStatus.Draft && order.OrderStatus != ProductionOrderStatus.Cancelled)
        {
            throw new InvalidOperationException($"لا يمكن حذف أمر الإنتاج في حالة '{order.OrderStatus}'. الحذف مسموح فقط للمسودات (Draft) أو الأوامر الملغاة (Cancelled).");
        }

        if (order.ProductionRecords != null && order.ProductionRecords.Any())
        {
            throw new InvalidOperationException("لا يمكن حذف هذا الأمر لوجود تسجيلات إنتاج سابقة مرتبطة به.");
        }

        _repositoryManager.WorkOrderRepository.Remove(order);
        await _repositoryManager.SaveAsync();
        return true;
    }

    public async Task<ProductionOrderSummaryDto> GetProductionOrderSummaryAsync()
    {
        var all = (await _repositoryManager.WorkOrderRepository.GetAllAsync()).ToList();

        var total = all.Count;
        var draft = all.Count(o => o.OrderStatus == ProductionOrderStatus.Draft);
        var planned = all.Count(o => o.OrderStatus == ProductionOrderStatus.Planned);
        var released = all.Count(o => o.OrderStatus == ProductionOrderStatus.Released);
        var inProgress = all.Count(o => o.OrderStatus == ProductionOrderStatus.InProgress);
        var completed = all.Count(o => o.OrderStatus == ProductionOrderStatus.Completed);
        var cancelled = all.Count(o => o.OrderStatus == ProductionOrderStatus.Cancelled);
        var totalPlannedQty = all.Where(o => o.OrderStatus != ProductionOrderStatus.Cancelled).Sum(o => o.PlannedQuantity);

        return new ProductionOrderSummaryDto(total, draft, planned, released, inProgress, completed, cancelled, totalPlannedQty);
    }

    public async Task<string> GenerateOrderNumberAsync()
    {
        var datePrefix = $"PO-{DateTime.UtcNow:yyyyMMdd}-";
        var countToday = (await _repositoryManager.WorkOrderRepository.GetAllAsync())
            .Count(w => w.OrderNumber.StartsWith(datePrefix));

        return $"{datePrefix}{(countToday + 1):D4}";
    }

    #endregion

    #region Legacy Compatibility Methods

    public async Task<IEnumerable<GetAllWorkOrderResponse>> GetAllAsync()
    {
        var workOrders = await _repositoryManager.WorkOrderRepository.GetAllAsync();
        var products = (await _repositoryManager.ProductRepository.GetAllAsync()).ToDictionary(p => p.Id, p => p);
        var machines = (await _repositoryManager.MachineRepository.GetAllAsync()).ToDictionary(m => m.Id, m => m);

        return workOrders.Select(w => new GetAllWorkOrderResponse(
            w.Id,
            CreatedAt: w.CreatedAt,
            UpdatedAt: w.UpdatedAt,
            ProductId: w.ProductId,
            ProductName: products.TryGetValue(w.ProductId, out var product) ? product.Name : null,
            MachineId: w.MachineId ?? 0,
            MachineName: w.MachineId.HasValue && machines.TryGetValue(w.MachineId.Value, out var machine) ? machine.Name : null,
            Quantity: (int)w.PlannedQuantity,
            StartDate: w.PlannedDate,
            EndDate: w.DueDate ?? w.PlannedDate,
            Status: w.Status
        ));
    }

    public async Task<GetWorkOrderResponse?> GetByIdAsync(int id)
    {
        var workOrder = await _repositoryManager.WorkOrderRepository.GetByIdAsync(id);
        if (workOrder == null) return null;
        var product = await _repositoryManager.ProductRepository.GetByIdAsync(workOrder.ProductId);
        var machine = workOrder.MachineId.HasValue ? await _repositoryManager.MachineRepository.GetByIdAsync(workOrder.MachineId.Value) : null;
        return new GetWorkOrderResponse(
            Id: workOrder.Id,
            CreatedAt: workOrder.CreatedAt,
            UpdatedAt: workOrder.UpdatedAt,
            ProductId: workOrder.ProductId,
            ProductName: product?.Name,
            MachineId: workOrder.MachineId ?? 0,
            MachineName: machine?.Name,
            Quantity: (int)workOrder.PlannedQuantity,
            StartDate: workOrder.PlannedDate,
            EndDate: workOrder.DueDate ?? workOrder.PlannedDate,
            Status: workOrder.Status);
    }

    public async Task<InsertWorkOrderResponse> CreateAsync(InsertWorkOrderRequest request)
    {
        var workOrder = _mapper.Map<WorkOrder>(request);
        if (string.IsNullOrWhiteSpace(workOrder.OrderNumber))
        {
            workOrder.OrderNumber = await GenerateOrderNumberAsync();
        }
        _repositoryManager.WorkOrderRepository.Create(workOrder);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<InsertWorkOrderResponse>(workOrder);
    }

    public async Task UpdateAsync(UpdateWorkOrderRequest request)
    {
        WorkOrder? workOrder = await _repositoryManager.WorkOrderRepository.GetByIdAsync(request.Id, trackChanges: true);
        if (workOrder == null) return;

        _mapper.Map(request, workOrder);
        _repositoryManager.WorkOrderRepository.Update(workOrder);
        await _repositoryManager.SaveAsync();
    }

    public async Task DeleteAsync(DeleteWorkOrderRequest request)
    {
        WorkOrder? entity = await _repositoryManager.WorkOrderRepository.GetByIdAsync(request.Id);
        if (entity != null)
        {
            _repositoryManager.WorkOrderRepository.Remove(entity);
            await _repositoryManager.SaveAsync();
        }
    }

    #endregion
}

public static class WorkOrderExtensions
{
    public static string StatusName(this WorkOrder order) => order.OrderStatus switch
    {
        ProductionOrderStatus.Draft => "مسودة (Draft)",
        ProductionOrderStatus.Planned => "مخطط (Planned)",
        ProductionOrderStatus.Released => "مطلق وجاهز (Released)",
        ProductionOrderStatus.InProgress => "قيد الإنتاج (In Progress)",
        ProductionOrderStatus.Completed => "مكتمل (Completed)",
        ProductionOrderStatus.Cancelled => "ملغي (Cancelled)",
        _ => order.OrderStatus.ToString()
    };
}