using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class FinishedGoodsReleaseService : IFinishedGoodsReleaseService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IQualityGateService _qualityGateService;
    private readonly IAccountingPostingService _postingService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public FinishedGoodsReleaseService(
        IRepositoryManager repositoryManager,
        IQualityGateService qualityGateService,
        IAccountingPostingService postingService,
        IMapper mapper,
        AppDbContext context)
    {
        _repositoryManager = repositoryManager;
        _qualityGateService = qualityGateService;
        _postingService = postingService;
        _mapper = mapper;
        _context = context;
    }

    public async Task<ReleaseAvailabilityDto> GetReleaseAvailabilityAsync(int batchId)
    {
        var batch = await _repositoryManager.ProductionBatchRepository.GetBatchWithDetailsAsync(batchId);
        if (batch == null)
        {
            throw new KeyNotFoundException($"دفعة الإنتاج بالمعرف #{batchId} غير موجودة.");
        }

        var product = batch.Product ?? await _repositoryManager.ProductRepository.GetByIdAsync(batch.ProductId);
        if (product == null)
        {
            throw new KeyNotFoundException($"المنتج التام بالمعرف #{batch.ProductId} غير موجود.");
        }

        var actualOutput = batch.ActualOutputQuantity > 0 ? batch.ActualOutputQuantity : batch.PlannedQuantity;

        // Calculate explicit output rejection loss from Phase 9 Waste records
        var wastes = await _repositoryManager.WasteRepository.GetAllWastesWithDetailsAsync(batchId: batchId);
        var outputRejections = wastes
            .Where(w => w.WasteType == WasteType.OutputRejection && w.Status == WasteStatus.Approved)
            .Sum(w => w.Quantity);

        if (outputRejections == 0 && batch.RejectedQuantity > 0)
        {
            outputRejections = batch.RejectedQuantity;
        }

        // Calculate already released quantity
        var alreadyReleased = await _repositoryManager.FinishedGoodsReleaseRepository.GetTotalReleasedQuantityForBatchAsync(batchId);

        // Remaining releasable
        var remainingReleasable = Math.Max(0m, actualOutput - outputRejections - alreadyReleased);

        // Check QC Gate
        var qcGate = await _qualityGateService.CanReleaseBatchAsync(batchId);

        // Check Packaging Gate
        var packagingOrders = await _repositoryManager.PackagingOrderRepository.GetOrdersForBatchAsync(batchId);
        var latestPkgOrder = packagingOrders.OrderByDescending(o => o.CreatedAt).FirstOrDefault();

        var packagingBOMs = await _repositoryManager.PackagingBOMRepository.GetAllWithDetailsAsync(onlyActive: true, productId: batch.ProductId);
        bool packagingRequired = packagingBOMs.Any() || packagingOrders.Any();

        bool packagingCompleted = true;
        string? packagingReason = null;

        if (packagingRequired)
        {
            if (latestPkgOrder == null)
            {
                packagingCompleted = false;
                packagingReason = "هذا المنتج يتطلب تعبئة وتغليف، ولم يتم إنشاء أو تنفيذ أي أمر تعبئة وتغليف للدفعة بعد.";
            }
            else if (latestPkgOrder.Status != PackagingOrderStatus.Completed)
            {
                packagingCompleted = false;
                packagingReason = $"أمر التعبئة والتغليف رقم [{latestPkgOrder.OrderNumber}] في حالة ({latestPkgOrder.Status}) ولم يكتمل بعد.";
            }
            else
            {
                packagingCompleted = true;
                packagingReason = $"تم إتمام أمر التعبئة والتغليف بنجاح برقم [{latestPkgOrder.OrderNumber}].";
            }
        }
        else
        {
            packagingReason = "هذا المنتج لا يتطلب تعبئة إضافية أو يباع بالوزن المباشر.";
        }

        // Determine Unit Cost
        decimal prodCost = 0m;
        if (batch.Consumptions != null && batch.Consumptions.Any())
        {
            var totalMaterialCost = batch.Consumptions.Sum(c => c.TotalCost > 0 ? c.TotalCost : (c.ActualQuantity * (c.Material?.CurrentCost ?? 0m)));
            if (actualOutput > 0)
            {
                prodCost = totalMaterialCost / actualOutput;
            }
        }

        if (prodCost == 0 && product.StandardCost > 0)
        {
            prodCost = product.StandardCost;
        }

        decimal pkgCost = 0m;
        if (latestPkgOrder != null && latestPkgOrder.PackagingMaterialCost > 0 && actualOutput > 0)
        {
            pkgCost = latestPkgOrder.PackagingMaterialCost / actualOutput;
        }

        decimal totalCost = prodCost + pkgCost;
        if (totalCost == 0 && product.StandardCost > 0)
        {
            totalCost = product.StandardCost;
        }

        var result = new ReleaseAvailabilityDto
        {
            BatchId = batch.Id,
            BatchNumber = batch.BatchNumber,
            ProductId = batch.ProductId,
            ProductName = product.Name,
            ProductCode = product.Code,
            ProductSKU = product.SKU,
            OutputUnit = batch.OutputUnit ?? product.Unit,
            PlannedQuantity = batch.PlannedQuantity,
            ActualOutputQuantity = actualOutput,
            RejectedOutputQuantity = outputRejections,
            AlreadyReleasedQuantity = alreadyReleased,
            RemainingReleaseableQuantity = remainingReleasable,
            ProductionDate = batch.ProductionDate,
            ExpiryDate = batch.ExpiryDate ?? batch.ProductionDate.AddDays(product.ExpiryPeriodDays),
            ProductionUnitCost = prodCost,
            PackagingUnitCost = pkgCost,
            TotalUnitCost = totalCost,
            QCGate = qcGate,
            PackagingRequired = packagingRequired,
            PackagingCompleted = packagingCompleted,
            PackagingOrderId = latestPkgOrder?.Id,
            PackagingOrderNumber = latestPkgOrder?.OrderNumber,
            PackagingOrderStatus = latestPkgOrder?.Status.ToString(),
            ActualPackagedQuantity = latestPkgOrder?.ActualQuantity,
            PackagingGateReason = packagingReason
        };

        if (!qcGate.IsAllowed)
        {
            result.BlockingReason = $"محظور من الإفراج: {qcGate.Reason}";
        }
        else if (packagingRequired && !packagingCompleted)
        {
            result.BlockingReason = $"محظور من الإفراج: {packagingReason}";
        }
        else if (remainingReleasable <= 0)
        {
            result.BlockingReason = "تم الإفراج عن كامل كمية الدفعة المتاحة بالفعل أو تم استبعاد المرفوضات بالكامل.";
        }

        return result;
    }

    public async Task<FinishedGoodsReleaseDto> ReleaseFinishedGoodsAsync(CreateFinishedGoodsReleaseRequest request, int userId)
    {
        if (request.Quantity <= 0)
        {
            throw new InvalidOperationException("كمية الإفراج للمنتج التام يجب أن تكون أكبر من الصفر.");
        }

        // Validate Warehouse Type
        var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.WarehouseId);
        if (warehouse == null)
        {
            throw new KeyNotFoundException($"المستودع بالمعرف #{request.WarehouseId} غير موجود.");
        }

        if (warehouse.Type != WarehouseType.FinishedGoods)
        {
            throw new InvalidOperationException($"المستودع المحدد '{warehouse.Name}' نوعه ({warehouse.Type}) وليس مستودع منتجات تامة (Finished Goods Warehouse). لا يمكن تخزين المنتجات التامة إلا في مستودع منتجات تامة.");
        }

        // Validate Location if supplied
        if (request.LocationId.HasValue && request.LocationId.Value > 0)
        {
            var location = await _repositoryManager.WarehouseLocationRepository.GetByIdAsync(request.LocationId.Value);
            if (location == null || location.WarehouseId != request.WarehouseId)
            {
                throw new InvalidOperationException("موقع التخزين المحدد لا يتبع المستودع المختار.");
            }
        }

        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Lock and evaluate availability under transaction to ensure strict concurrency protection
            var availability = await GetReleaseAvailabilityAsync(request.ProductionBatchId);

            if (!availability.CanRelease)
            {
                throw new InvalidOperationException(availability.BlockingReason ?? "لا يمكن إتمام الإفراج لعدم استيفاء شروط بوابة الجودة أو التعبئة.");
            }

            if (request.Quantity > availability.RemainingReleaseableQuantity)
            {
                throw new InvalidOperationException($"الكمية المطلوبة للإفراج ({request.Quantity} {availability.OutputUnit}) تتجاوز الكمية المتبقية المتاحة للإفراج ({availability.RemainingReleaseableQuantity} {availability.OutputUnit}).");
            }

            var batch = await _repositoryManager.ProductionBatchRepository.GetByIdAsync(request.ProductionBatchId);
            var product = await _repositoryManager.ProductRepository.GetByIdAsync(batch!.ProductId);

            // Generate deterministic release number: FG-YYYYMMDD-XXXX
            var date = DateTime.UtcNow;
            var todayCount = await _repositoryManager.FinishedGoodsReleaseRepository.GetCountForDateAsync(date);
            var releaseNumber = $"FG-{date:yyyyMMdd}-{(todayCount + 1):D4}";

            int retry = 2;
            while (!await _repositoryManager.FinishedGoodsReleaseRepository.IsReleaseNumberUniqueAsync(releaseNumber))
            {
                releaseNumber = $"FG-{date:yyyyMMdd}-{(todayCount + retry):D4}";
                retry++;
            }

            var unitCost = availability.TotalUnitCost;
            var totalCost = unitCost * request.Quantity;
            var expiryDate = availability.ExpiryDate ?? batch.ProductionDate.AddDays(product?.ExpiryPeriodDays ?? 180);

            // 1. Find or create FinishedGoodsStock
            var stock = await _repositoryManager.FinishedGoodsStockRepository.FindStockAsync(
                request.WarehouseId, request.LocationId, batch.ProductId, batch.BatchNumber, trackChanges: true);

            if (stock == null)
            {
                stock = new FinishedGoodsStock
                {
                    ProductId = batch.ProductId,
                    ProductionBatchId = batch.Id,
                    WarehouseId = request.WarehouseId,
                    LocationId = request.LocationId,
                    BatchNumber = batch.BatchNumber,
                    Quantity = request.Quantity,
                    Unit = availability.OutputUnit,
                    ProductionDate = batch.ProductionDate,
                    ExpiryDate = expiryDate,
                    UnitCost = unitCost,
                    TotalCost = totalCost,
                    QCInspectionId = availability.QCGate.InspectionNumber != null ? (await _repositoryManager.QualityInspectionRepository.GetInspectionByNumberAsync(availability.QCGate.InspectionNumber))?.Id : null,
                    PackagingOrderId = availability.PackagingOrderId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _repositoryManager.FinishedGoodsStockRepository.Create(stock);
            }
            else
            {
                stock.Quantity += request.Quantity;
                stock.TotalCost += totalCost;
                stock.UnitCost = stock.Quantity > 0 ? (stock.TotalCost / stock.Quantity) : unitCost;
                stock.UpdatedAt = DateTime.UtcNow;
                _repositoryManager.FinishedGoodsStockRepository.Update(stock);
            }

            // 2. Create central InventoryTransaction audit record
            var invTx = new InventoryTransaction
            {
                TransactionType = InventoryTransactionType.FinishedGoodsReceipt,
                TransactionDate = DateTime.UtcNow,
                WarehouseId = request.WarehouseId,
                DestinationLocationId = request.LocationId,
                ProductId = batch.ProductId,
                BatchNumber = batch.BatchNumber,
                Quantity = request.Quantity,
                Unit = availability.OutputUnit,
                UnitCost = unitCost,
                TotalCost = totalCost,
                ReferenceDocumentNumber = releaseNumber,
                UserId = userId,
                Notes = $"إفراج منتج تام للدفعة {batch.BatchNumber}. {request.Notes}".Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _repositoryManager.InventoryTransactionRepository.Create(invTx);
            await _repositoryManager.SaveAsync();

            // 3. Create FinishedGoodsRelease record
            var release = new FinishedGoodsRelease
            {
                ReleaseNumber = releaseNumber,
                ProductId = batch.ProductId,
                ProductionBatchId = batch.Id,
                PackagingOrderId = availability.PackagingOrderId,
                QCInspectionId = stock.QCInspectionId,
                WarehouseId = request.WarehouseId,
                LocationId = request.LocationId,
                BatchNumber = batch.BatchNumber,
                Quantity = request.Quantity,
                Unit = availability.OutputUnit,
                UnitCost = unitCost,
                TotalCost = totalCost,
                ProductionDate = batch.ProductionDate,
                ExpiryDate = expiryDate,
                ReleasedByUserId = userId,
                ReleasedAt = DateTime.UtcNow,
                Notes = request.Notes?.Trim(),
                InventoryTransactionId = invTx.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _repositoryManager.FinishedGoodsReleaseRepository.Create(release);

            await _repositoryManager.SaveAsync();
            await dbTransaction.CommitAsync();

            // Automatic Accounting Posting: Dr Finished Goods Inventory, Cr Production Clearing
            await _postingService.PostFinishedGoodsReleaseAsync(release.Id, userId);

            return await GetReleaseByIdAsync(release.Id);
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<FinishedGoodsReleaseDto>> GetAllReleasesAsync(
        int? productId = null,
        int? batchId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var releases = await _repositoryManager.FinishedGoodsReleaseRepository.GetAllWithDetailsAsync(
            productId, batchId, warehouseId, fromDate, toDate, searchTerm);

        return _mapper.Map<IEnumerable<FinishedGoodsReleaseDto>>(releases);
    }

    public async Task<FinishedGoodsReleaseDto> GetReleaseByIdAsync(int id)
    {
        var release = await _repositoryManager.FinishedGoodsReleaseRepository.GetByIdWithDetailsAsync(id);
        if (release == null)
        {
            throw new KeyNotFoundException($"سجل إفراج المنتج التام بالمعرف #{id} غير موجود.");
        }

        return _mapper.Map<FinishedGoodsReleaseDto>(release);
    }

    public async Task<IEnumerable<FinishedGoodsReleaseDto>> GetReleasesForBatchAsync(int batchId)
    {
        var releases = await _repositoryManager.FinishedGoodsReleaseRepository.GetReleasesForBatchAsync(batchId);
        return _mapper.Map<IEnumerable<FinishedGoodsReleaseDto>>(releases);
    }
}
