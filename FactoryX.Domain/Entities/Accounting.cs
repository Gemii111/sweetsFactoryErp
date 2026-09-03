using FactoryX.Domain.Common;

namespace FactoryX.Domain.Entities;

public enum AccountType
{
    Asset = 1,        // أصول
    Liability = 2,    // التزامات
    Equity = 3,       // حقوق ملكية
    Revenue = 4,      // إيرادات
    Expense = 5       // مصروفات
}

public enum AccountingPeriodStatus
{
    Open = 1,         // مفتوحة
    Closed = 2        // مغلقة
}

public enum JournalEntryStatus
{
    Draft = 1,        // مسودة
    Posted = 2,       // مرحل
    Reversed = 3      // معكوس / ملغي بقيد عكسي
}

public enum JournalReferenceType
{
    Manual = 1,                 // قيد يدوي
    SalesInvoice = 2,           // فاتورة مبيعات
    CustomerPayment = 3,        // سند قبض عميل
    PurchaseReceipt = 4,        // سند استلام مشتريات (GRN)
    SupplierPayment = 5,        // سند صرف مورد
    SalesFulfillment = 6,       // تكلفة بضاعة مباعة (تسليم مبيعات)
    Waste = 7,                  // هالك وتوالف
    FinishedGoodsRelease = 8,   // إفراج إنتاج تام
    Reversal = 9                // قيد عكسي
}

public enum AccountRole
{
    None = 0,
    SalesRevenue = 1,           // إيرادات المبيعات
    AccountsReceivable = 2,     // مدينون / عملاء
    OutputVat = 3,              // ضريبة مخرجات (مبيعات)
    InputVat = 4,               // ضريبة مدخلات (مشتريات)
    AccountsPayable = 5,        // دائنون / موردون
    RawMaterialInventory = 6,   // مخزون مواد خام
    PackagingInventory = 7,     // مخزون مواد تعبئة وتغليف
    FinishedGoodsInventory = 8, // مخزون إنتاج تام
    CostOfGoodsSold = 9,        // تكلفة البضاعة المباعة (COGS)
    WasteExpense = 10,          // مصروف الهالك والتوالف
    Cash = 11,                  // الخزينة والنقدية
    Bank = 12,                  // البنك
    CardSettlement = 13,        // تسوية بطاقات بنكية
    ChequesReceivable = 14,     // شيكات تحت التحصيل
    ProductionClearing = 15     // وسيط تسوية الإنتاج
}

public class Account : EntityBase
{
    public string AccountCode { get; set; } = string.Empty; // e.g. "1101"
    public string AccountName { get; set; } = string.Empty; // e.g. "Cash on Hand"
    public string AccountNameAr { get; set; } = string.Empty; // e.g. "الخزينة الرئيسية"
    public AccountType AccountType { get; set; } = AccountType.Asset;
    
    public int? ParentAccountId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsControlAccount { get; set; } = false; // Header account vs Leaf/Posting account
    public AccountRole AccountRole { get; set; } = AccountRole.None;
    public string? Notes { get; set; }

    // Navigation Properties
    public Account? ParentAccount { get; set; }
    public ICollection<Account> ChildAccounts { get; set; } = new List<Account>();
    public ICollection<JournalEntryLine> JournalEntryLines { get; set; } = new List<JournalEntryLine>();
}

public class AccountingPeriod : EntityBase
{
    public string PeriodName { get; set; } = string.Empty; // e.g. "FY2026-M09"
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AccountingPeriodStatus Status { get; set; } = AccountingPeriodStatus.Open;
    
    public DateTime? ClosedAt { get; set; }
    public int? ClosedByUserId { get; set; }
    public string? Notes { get; set; }

    // Navigation Properties
    public User? ClosedByUser { get; set; }
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}

public class JournalEntry : EntityBase
{
    public string JournalNumber { get; set; } = string.Empty; // JE-YYYYMMDD-XXXX
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;
    public int AccountingPeriodId { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public JournalReferenceType ReferenceType { get; set; } = JournalReferenceType.Manual;
    public int? ReferenceId { get; set; }
    public string? ReferenceDocumentNumber { get; set; } // e.g. "INV-20260901-0001"
    
    public JournalEntryStatus Status { get; set; } = JournalEntryStatus.Posted;
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    public int? ReversalOfJournalEntryId { get; set; }
    public string? ReversalReason { get; set; }

    public int CreatedByUserId { get; set; }
    public int? PostedByUserId { get; set; }
    public DateTime? PostedAt { get; set; }

    // Navigation Properties
    public AccountingPeriod? AccountingPeriod { get; set; }
    public JournalEntry? ReversalOfJournalEntry { get; set; }
    public User? CreatedByUser { get; set; }
    public User? PostedByUser { get; set; }
    public ICollection<JournalEntryLine> Lines { get; set; } = new List<JournalEntryLine>();
}

public class JournalEntryLine : EntityBase
{
    public int JournalEntryId { get; set; }
    public int AccountId { get; set; }
    
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }

    // Subledger dimensions
    public int? CustomerId { get; set; }
    public int? SupplierId { get; set; }
    public int? ProductId { get; set; }
    public int? MaterialId { get; set; }
    public string? ReferenceNumber { get; set; }

    // Navigation Properties
    public JournalEntry? JournalEntry { get; set; }
    public Account? Account { get; set; }
    public Customer? Customer { get; set; }
    public Supplier? Supplier { get; set; }
    public Product? Product { get; set; }
    public Material? Material { get; set; }
}

public class SupplierPayment : EntityBase
{
    public string PaymentNumber { get; set; } = string.Empty; // SPAY-YYYYMMDD-XXXX
    public int SupplierId { get; set; }
    public int? PurchaseReceiptId { get; set; }
    public int? PurchaseOrderId { get; set; }
    
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Recorded;

    public int CreatedByUserId { get; set; }

    // Navigation Properties
    public Supplier? Supplier { get; set; }
    public PurchaseReceipt? PurchaseReceipt { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
    public User? CreatedByUser { get; set; }
}

public class AccountingSetting : EntityBase
{
    public AccountRole Role { get; set; }
    public int AccountId { get; set; }
    public string? Description { get; set; }

    // Navigation Properties
    public Account? Account { get; set; }
}
