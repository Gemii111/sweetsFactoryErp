using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public class SupplierCategory : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<Supplier>? Suppliers { get; set; }
}

public class Supplier : EntityBase
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Mobile { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string TaxNumber { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public string Notes { get; set; } = string.Empty;

    public SupplierCategory? Category { get; set; }
    public ICollection<PurchaseOrder>? PurchaseOrders { get; set; }
    public ICollection<PurchaseReceipt>? PurchaseReceipts { get; set; }
    public ICollection<SupplierPriceHistory>? PriceHistories { get; set; }
}

public enum PurchaseRequestStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    Rejected = 4,
    Cancelled = 5
}

public class PurchaseRequest : EntityBase
{
    public string RequestNumber { get; set; } = string.Empty; // PR-YYYYMMDD-XXXX
    public DateTime RequestDate { get; set; } = DateTime.UtcNow;
    public DateTime? RequiredDate { get; set; }
    public int? DepartmentId { get; set; }
    public string Priority { get; set; } = "Normal"; // Normal, High, Urgent
    public PurchaseRequestStatus Status { get; set; } = PurchaseRequestStatus.Draft;
    
    public int RequestedByUserId { get; set; }
    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Notes { get; set; } = string.Empty;

    public Department? Department { get; set; }
    public User? RequestedByUser { get; set; }
    public User? ApprovedByUser { get; set; }
    public ICollection<PurchaseRequestItem> Items { get; set; } = new List<PurchaseRequestItem>();
    public ICollection<PurchaseOrder>? PurchaseOrders { get; set; }
}

public class PurchaseRequestItem : EntityBase
{
    public int PurchaseRequestId { get; set; }
    public int MaterialId { get; set; }
    public decimal RequestedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal EstimatedUnitPrice { get; set; }
    public DateTime? RequiredDate { get; set; }
    public string Notes { get; set; } = string.Empty;

    public PurchaseRequest? PurchaseRequest { get; set; }
    public Material? Material { get; set; }
}

public enum PurchaseOrderStatus
{
    Draft = 1,
    Submitted = 2,
    Approved = 3,
    PartiallyReceived = 4,
    FullyReceived = 5,
    Cancelled = 6,
    Closed = 7
}

public class PurchaseOrder : EntityBase
{
    public string OrderNumber { get; set; } = string.Empty; // PO-YYYYMMDD-XXXX
    public int SupplierId { get; set; }
    public int? PurchaseRequestId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public int WarehouseId { get; set; }
    
    public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;
    public string Currency { get; set; } = "EGP";
    
    public decimal TotalBeforeTax { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }

    public int? ApprovedByUserId { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Notes { get; set; } = string.Empty;

    public Supplier? Supplier { get; set; }
    public PurchaseRequest? PurchaseRequest { get; set; }
    public Warehouse? Warehouse { get; set; }
    public User? ApprovedByUser { get; set; }
    public ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    public ICollection<PurchaseReceipt>? Receipts { get; set; }
}

public class PurchaseOrderItem : EntityBase
{
    public int PurchaseOrderId { get; set; }
    public int MaterialId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Notes { get; set; } = string.Empty;

    public PurchaseOrder? PurchaseOrder { get; set; }
    public Material? Material { get; set; }
    public ICollection<PurchaseReceiptItem>? ReceiptItems { get; set; }
}

public enum PurchaseReceiptStatus
{
    Draft = 1,
    Posted = 2,
    Cancelled = 3
}

public class PurchaseReceipt : EntityBase
{
    public string ReceiptNumber { get; set; } = string.Empty; // GRN-YYYYMMDD-XXXX
    public int PurchaseOrderId { get; set; }
    public int SupplierId { get; set; }
    public DateTime ReceiptDate { get; set; } = DateTime.UtcNow;
    public int WarehouseId { get; set; }
    
    public PurchaseReceiptStatus Status { get; set; } = PurchaseReceiptStatus.Draft;
    public int ReceivedByUserId { get; set; }
    public decimal TotalCost { get; set; }
    public string Notes { get; set; } = string.Empty;

    public PurchaseOrder? PurchaseOrder { get; set; }
    public Supplier? Supplier { get; set; }
    public Warehouse? Warehouse { get; set; }
    public User? ReceivedByUser { get; set; }
    public ICollection<PurchaseReceiptItem> Items { get; set; } = new List<PurchaseReceiptItem>();
}

public class PurchaseReceiptItem : EntityBase
{
    public int PurchaseReceiptId { get; set; }
    public int? PurchaseOrderItemId { get; set; }
    public int MaterialId { get; set; }
    public decimal OrderedQuantity { get; set; }
    public decimal ReceivedQuantity { get; set; }
    public decimal AcceptedQuantity { get; set; }
    public decimal RejectedQuantity { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public decimal TotalCost { get; set; }

    public string SupplierBatchNumber { get; set; } = string.Empty;
    public string InternalBatchNumber { get; set; } = string.Empty; // INT-GRN-YYYYMMDD-XXXX
    public DateTime? ManufacturingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public int WarehouseId { get; set; }
    public int? LocationId { get; set; }
    public int? InventoryTransactionId { get; set; }
    public string Notes { get; set; } = string.Empty;

    public PurchaseReceipt? PurchaseReceipt { get; set; }
    public PurchaseOrderItem? PurchaseOrderItem { get; set; }
    public Material? Material { get; set; }
    public Warehouse? Warehouse { get; set; }
    public WarehouseLocation? Location { get; set; }
    public InventoryTransaction? InventoryTransaction { get; set; }
}

public class SupplierPriceHistory : EntityBase
{
    public int SupplierId { get; set; }
    public int MaterialId { get; set; }
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow;
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "EGP";
    public int? PurchaseOrderId { get; set; }
    public int? PurchaseReceiptId { get; set; }

    public Supplier? Supplier { get; set; }
    public Material? Material { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public PurchaseReceipt? PurchaseReceipt { get; set; }
}
