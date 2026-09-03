using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

public class InvoiceDto
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;

    public int SalesOrderId { get; set; }
    public string SalesOrderNumber { get; set; } = string.Empty;

    public int? FulfillmentId { get; set; }
    public string? FulfillmentNumber { get; set; }

    public DateTime InvoiceDate { get; set; }
    public DateTime? DueDate { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusName => Status switch
    {
        InvoiceStatus.Draft => "مسودة (Draft)",
        InvoiceStatus.Issued => "صادرة (Issued)",
        InvoiceStatus.PartiallyPaid => "مسددة جزئياً (Partially Paid)",
        InvoiceStatus.Paid => "مسددة بالكامل (Paid)",
        InvoiceStatus.Cancelled => "ملغاة (Cancelled)",
        _ => Status.ToString()
    };

    public string Currency { get; set; } = "EGP";
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }

    public string? Notes { get; set; }
    public string? CreatedByName { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime? CancelledAt { get; set; }

    public List<InvoiceItemDto> Items { get; set; } = new();
    public List<PaymentDto> Payments { get; set; } = new();
}

public class InvoiceItemDto
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string? ProductSKU { get; set; }

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
}

public class CreateInvoiceRequest
{
    public int CustomerId { get; set; }
    public int SalesOrderId { get; set; }
    public int? FulfillmentId { get; set; }

    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public string Currency { get; set; } = "EGP";
    public decimal TaxRate { get; set; } = 14.00m;
    public string? Notes { get; set; }
    public bool IssueImmediately { get; set; } = true;

    public List<CreateInvoiceItemRequest> Items { get; set; } = new();
}

public class CreateInvoiceItemRequest
{
    public int ProductId { get; set; }
    public int? SalesOrderItemId { get; set; }
    public int? SalesFulfillmentItemId { get; set; }

    public string? Description { get; set; }
    public decimal Quantity { get; set; }
    public string Unit { get; set; } = "KG";
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; } = 14.00m;
    public decimal TaxAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}

public class InvoiceSummaryDto
{
    public int TotalInvoices { get; set; }
    public int DraftCount { get; set; }
    public int IssuedCount { get; set; }
    public int PartiallyPaidCount { get; set; }
    public int PaidCount { get; set; }
    public int CancelledCount { get; set; }

    public decimal TotalInvoicedAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
}

public class InvoiceableOrderDto
{
    public int SalesOrderId { get; set; }
    public string SalesOrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public List<InvoiceableItemDto> Items { get; set; } = new();
}

public class InvoiceableItemDto
{
    public int SalesOrderItemId { get; set; }
    public int? SalesFulfillmentItemId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string? ProductSKU { get; set; }

    public decimal OrderedQuantity { get; set; }
    public decimal FulfilledQuantity { get; set; }
    public decimal AlreadyInvoicedQuantity { get; set; }
    public decimal InvoiceableQuantity => Math.Max(0, FulfilledQuantity - AlreadyInvoicedQuantity);

    public string Unit { get; set; } = "KG";
    public decimal UnitPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxRate { get; set; } = 14.00m;
}

public class PaymentDto
{
    public int Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public int InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;

    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName => PaymentMethod switch
    {
        PaymentMethod.Cash => "نقداً (Cash)",
        PaymentMethod.BankTransfer => "تحويل بنكي (Bank Transfer)",
        PaymentMethod.Card => "بطاقة / POS (Card)",
        PaymentMethod.Cheque => "شيك بنكي (Cheque)",
        PaymentMethod.Other => "أخرى (Other)",
        _ => PaymentMethod.ToString()
    };

    public string? ReferenceNumber { get; set; }
    public PaymentStatus Status { get; set; }
    public string StatusName => Status switch
    {
        PaymentStatus.Recorded => "مثبت (Recorded)",
        PaymentStatus.Voided => "ملغي / مسترد (Voided)",
        _ => Status.ToString()
    };

    public string? Notes { get; set; }
    public string? ReceivedByName { get; set; }
    public string? VoidReason { get; set; }
    public DateTime? VoidedAt { get; set; }

    public decimal InvoiceTotalAmount { get; set; }
    public decimal InvoiceRemainingAmount { get; set; }
}

public class CreatePaymentRequest
{
    public int InvoiceId { get; set; }
    public int CustomerId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}

public class VoidPaymentRequest
{
    public int PaymentId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class PaymentSummaryDto
{
    public int TotalPayments { get; set; }
    public decimal TotalRecordedAmount { get; set; }
    public decimal TotalVoidedAmount { get; set; }
    public decimal CashAmount { get; set; }
    public decimal BankTransferAmount { get; set; }
    public decimal CardAmount { get; set; }
    public decimal ChequeAmount { get; set; }
    public decimal OtherAmount { get; set; }
}

public class CustomerStatementDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public decimal CreditLimit { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public decimal OpeningBalance { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal ClosingBalance { get; set; }

    public List<CustomerStatementLineDto> Lines { get; set; } = new();
}

public class CustomerStatementLineDto
{
    public DateTime Date { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty; // فاتورة بيع / سند قبض
    public string? Description { get; set; }
    public string? Reference { get; set; }
    public decimal DebitAmount { get; set; }  // قيمة الفاتورة (زاد الدين)
    public decimal CreditAmount { get; set; } // قيمة السداد (قل الدين)
    public decimal RunningBalance { get; set; }
}

public class CustomerBalanceSummaryDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerType { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public decimal CreditLimit { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal OutstandingBalance => TotalInvoiced - TotalPaid;
    public int InvoicesCount { get; set; }
    public int PaymentsCount { get; set; }
}
