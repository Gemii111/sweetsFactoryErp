using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class PurchaseOrderItemDto
{
    public int Id { get; set; }
    public int PurchaseOrderId { get; set; }
    public int MaterialId { get; set; }
    public string MaterialName { get; set; } = string.Empty;
    public string MaterialCode { get; set; } = string.Empty;
    public string? MaterialSKU { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal RemainingQuantity => Math.Max(0m, OrderedQuantity - ReceivedQuantity);
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class PurchaseOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public int? PurchaseRequestId { get; set; }
    public string? PurchaseRequestNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public PurchaseOrderStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string Currency { get; set; } = "EGP";
    public decimal TotalBeforeTax { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Notes { get; set; } = string.Empty;
    public int ReceiptsCount { get; set; }
    public decimal TotalOrderedQuantity => Items?.Sum(i => i.OrderedQuantity) ?? 0m;
    public decimal TotalReceivedQuantity => Items?.Sum(i => i.ReceivedQuantity) ?? 0m;
    public decimal TotalRemainingQuantity => Math.Max(0m, TotalOrderedQuantity - TotalReceivedQuantity);
    public List<PurchaseOrderItemDto> Items { get; set; } = new();
}

public class CreatePurchaseOrderItemRequest
{
    public int MaterialId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class CreatePurchaseOrderRequest
{
    public int SupplierId { get; set; }
    public int? PurchaseRequestId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public int WarehouseId { get; set; }
    public string Currency { get; set; } = "EGP";
    public string Notes { get; set; } = string.Empty;
    public List<CreatePurchaseOrderItemRequest> Items { get; set; } = new();
}

public class UpdatePurchaseOrderRequest : CreatePurchaseOrderRequest
{
    public int Id { get; set; }
}

public class PurchaseOrderSummaryDto
{
    public int TotalOrders { get; set; }
    public int DraftOrders { get; set; }
    public int ApprovedOrders { get; set; }
    public int PartiallyReceivedOrders { get; set; }
    public int FullyReceivedOrders { get; set; }
    public decimal TotalPurchasingValue { get; set; }
}
