using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class PackagingOrderService : IPackagingOrderService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IQualityGateService _qualityGateService;
    private readonly IInventoryService _inventoryService;
    private readonly IPackagingCostService _packagingCostService;

    public PackagingOrderService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IQualityGateService qualityGateService,
        IInventoryService inventoryService,
        IPackagingCostService packagingCostService)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _qualityGateService = qualityGateService;
        _inventoryService = inventoryService;
        _packagingCostService = packagingCostService;
    }

    public async Task<IEnumerable<PackagingOrderDto>> GetAllOrdersAsync(
        PackagingOrderStatus? status = null,
        int? batchId = null,
        int? productId = null,
        int? bomId = null,
        int? operatorId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var orders = await _repositoryManager.PackagingOrderRepository.GetAllWithDetailsAsync(
            status, batchId, productId, bomId, operatorId, fromDate, toDate, searchTerm);

        var dtos = new List<PackagingOrderDto>();
        foreach (var order in orders)
        {
            var dto = MapToDto(order);
            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<PackagingOrderDto> GetOrderByIdAsync(int id)
    {
        var order = await _repositoryManager.PackagingOrderRepository.GetByIdWithDetailsAsync(id);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر التعبئة والتغليف بالمعرف #{id} غير موجود.");
        }

        var dto = MapToDto(order);
        dto.QCGateStatus = await _qualityGateService.CanReleaseBatchAsync(order.ProductionBatchId);

        if (order.PackagingBOM != null)
        {
            var targetQty = order.ActualQuantity > 0 ? order.ActualQuantity : order.PlannedQuantity;
            dto.Requirements = await CalculateOrderRequirementsAsync(
                order.PackagingBOMId, targetQty, order.PackagingBOMVersionId);
        }

        return dto;
    }

    public async Task<PackagingOrderDto> CreateOrderAsync(CreatePackagingOrderRequest request, int userId)
    {
        if (request.PlannedQuantity <= 0)
        {
            throw new InvalidOperationException("الكمية المخططة للتعبئة يجب أن تكون أكبر من الصفر.");
        }

        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(request.ProductionBatchId);
        if (batch == null)
        {
            throw new KeyNotFoundException($"دفعة الإنتاج بالمعرف #{request.ProductionBatchId} غير موجودة.");
        }

        var bom = await _repositoryManager.PackagingBOMRepository.GetByIdWithDetailsAsync(request.PackagingBOMId);
        if (bom == null || !bom.IsActive)
        {
            throw new KeyNotFoundException($"مواصفة التعبئة بالمعرف #{request.PackagingBOMId} غير موجودة أو معطلة.");
        }

        if (bom.ProductId != batch.ProductId)
        {
            throw new InvalidOperationException($"مواصفة التعبئة المحددة تتبع المنتج #{bom.ProductId}، بينما دفعة الإنتاج تنتج المنتج #{batch.ProductId}.");
        }

        // Verify QC Gate (Strict check)
        var gateStatus = await _qualityGateService.CanReleaseBatchAsync(batch.Id);
        if (!gateStatus.IsAllowed)
        {
            throw new InvalidOperationException($"لا يمكن إنشاء أمر التعبئة لأن دفعة الإنتاج #{batch.BatchNumber} محظورة من بوابة الجودة (QC Gate): {gateStatus.Reason}");
        }

        // Resolve active version
        PackagingBOMVersion? version = null;
        if (request.PackagingBOMVersionId.HasValue && request.PackagingBOMVersionId.Value > 0)
        {
            version = bom.Versions.FirstOrDefault(v => v.Id == request.PackagingBOMVersionId.Value);
        }
        else
        {
            version = bom.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault(v => v.Status == PackagingBOMStatus.Active)
                      ?? bom.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
        }

        // Calculate theoretical maximum packs
        var packSizeKg = bom.PackSizeKg > 0 ? bom.PackSizeKg : 1.0m;
        var batchOutputKg = batch.ActualOutputQuantity > 0 ? batch.ActualOutputQuantity : (batch.ActualOutput > 0 ? batch.ActualOutput : batch.PlannedQuantity);
        var maxPacks = packSizeKg > 0 ? Math.Floor(batchOutputKg / packSizeKg) : 0m;

        // Generate deterministic OrderNumber: PKG-YYYYMMDD-XXXX
        var today = DateTime.UtcNow;
        var datePrefix = $"PKG-{today:yyyyMMdd}-";
        var countToday = await _repositoryManager.PackagingOrderRepository.GetCountForDateAsync(today);
        var sequence = countToday + 1;
        var orderNumber = $"{datePrefix}{sequence:D4}";

        while (!await _repositoryManager.PackagingOrderRepository.IsOrderNumberUniqueAsync(orderNumber))
        {
            sequence++;
            orderNumber = $"{datePrefix}{sequence:D4}";
        }

        var order = new PackagingOrder
        {
            OrderNumber = orderNumber,
            ProductionBatchId = batch.Id,
            ProductId = batch.ProductId,
            PackagingBOMId = bom.Id,
            PackagingBOMVersionId = version?.Id,
            PlannedQuantity = request.PlannedQuantity,
            ActualQuantity = 0m,
            TheoreticalMaxPacks = maxPacks,
            Status = PackagingOrderStatus.Planned,
            OperatorId = request.OperatorId,
            Notes = request.Notes?.Trim(),
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryManager.PackagingOrderRepository.Create(order);
        await _repositoryManager.SaveAsync();

        return await GetOrderByIdAsync(order.Id);
    }

    public async Task<List<PackagingRequirementDto>> CalculateOrderRequirementsAsync(
        int bomId, decimal packQuantity, int? versionId = null, int? warehouseId = null)
    {
        var bom = await _repositoryManager.PackagingBOMRepository.GetByIdWithDetailsAsync(bomId);
        if (bom == null) return new List<PackagingRequirementDto>();

        PackagingBOMVersion? version = null;
        if (versionId.HasValue && versionId.Value > 0)
        {
            version = bom.Versions.FirstOrDefault(v => v.Id == versionId.Value);
        }
        else
        {
            version = bom.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault(v => v.Status == PackagingBOMStatus.Active)
                      ?? bom.Versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();
        }

        var items = version != null && version.Items.Any()
            ? version.Items.ToList()
            : bom.Items.ToList();

        var requirements = new List<PackagingRequirementDto>();
        var now = DateTime.UtcNow.Date;

        foreach (var item in items.OrderBy(i => i.Sequence))
        {
            var material = item.Material ?? await _repositoryManager.MaterialRepository.GetByIdAsync(item.MaterialId);
            var reqQty = Math.Round(item.QuantityRequired * packQuantity, 4);

            // Calculate valid non-expired available stock
            var stockBalances = await _repositoryManager.StockBalanceRepository.GetStockBalancesAsync(
                warehouseId: warehouseId, locationId: null, materialId: item.MaterialId, productId: null, batchNumber: null);

            var validStock = stockBalances.Where(sb => !sb.ExpiryDate.HasValue || sb.ExpiryDate.Value.Date >= now);
            var availableQty = validStock.Sum(sb => sb.Quantity);

            requirements.Add(new PackagingRequirementDto
            {
                MaterialId = item.MaterialId,
                MaterialCode = material?.Code ?? $"MAT-{item.MaterialId}",
                MaterialName = material?.Name ?? $"Material #{item.MaterialId}",
                QuantityPerPack = item.QuantityRequired,
                RequiredQuantity = reqQty,
                AvailableQuantity = availableQty,
                Unit = item.Unit
            });
        }

        return requirements;
    }

    public async Task<PackagingOrderDto> StartOrderAsync(int orderId, int userId)
    {
        var order = await _repositoryManager.PackagingOrderRepository.GetByIdWithDetailsAsync(orderId, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر التعبئة بالمعرف #{orderId} غير موجود.");
        }

        if (order.Status != PackagingOrderStatus.Draft && order.Status != PackagingOrderStatus.Planned && order.Status != PackagingOrderStatus.Paused)
        {
            throw new InvalidOperationException($"لا يمكن بدء أمر التعبئة في حالته الحالية: {order.Status}");
        }

        // Verify QC Gate
        var gateStatus = await _qualityGateService.CanReleaseBatchAsync(order.ProductionBatchId);
        if (!gateStatus.IsAllowed)
        {
            throw new InvalidOperationException($"لا يمكن بدء تنفيذ أمر التعبئة: {gateStatus.Reason}");
        }

        order.Status = PackagingOrderStatus.InProgress;
        if (!order.StartTime.HasValue)
        {
            order.StartTime = DateTime.UtcNow;
        }
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PackagingOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return await GetOrderByIdAsync(order.Id);
    }

    public async Task<PackagingOrderDto> PauseOrderAsync(PausePackagingOrderRequest request, int userId)
    {
        var order = await _repositoryManager.PackagingOrderRepository.GetByIdWithDetailsAsync(request.PackagingOrderId, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر التعبئة بالمعرف #{request.PackagingOrderId} غير موجود.");
        }

        if (order.Status != PackagingOrderStatus.InProgress)
        {
            throw new InvalidOperationException("لا يمكن إيقاف أمر التعبئة مؤقتاً إلا إذا كان قيد التنفيذ حالياً.");
        }

        order.Status = PackagingOrderStatus.Paused;
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            order.Notes = (string.IsNullOrEmpty(order.Notes) ? "" : order.Notes + " | ") + $"[إيقاف مؤقت: {request.Notes}]";
        }
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PackagingOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return await GetOrderByIdAsync(order.Id);
    }

    public async Task<PackagingOrderDto> ResumeOrderAsync(int orderId, int userId)
    {
        var order = await _repositoryManager.PackagingOrderRepository.GetByIdWithDetailsAsync(orderId, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر التعبئة بالمعرف #{orderId} غير موجود.");
        }

        if (order.Status != PackagingOrderStatus.Paused)
        {
            throw new InvalidOperationException("أمر التعبئة ليس في حالة إيقاف مؤقت.");
        }

        order.Status = PackagingOrderStatus.InProgress;
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PackagingOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return await GetOrderByIdAsync(order.Id);
    }

    public async Task<PackagingOrderDto> ExecuteAndCompleteOrderAsync(ExecutePackagingOrderRequest request, int userId)
    {
        if (request.ActualPackagedQuantity <= 0)
        {
            throw new InvalidOperationException("الكمية المعبأة الفعلية يجب أن تكون أكبر من الصفر.");
        }

        var order = await _repositoryManager.PackagingOrderRepository.GetByIdWithDetailsAsync(request.PackagingOrderId, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر التعبئة بالمعرف #{request.PackagingOrderId} غير موجود.");
        }

        if (order.Status == PackagingOrderStatus.Completed)
        {
            throw new InvalidOperationException("أمر التعبئة مكتمل بالفعل.");
        }

        if (order.Status == PackagingOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("لا يمكن استكمال أمر تعبئة ملغي.");
        }

        // Verify QC Gate once more
        var gateStatus = await _qualityGateService.CanReleaseBatchAsync(order.ProductionBatchId);
        if (!gateStatus.IsAllowed)
        {
            throw new InvalidOperationException($"فشل التحقق من بوابة الجودة: {gateStatus.Reason}");
        }

        // Build list of consumption items
        var consumptionRequests = new List<PackagingConsumptionItemRequest>();

        if (request.Consumptions != null && request.Consumptions.Any(c => c.Quantity > 0))
        {
            consumptionRequests = request.Consumptions.Where(c => c.Quantity > 0).ToList();
        }
        else
        {
            // Auto-calculate standard consumption based on BOM items
            var requirements = await CalculateOrderRequirementsAsync(
                order.PackagingBOMId, request.ActualPackagedQuantity, order.PackagingBOMVersionId, request.WarehouseId);

            foreach (var req in requirements)
            {
                consumptionRequests.Add(new PackagingConsumptionItemRequest
                {
                    MaterialId = req.MaterialId,
                    Quantity = req.RequiredQuantity,
                    Unit = req.Unit,
                    WarehouseId = request.WarehouseId,
                    LocationId = request.LocationId,
                    Notes = $"صرف قياسي بموجب مواصفة التعبئة لأمر {order.OrderNumber}"
                });
            }
        }

        // Perform Atomic Inventory Consumption
        var transactionLogs = await _inventoryService.ConsumeStockForPackagingBatchAsync(
            order.Id, order.OrderNumber, consumptionRequests, userId);

        // Record PackagingConsumption entries
        decimal totalPackagingCost = 0m;
        foreach (var tx in transactionLogs)
        {
            var plannedForThis = Math.Round(order.PlannedQuantity * (tx.Quantity / (request.ActualPackagedQuantity > 0 ? request.ActualPackagedQuantity : 1m)), 4);
            var consumption = new PackagingConsumption
            {
                PackagingOrderId = order.Id,
                MaterialId = tx.MaterialId!.Value,
                PlannedQuantity = plannedForThis,
                ActualQuantity = tx.Quantity,
                Unit = tx.Unit,
                WarehouseId = tx.WarehouseId,
                LocationId = tx.SourceLocationId,
                BatchNumber = tx.BatchNumber,
                UnitCost = tx.UnitCost,
                TotalCost = tx.TotalCost,
                InventoryTransactionId = tx.Id,
                Notes = tx.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            order.Consumptions.Add(consumption);
            totalPackagingCost += tx.TotalCost;
        }

        // Update Packaging Order state
        order.ActualQuantity = request.ActualPackagedQuantity;
        order.PackagingMaterialCost = totalPackagingCost;
        order.Status = PackagingOrderStatus.Completed;
        if (!order.StartTime.HasValue)
        {
            order.StartTime = DateTime.UtcNow.AddMinutes(-30);
        }
        order.EndTime = DateTime.UtcNow;
        order.CompletedByUserId = userId;
        if (!string.IsNullOrWhiteSpace(request.Notes))
        {
            order.Notes = (string.IsNullOrEmpty(order.Notes) ? "" : order.Notes + " | ") + request.Notes.Trim();
        }
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PackagingOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return await GetOrderByIdAsync(order.Id);
    }

    public async Task<PackagingOrderDto> CancelOrderAsync(CancelPackagingOrderRequest request, int userId)
    {
        var order = await _repositoryManager.PackagingOrderRepository.GetByIdWithDetailsAsync(request.PackagingOrderId, trackChanges: true);
        if (order == null)
        {
            throw new KeyNotFoundException($"أمر التعبئة بالمعرف #{request.PackagingOrderId} غير موجود.");
        }

        if (order.Status == PackagingOrderStatus.Completed)
        {
            throw new InvalidOperationException("لا يمكن إلغاء أمر تعبئة تم اكتماله وصرف مواده بالفعل.");
        }

        if (order.Status == PackagingOrderStatus.Cancelled)
        {
            throw new InvalidOperationException("أمر التعبئة ملغي بالفعل.");
        }

        order.Status = PackagingOrderStatus.Cancelled;
        order.CancellationReason = request.CancellationReason?.Trim() ?? "تم الإلغاء بواسطة المستخدم";
        order.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.PackagingOrderRepository.Update(order);
        await _repositoryManager.SaveAsync();

        return await GetOrderByIdAsync(order.Id);
    }

    public async Task<decimal> CalculateTheoreticalMaxPacksAsync(int batchId, int packagingBomId)
    {
        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(batchId);
        var bom = await _repositoryManager.PackagingBOMRepository.GetByIdWithDetailsAsync(packagingBomId);

        if (batch == null || bom == null) return 0m;

        var packSizeKg = bom.PackSizeKg > 0 ? bom.PackSizeKg : 1.0m;
        var batchOutputKg = batch.ActualOutputQuantity > 0 ? batch.ActualOutputQuantity : (batch.ActualOutput > 0 ? batch.ActualOutput : batch.PlannedQuantity);

        return packSizeKg > 0 ? Math.Floor(batchOutputKg / packSizeKg) : 0m;
    }

    private PackagingOrderDto MapToDto(PackagingOrder order)
    {
        var dto = new PackagingOrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            ProductionBatchId = order.ProductionBatchId,
            BatchNumber = order.ProductionBatch?.BatchNumber ?? $"Batch #{order.ProductionBatchId}",
            ProductId = order.ProductId,
            ProductName = order.Product?.Name ?? $"Product #{order.ProductId}",
            ProductCode = order.Product?.Code,
            PackagingBOMId = order.PackagingBOMId,
            PackagingBOMName = order.PackagingBOM?.Name ?? $"BOM #{order.PackagingBOMId}",
            PackagingBOMCode = order.PackagingBOM?.Code ?? string.Empty,
            PackagingBOMVersionId = order.PackagingBOMVersionId,
            VersionNumber = order.PackagingBOMVersion?.VersionNumber ?? 1,
            PlannedQuantity = order.PlannedQuantity,
            ActualQuantity = order.ActualQuantity,
            TheoreticalMaxPacks = order.TheoreticalMaxPacks,
            PackSizeKg = order.PackagingBOM?.PackSizeKg ?? 1.0m,
            PackUnit = order.PackagingBOM?.PackUnit ?? "Box",
            Status = order.Status,
            StartTime = order.StartTime,
            EndTime = order.EndTime,
            OperatorId = order.OperatorId,
            OperatorName = order.Operator?.Name,
            PackagingMaterialCost = order.PackagingMaterialCost,
            Notes = order.Notes,
            CancellationReason = order.CancellationReason,
            CreatedAt = order.CreatedAt
        };

        if (order.Consumptions != null && order.Consumptions.Any())
        {
            foreach (var c in order.Consumptions)
            {
                dto.Consumptions.Add(new PackagingConsumptionDto
                {
                    Id = c.Id,
                    PackagingOrderId = c.PackagingOrderId,
                    MaterialId = c.MaterialId,
                    MaterialName = c.Material?.Name ?? $"Material #{c.MaterialId}",
                    MaterialCode = c.Material?.Code,
                    PlannedQuantity = c.PlannedQuantity,
                    ActualQuantity = c.ActualQuantity,
                    Unit = c.Unit,
                    WarehouseId = c.WarehouseId,
                    WarehouseName = c.Warehouse?.Name ?? $"Warehouse #{c.WarehouseId}",
                    LocationId = c.LocationId,
                    LocationName = c.Location?.Name,
                    BatchNumber = c.BatchNumber,
                    UnitCost = c.UnitCost,
                    TotalCost = c.TotalCost,
                    InventoryTransactionId = c.InventoryTransactionId,
                    Notes = c.Notes
                });
            }
        }

        return dto;
    }
}
