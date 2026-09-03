using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class SalesOrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDeliveryDate { get; set; }
    public SalesOrderStatus Status { get; set; }
    public string StatusName => Status switch
    {
        SalesOrderStatus.Draft => "مسودة (Draft)",
        SalesOrderStatus.Confirmed => "معتمد (Confirmed)",
        SalesOrderStatus.Reserved => "محجوز (Reserved)",
        SalesOrderStatus.PartiallyFulfilled => "مستلم جزئياً (Partially Fulfilled)",
        SalesOrderStatus.FullyFulfilled => "مكتمل التسليم (Fully Fulfilled)",
        SalesOrderStatus.Cancelled => "ملغي (Cancelled)",
        SalesOrderStatus.Closed => "مغلق (Closed)",
        _ => Status.ToString()
    };

    public SalesOrderPriority Priority { get; set; }
    public string PriorityName => Priority switch
    {
        SalesOrderPriority.Normal => "عادي (Normal)",
        SalesOrderPriority.High => "مرتفع (High)",
        SalesOrderPriority.Urgent => "عاجل (Urgent)",
        _ => Priority.ToString()
    };

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public int? ConfirmedByUserId { get; set; }
    public string? ConfirmedByName { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public int FulfillmentsCount { get; set; }
    public decimal TotalOrderedQuantity => Items.Sum(i => i.OrderedQuantity);
    public decimal TotalFulfilledQuantity => Items.Sum(i => i.FulfilledQuantity);
    public decimal RemainingQuantity => Math.Max(0, TotalOrderedQuantity - TotalFulfilledQuantity);

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<SalesOrderItemDto> Items { get; set; } = new();
    public List<SalesFulfillmentDto> Fulfillments { get; set; } = new();
}

public class SalesOrderItemDto
{
    public int Id { get; set; }
    public int SalesOrderId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string? ProductSKU { get; set; }

    public decimal OrderedQuantity { get; set; }
    public decimal FulfilledQuantity { get; set; }
    public decimal RemainingQuantity => Math.Max(0, OrderedQuantity - FulfilledQuantity);

    public string Unit { get; set; } = "KG";
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? BatchNumber { get; set; }
    public string? Notes { get; set; }
}

public class CreateSalesOrderRequest
{
    public int CustomerId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? RequiredDeliveryDate { get; set; }
    public SalesOrderPriority Priority { get; set; } = SalesOrderPriority.Normal;
    public string? Notes { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public List<CreateSalesOrderItemRequest> Items { get; set; } = new();
}

public class CreateSalesOrderItemRequest
{
    public int ProductId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public string Unit { get; set; } = "KG";
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }
}

public class UpdateSalesOrderRequest
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDeliveryDate { get; set; }
    public SalesOrderPriority Priority { get; set; }
    public string? Notes { get; set; }

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public List<CreateSalesOrderItemRequest> Items { get; set; } = new();
}

public class SalesOrderSummaryDto
{
    public int TotalOrders { get; set; }
    public int DraftOrders { get; set; }
    public int ConfirmedOrders { get; set; }
    public int PartiallyFulfilledOrders { get; set; }
    public int FullyFulfilledOrders { get; set; }
    public decimal TotalOrderValue { get; set; }
}

public class SalesOrderFulfillmentInfoDto
{
    public int SalesOrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<SalesOrderFulfillmentItemInfoDto> Items { get; set; } = new();
}

public class SalesOrderFulfillmentItemInfoDto
{
    public int SalesOrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string Unit { get; set; } = "KG";
    public decimal UnitPrice { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal FulfilledQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
    public decimal AvailableStockQuantity { get; set; }
    public decimal Shortage => Math.Max(0, RemainingQuantity - AvailableStockQuantity);
    public List<BatchAvailabilityDto> AvailableBatches { get; set; } = new();
}

public class BatchAvailabilityDto
{
    public int FinishedGoodsStockId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ProductionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }
    public decimal AvailableQuantity { get; set; }
    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsExpired { get; set; }
}
