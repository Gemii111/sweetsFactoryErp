using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum InvoiceStatus
{
    Draft = 1,          // مسودة
    Issued = 2,         // صادرة / غير مسددة
    PartiallyPaid = 3,  // مسددة جزئياً
    Paid = 4,           // مسددة بالكامل
    Cancelled = 5       // ملغاة
}

public enum PaymentMethod
{
    Cash = 1,          // نقداً
    BankTransfer = 2,  // تحويل بنكي
    Card = 3,          // بطاقة بنكية / POS
    Cheque = 4,        // شيك
    Other = 5          // أخرى
}

public enum PaymentStatus
{
    Recorded = 1,      // مثبت ومسجل
    Voided = 2         // ملغي / مسترد
}

public class Invoice : EntityBase
{
    public string InvoiceNumber { get; set; } = string.Empty; // INV-YYYYMMDD-XXXX
    public int CustomerId { get; set; }
    public int SalesOrderId { get; set; }
    public int? FulfillmentId { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public string Currency { get; set; } = "EGP";

    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; } = 14.00m; // Default VAT percentage
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    public string? Notes { get; set; }
    public int? CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }

    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledByUserId { get; set; }

    public byte[]? RowVersion { get; set; }

    // Navigation Properties
    public Customer? Customer { get; set; }
    public SalesOrder? SalesOrder { get; set; }
    public SalesFulfillment? SalesFulfillment { get; set; }
    public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public class InvoiceItem : EntityBase
{
    public int InvoiceId { get; set; }
    public int ProductId { get; set; }
    public int? SalesOrderItemId { get; set; }
    public int? SalesFulfillmentItemId { get; set; }

    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public Invoice? Invoice { get; set; }
    public Product? Product { get; set; }
    public SalesOrderItem? SalesOrderItem { get; set; }
    public SalesFulfillmentItem? SalesFulfillmentItem { get; set; }
}

public class Payment : EntityBase
{
    public string PaymentNumber { get; set; } = string.Empty; // PAY-YYYYMMDD-XXXX
    public int InvoiceId { get; set; }
    public int CustomerId { get; set; }

    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNumber { get; set; } // Cheque/Transfer Reference
    public PaymentStatus Status { get; set; } = PaymentStatus.Recorded;

    public string? Notes { get; set; }
    public int? ReceivedByUserId { get; set; }
    public string? ReceivedByName { get; set; }

    public string? VoidReason { get; set; }
    public DateTime? VoidedAt { get; set; }
    public int? VoidedByUserId { get; set; }

    // Navigation Properties
    public Invoice? Invoice { get; set; }
    public Customer? Customer { get; set; }
}
