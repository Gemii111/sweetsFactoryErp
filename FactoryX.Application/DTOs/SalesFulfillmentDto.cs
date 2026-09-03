using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class SalesFulfillmentDto
{
    public int Id { get; set; }
    public string FulfillmentNumber { get; set; } = string.Empty;
    public int SalesOrderId { get; set; }
    public string SalesOrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public int WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public DateTime FulfillmentDate { get; set; }
    public SalesFulfillmentStatus Status { get; set; }
    public string StatusName => Status switch
    {
        SalesFulfillmentStatus.Draft => "مسودة (Draft)",
        SalesFulfillmentStatus.Shipped => "تم الصرف والتسليم (Shipped)",
        SalesFulfillmentStatus.Cancelled => "ملغي (Cancelled)",
        _ => Status.ToString()
    };

    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }

    public int ShippedByUserId { get; set; }
    public string? ShippedByName { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<SalesFulfillmentItemDto> Items { get; set; } = new();
}

public class SalesFulfillmentItemDto
{
    public int Id { get; set; }
    public int SalesFulfillmentId { get; set; }
    public int? SalesOrderItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public int? FinishedGoodsStockId { get; set; }

    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ProductionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public int WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int? LocationId { get; set; }
    public string? LocationName { get; set; }

    public decimal OrderedQuantity { get; set; }
    public decimal ShippedQuantity { get; set; }
    public string Unit { get; set; } = "KG";

    public decimal UnitCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public int? InventoryTransactionId { get; set; }
    public string? Notes { get; set; }
}

public class CreateSalesFulfillmentRequest
{
    public int SalesOrderId { get; set; }
    public int CustomerId { get; set; }
    public int WarehouseId { get; set; }
    public DateTime FulfillmentDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public List<CreateSalesFulfillmentItemRequest> Items { get; set; } = new();
}

public class CreateSalesFulfillmentItemRequest
{
    public int? SalesOrderItemId { get; set; }
    public int ProductId { get; set; }
    public int? FinishedGoodsStockId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public DateTime? ProductionDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }

    public decimal OrderedQuantity { get; set; }
    public decimal ShippedQuantity { get; set; }
    public string Unit { get; set; } = "KG";

    public decimal UnitCost { get; set; }
    public decimal UnitPrice { get; set; }
    public string? Notes { get; set; }
}

public class SalesFulfillmentSummaryDto
{
    public int TotalFulfillments { get; set; }
    public int ShippedCount { get; set; }
    public decimal TotalShippedQuantity { get; set; }
    public decimal TotalShippedValue { get; set; }
}
