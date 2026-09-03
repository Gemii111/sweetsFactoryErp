using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum CustomerType
{
    Wholesale = 1,
    Retail = 2,
    Distributor = 3,
    Supermarket = 4,
    Corporate = 5,
    Other = 6
}

public enum SalesOrderStatus
{
    Draft = 1,
    Confirmed = 2,
    Reserved = 3,
    PartiallyFulfilled = 4,
    FullyFulfilled = 5,
    Cancelled = 6,
    Closed = 7
}

public enum SalesOrderPriority
{
    Normal = 1,
    High = 2,
    Urgent = 3
}

public enum SalesFulfillmentStatus
{
    Draft = 1,
    Shipped = 2,
    Cancelled = 3
}

public class Customer : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ArabicName { get; set; }
    public CustomerType Type { get; set; } = CustomerType.Wholesale;
    public string? ContactPerson { get; set; }
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? TaxNumber { get; set; }
    public string? Notes { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal CurrentBalance { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<SalesOrder>? SalesOrders { get; set; }
    public ICollection<SalesFulfillment>? SalesFulfillments { get; set; }
    public ICollection<Invoice>? Invoices { get; set; }
    public ICollection<Payment>? Payments { get; set; }
}

public class SalesOrder : EntityBase
{
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public int WarehouseId { get; set; } // Finished Goods Warehouse
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? RequiredDeliveryDate { get; set; }
    public SalesOrderStatus Status { get; set; } = SalesOrderStatus.Draft;
    public SalesOrderPriority Priority { get; set; } = SalesOrderPriority.Normal;

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Notes { get; set; }

    public int? ConfirmedByUserId { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    // Navigation Properties
    public Customer? Customer { get; set; }
    public Warehouse? Warehouse { get; set; }
    public User? ConfirmedByUser { get; set; }
    public ICollection<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();
    public ICollection<SalesFulfillment>? Fulfillments { get; set; }
    public ICollection<Invoice>? Invoices { get; set; }
}

public class SalesOrderItem : EntityBase
{
    public int SalesOrderId { get; set; }
    public int ProductId { get; set; }
    public string? BatchNumber { get; set; }

    public decimal OrderedQuantity { get; set; }
    public decimal FulfilledQuantity { get; set; }

    // Compatibility alias
    public decimal Quantity
    {
        get => OrderedQuantity;
        set => OrderedQuantity = value;
    }

    public string Unit { get; set; } = "KG";
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public SalesOrder? SalesOrder { get; set; }
    public Product? Product { get; set; }
    public ICollection<SalesFulfillmentItem>? FulfillmentItems { get; set; }
}

public class SalesFulfillment : EntityBase
{
    public string FulfillmentNumber { get; set; } = string.Empty; // e.g. SF-YYYYMMDD-XXXX
    public int SalesOrderId { get; set; }
    public int CustomerId { get; set; }
    public int WarehouseId { get; set; } // Finished Goods Warehouse
    public DateTime FulfillmentDate { get; set; } = DateTime.UtcNow;
    public SalesFulfillmentStatus Status { get; set; } = SalesFulfillmentStatus.Shipped;

    public decimal TotalQuantity { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalPrice { get; set; }

    public int ShippedByUserId { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public SalesOrder? SalesOrder { get; set; }
    public Customer? Customer { get; set; }
    public Warehouse? Warehouse { get; set; }
    public User? ShippedByUser { get; set; }
    public ICollection<SalesFulfillmentItem> Items { get; set; } = new List<SalesFulfillmentItem>();
    public ICollection<Invoice>? Invoices { get; set; }
}

public class SalesFulfillmentItem : EntityBase
{
    public int SalesFulfillmentId { get; set; }
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
    public decimal TotalCost { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public int? InventoryTransactionId { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public SalesFulfillment? SalesFulfillment { get; set; }
    public SalesOrderItem? SalesOrderItem { get; set; }
    public Product? Product { get; set; }
    public FinishedGoodsStock? FinishedGoodsStock { get; set; }
    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? Location { get; set; }
    public InventoryTransaction? InventoryTransaction { get; set; }
    public ICollection<InvoiceItem>? InvoiceItems { get; set; }
}
