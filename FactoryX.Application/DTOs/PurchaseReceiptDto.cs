using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class PurchaseReceiptItemDto
{
    public int Id { get; set; }
    public int PurchaseReceiptId { get; set; }
    public int? PurchaseOrderItemId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }
    public string SupplierBatchNumber { get; set; } = string.Empty;
    public string InternalBatchNumber { get; set; } = string.Empty;
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public int? InventoryTransactionId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class PurchaseReceiptDto
{
    public int Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public int PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public DateTime ReceiptDate { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public PurchaseReceiptStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public int ReceivedByUserId { get; set; }
    public string? ReceivedByName { get; set; }
    public decimal TotalCost { get; set; }
    public string Notes { get; set; } = string.Empty;
    public decimal TotalReceivedQuantity => Items?.Sum(i => i.ReceivedQuantity) ?? 0m;
    public decimal TotalAcceptedQuantity => Items?.Sum(i => i.AcceptedQuantity) ?? 0m;
    public decimal TotalRejectedQuantity => Items?.Sum(i => i.RejectedQuantity) ?? 0m;
    public List<PurchaseReceiptItemDto> Items { get; set; } = new();
}

public class CreatePurchaseReceiptItemRequest
{
    public int? PurchaseOrderItemId { get; set; }
    public int MaterialId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string SupplierBatchNumber { get; set; } = string.Empty;
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreatePurchaseReceiptRequest
{
    public int PurchaseOrderId { get; set; }
    public int SupplierId { get; set; }
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
    public int WarehouseId { get; set; }
    public string Notes { get; set; } = string.Empty;
    public List<CreatePurchaseReceiptItemRequest> Items { get; set; } = new();
}

public class POReceivingInfoDto
{
    public int PurchaseOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string Currency { get; set; } = "EGP";
    public List<POReceivingItemInfoDto> Items { get; set; } = new();
}

public class POReceivingItemInfoDto
{
    public int PurchaseOrderItemId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public decimal OrderedQuantity { get; set; }
    public decimal AlreadyReceivedQuantity { get; set; }
    public decimal RemainingQuantity => Math.Max(0m, OrderedQuantity - AlreadyReceivedQuantity);
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public bool RequiresBatchNumber { get; set; } = true;
    public bool RequiresExpiryDate { get; set; } = true;
}

public class PurchaseReceiptSummaryDto
{
    public int TotalReceipts { get; set; }
    public int DraftReceipts { get; set; }
    public int PostedReceipts { get; set; }
    public decimal TotalReceivedValue { get; set; }
}
