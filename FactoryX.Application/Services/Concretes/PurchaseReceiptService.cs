using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class PurchaseReceiptService : IPurchaseReceiptService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IAccountingPostingService _postingService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public PurchaseReceiptService(
        IRepositoryManager repositoryManager,
        IAccountingPostingService postingService,
        IMapper mapper,
        AppDbContext context)
    {
        _repositoryManager = repositoryManager;
        _postingService = postingService;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<PurchaseReceiptDto>> GetAllReceiptsAsync(
        PurchaseReceiptStatus? status = null,
        int? purchaseOrderId = null,
        int? supplierId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var receipts = await _repositoryManager.PurchaseReceiptRepository.GetAllReceiptsAsync(
            status, purchaseOrderId, supplierId, warehouseId, fromDate, toDate, searchTerm);
        return _mapper.Map<IEnumerable<PurchaseReceiptDto>>(receipts);
    }

    public async Task<PurchaseReceiptDto?> GetReceiptByIdAsync(int id)
    {
        var receipt = await _repositoryManager.PurchaseReceiptRepository.GetByIdWithDetailsAsync(id);
        return _mapper.Map<PurchaseReceiptDto>(receipt);
    }

    public async Task<PurchaseReceiptDto> CreateAndPostReceiptAsync(CreatePurchaseReceiptRequest request, int userId)
    {
        if (request.Items == null || !request.Items.Any())
        {
            throw new InvalidOperationException("يجب تضمين بند واحد على الأقل في محضر وسند الاستلام المخزني.");
        }

        // 1. Validate PO
        var po = await _repositoryManager.PurchaseOrderRepository.GetByIdWithDetailsAsync(request.PurchaseOrderId, trackChanges: true);
        if (po == null)
        {
            throw new KeyNotFoundException($"أمر الشراء بالمعرف #{request.PurchaseOrderId} غير موجود.");
        }

        if (po.Status != PurchaseOrderStatus.Approved && po.Status != PurchaseOrderStatus.PartiallyReceived)
        {
            throw new InvalidOperationException($"لا يمكن استلام أو ترحيل شحنات لأمر شراء غير معتمد. الحالة الحالية لأمر الشراء: {po.Status}.");
        }

        // 2. Validate Default Warehouse
        var defaultWh = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.WarehouseId);
        if (defaultWh == null)
        {
            throw new KeyNotFoundException($"المستودع بالمعرف #{request.WarehouseId} غير موجود.");
        }

        if (defaultWh.Type != WarehouseType.RawMaterial && defaultWh.Type != WarehouseType.Packaging)
        {
            throw new InvalidOperationException("مستودع استلام الخامات ومواد التعبئة يجب أن يكون من نوع مستودع خامات (RawMaterial) أو مستودع تعبئة (Packaging). لا يمكن إدخال خامات مشتراة إلى مستودع منتجات تامة أو مستودع هوالك.");
        }

        // 3. Validate Supplier
        var supplier = await _repositoryManager.SupplierRepository.GetByIdWithDetailsAsync(request.SupplierId);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"المورد بالمعرف #{request.SupplierId} غير موجود.");
        }

        var receiptDate = request.ReceiptDate.Date;
        var now = DateTime.UtcNow.Date;

        // 4. Generate deterministic Receipt Number: GRN-YYYYMMDD-XXXX
        var date = request.ReceiptDate.Date;
        var count = await _repositoryManager.PurchaseReceiptRepository.GetCountForDateAsync(date);
        var receiptNumber = $"GRN-{date:yyyyMMdd}-{(count + 1):D4}";

        int suffix = 1;
        while (!await _repositoryManager.PurchaseReceiptRepository.IsReceiptNumberUniqueAsync(receiptNumber))
        {
            receiptNumber = $"GRN-{date:yyyyMMdd}-{(count + 1 + suffix):D4}";
            suffix++;
        }

        // 5. Atomic Posting Execution
        using var dbTransaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var receipt = new PurchaseReceipt
            {
                ReceiptNumber = receiptNumber,
                PurchaseOrderId = request.PurchaseOrderId,
                SupplierId = request.SupplierId,
                ReceiptDate = request.ReceiptDate,
                WarehouseId = request.WarehouseId,
                Status = PurchaseReceiptStatus.Posted,
                ReceivedByUserId = userId,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Items = new List<PurchaseReceiptItem>()
            };

            decimal totalReceiptCost = 0m;
            int itemIndex = 1;

            foreach (var itemReq in request.Items)
            {
                if (itemReq.ReceivedQuantity < 0 || itemReq.AcceptedQuantity < 0 || itemReq.RejectedQuantity < 0)
                {
                    throw new InvalidOperationException("الكميات المستلمة أو المقبولة أو المرفوضة لا يمكن أن تكون سالبة.");
                }

                if ((itemReq.AcceptedQuantity + itemReq.RejectedQuantity) > itemReq.ReceivedQuantity)
                {
                    throw new InvalidOperationException("مجموع الكمية المقبولة والمرفوضة لا يمكن أن يتجاوز إجمالي الكمية المستلمة.");
                }

                if (itemReq.ReceivedQuantity == 0 && itemReq.AcceptedQuantity == 0)
                {
                    continue; // Skip zero items if any
                }

                var material = await _repositoryManager.MaterialRepository.GetByIdWithDetailsAsync(itemReq.MaterialId, trackChanges: true);
                if (material == null)
                {
                    throw new KeyNotFoundException($"المادة بالمعرف #{itemReq.MaterialId} غير موجودة.");
                }

                // Check PO Line remaining quantity (Over-Receipt Protection)
                var poItem = po.Items.FirstOrDefault(i => i.Id == itemReq.PurchaseOrderItemId || i.MaterialId == itemReq.MaterialId);
                if (poItem != null)
                {
                    var remainingPOQty = Math.Max(0m, poItem.OrderedQuantity - poItem.ReceivedQuantity);
                    if (itemReq.AcceptedQuantity > remainingPOQty)
                    {
                        throw new InvalidOperationException($"الكمية المقبولة للمادة '{material.Name}' ({itemReq.AcceptedQuantity} {itemReq.Unit}) تتجاوز الكمية المتبقية بأمر الشراء ({remainingPOQty} {itemReq.Unit}). نظام ضبط المشتريات يمنع الاستلام الزائد (Over-Receipt).");
                    }
                }

                // Validate item warehouse & location
                var targetWhId = itemReq.WarehouseId > 0 ? itemReq.WarehouseId : request.WarehouseId;
                var targetWh = await _repositoryManager.WarehouseRepository.GetByIdAsync(targetWhId);
                if (targetWh == null)
                {
                    throw new KeyNotFoundException($"المستودع المحدد للبند #{itemReq.MaterialId} غير موجود.");
                }
                if (targetWh.Type != WarehouseType.RawMaterial && targetWh.Type != WarehouseType.Packaging)
                {
                    throw new InvalidOperationException($"المستودع '{targetWh.Name}' نوعه ({targetWh.Type}) وليس مستودع خامات أو تعبئة. لا يمكن استلام المواد فيه.");
                }

                if (itemReq.LocationId.HasValue && itemReq.LocationId.Value > 0)
                {
                    var location = await _repositoryManager.WarehouseLocationRepository.GetByIdAsync(itemReq.LocationId.Value);
                    if (location == null || location.WarehouseId != targetWhId)
                    {
                        throw new InvalidOperationException($"مكان التخزين المحدد لا يتبع المستودع '{targetWh.Name}'.");
                    }
                }

                // Expiry Date Validation
                if (itemReq.ExpiryDate.HasValue)
                {
                    if (itemReq.ExpiryDate.Value.Date <= receiptDate)
                    {
                        throw new InvalidOperationException($"تاريخ انتهاء صلاحية المادة '{material.Name}' ({itemReq.ExpiryDate.Value:yyyy-MM-dd}) منتهي أو يطابق تاريخ الاستلام. يمنع النظام استلام خامات منتهية الصلاحية قطيعاً.");
                    }
                }

                // Batch Number handling: Prefer supplier's batch, fallback to internal deterministic lot
                var internalBatch = $"INT-{receiptNumber}-{itemIndex:D2}";
                var effectiveBatch = !string.IsNullOrWhiteSpace(itemReq.SupplierBatchNumber) 
                    ? itemReq.SupplierBatchNumber.Trim() 
                    : internalBatch;

                var unitPrice = itemReq.UnitPrice > 0 ? itemReq.UnitPrice : (poItem?.UnitPrice ?? material.StandardCost);
                var lineCost = Math.Round(itemReq.AcceptedQuantity * unitPrice, 4);
                totalReceiptCost += lineCost;

                int? invTransactionId = null;

                // ONLY ACCEPTED QUANTITY INCREASES STOCK
                if (itemReq.AcceptedQuantity > 0)
                {
                    // 1. StockBalance update or create
                    var stock = await _repositoryManager.StockBalanceRepository.FindStockAsync(
                        targetWhId, itemReq.LocationId, itemReq.MaterialId, null, effectiveBatch);

                    if (stock == null)
                    {
                        stock = new StockBalance
                        {
                            WarehouseId = targetWhId,
                            LocationId = itemReq.LocationId,
                            MaterialId = itemReq.MaterialId,
                            BatchNumber = effectiveBatch,
                            Quantity = itemReq.AcceptedQuantity,
                            Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? material.Unit : itemReq.Unit,
                            ManufacturingDate = itemReq.ManufacturingDate,
                            ExpiryDate = itemReq.ExpiryDate,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _repositoryManager.StockBalanceRepository.Create(stock);
                    }
                    else
                    {
                        stock.Quantity += itemReq.AcceptedQuantity;
                        if (itemReq.ExpiryDate.HasValue) stock.ExpiryDate = itemReq.ExpiryDate;
                        if (itemReq.ManufacturingDate.HasValue) stock.ManufacturingDate = itemReq.ManufacturingDate;
                        stock.UpdatedAt = DateTime.UtcNow;
                        _repositoryManager.StockBalanceRepository.Update(stock);
                    }

                    // 2. Material current stock increment & cost update
                    material.CurrentStock += itemReq.AcceptedQuantity;
                    if (unitPrice > 0)
                    {
                        material.CurrentCost = unitPrice;
                    }
                    material.UpdatedAt = DateTime.UtcNow;
                    _repositoryManager.MaterialRepository.Update(material);

                    // 3. Central Inventory Transaction creation
                    var invTx = new InventoryTransaction
                    {
                        TransactionType = InventoryTransactionType.PurchaseReceipt,
                        TransactionDate = request.ReceiptDate,
                        WarehouseId = targetWhId,
                        DestinationLocationId = itemReq.LocationId,
                        MaterialId = itemReq.MaterialId,
                        BatchNumber = effectiveBatch,
                        Quantity = itemReq.AcceptedQuantity,
                        Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? material.Unit : itemReq.Unit,
                        UnitCost = unitPrice,
                        TotalCost = lineCost,
                        ReferenceDocumentNumber = receiptNumber,
                        UserId = userId,
                        Notes = $"استلام مشتريات بسند [{receiptNumber}] من المورد [{supplier.Name}] بموجب أمر الشراء [{po.OrderNumber}]".Trim(),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _repositoryManager.InventoryTransactionRepository.Create(invTx);
                    await _repositoryManager.SaveAsync();
                    invTransactionId = invTx.Id;

                    // 4. Record Supplier Price History
                    var priceHistory = new SupplierPriceHistory
                    {
                        SupplierId = request.SupplierId,
                        MaterialId = itemReq.MaterialId,
                        PurchaseDate = request.ReceiptDate,
                        UnitPrice = unitPrice,
                        Currency = po.Currency,
                        PurchaseOrderId = po.Id,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _repositoryManager.SupplierPriceHistoryRepository.Create(priceHistory);

                    // 5. Update PO Item received quantity
                    if (poItem != null)
                    {
                        poItem.ReceivedQuantity += itemReq.AcceptedQuantity;
                        poItem.UpdatedAt = DateTime.UtcNow;
                        _repositoryManager.PurchaseOrderRepository.Update(po);
                    }
                }

                var receiptItem = new PurchaseReceiptItem
                {
                    PurchaseOrderItemId = poItem?.Id,
                    MaterialId = itemReq.MaterialId,
                    OrderedQuantity = poItem?.OrderedQuantity ?? itemReq.OrderedQuantity,
                    ReceivedQuantity = itemReq.ReceivedQuantity,
                    AcceptedQuantity = itemReq.AcceptedQuantity,
                    RejectedQuantity = itemReq.RejectedQuantity,
                    Unit = string.IsNullOrWhiteSpace(itemReq.Unit) ? material.Unit : itemReq.Unit,
                    UnitPrice = unitPrice,
                    TotalCost = lineCost,
                    SupplierBatchNumber = itemReq.SupplierBatchNumber?.Trim() ?? string.Empty,
                    InternalBatchNumber = internalBatch,
                    ManufacturingDate = itemReq.ManufacturingDate,
                    ExpiryDate = itemReq.ExpiryDate,
                    WarehouseId = targetWhId,
                    LocationId = itemReq.LocationId,
                    InventoryTransactionId = invTransactionId,
                    Notes = itemReq.Notes,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                receipt.Items.Add(receiptItem);
                itemIndex++;
            }

            receipt.TotalCost = totalReceiptCost;
            _repositoryManager.PurchaseReceiptRepository.Create(receipt);

            // Update PO Status (PartiallyReceived vs FullyReceived)
            bool isAllFullyReceived = po.Items.All(i => i.ReceivedQuantity >= i.OrderedQuantity);
            if (isAllFullyReceived)
            {
                po.Status = PurchaseOrderStatus.FullyReceived;
            }
            else if (po.Items.Any(i => i.ReceivedQuantity > 0))
            {
                po.Status = PurchaseOrderStatus.PartiallyReceived;
            }
            po.UpdatedAt = DateTime.UtcNow;
            _repositoryManager.PurchaseOrderRepository.Update(po);

            await _repositoryManager.SaveAsync();
            await dbTransaction.CommitAsync();

            // Automatic Accounting Posting
            await _postingService.PostPurchaseReceiptAsync(receipt.Id, userId);

            return (await GetReceiptByIdAsync(receipt.Id))!;
        }
        catch
        {
            await dbTransaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PurchaseReceiptSummaryDto> GetSummaryAsync()
    {
        var receipts = await _repositoryManager.PurchaseReceiptRepository.GetAllReceiptsAsync();

        return new PurchaseReceiptSummaryDto
        {
            TotalReceipts = receipts.Count(),
            DraftReceipts = receipts.Count(r => r.Status == PurchaseReceiptStatus.Draft),
            PostedReceipts = receipts.Count(r => r.Status == PurchaseReceiptStatus.Posted),
            TotalReceivedValue = receipts.Where(r => r.Status == PurchaseReceiptStatus.Posted).Sum(r => r.TotalCost)
        };
    }
}
