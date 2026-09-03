using FactoryX.Domain.Entities;

namespace FactoryX.Application.DTOs;

#region Accounts DTOs
public class AccountDto
{
    public int Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public string AccountTypeName => AccountType switch
    {
        AccountType.Asset => "أصول (Assets)",
        AccountType.Liability => "التزامات (Liabilities)",
        AccountType.Equity => "حقوق ملكية (Equity)",
        AccountType.Revenue => "إيرادات (Revenue)",
        AccountType.Expense => "مصروفات (Expenses)",
        _ => AccountType.ToString()
    };
    public int? ParentAccountId { get; set; }
    public string? ParentAccountName { get; set; }
    public string? ParentAccountCode { get; set; }
    public bool IsActive { get; set; }
    public bool IsControlAccount { get; set; }
    public AccountRole AccountRole { get; set; }
    public string? Notes { get; set; }
    public int ChildCount { get; set; }
    public decimal CurrentBalance { get; set; }
}

public class AccountCreateDto
{
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public AccountType AccountType { get; set; } = AccountType.Asset;
    public int? ParentAccountId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsControlAccount { get; set; } = false;
    public AccountRole AccountRole { get; set; } = AccountRole.None;
    public string? Notes { get; set; }
}

public class AccountUpdateDto
{
    public int Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public int? ParentAccountId { get; set; }
    public bool IsActive { get; set; }
    public bool IsControlAccount { get; set; }
    public AccountRole AccountRole { get; set; }
    public string? Notes { get; set; }
}

public class AccountTreeNodeDto
{
    public int Id { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public bool IsControlAccount { get; set; }
    public bool IsActive { get; set; }
    public decimal Balance { get; set; }
    public List<AccountTreeNodeDto> Children { get; set; } = new List<AccountTreeNodeDto>();
}
#endregion

#region Accounting Periods DTOs
public class AccountingPeriodDto
{
    public int Id { get; set; }
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AccountingPeriodStatus Status { get; set; }
    public string StatusName => Status == AccountingPeriodStatus.Open ? "مفتوحة (Open)" : "مغلقة (Closed)";
    public DateTime? ClosedAt { get; set; }
    public int? ClosedByUserId { get; set; }
    public string? ClosedByName { get; set; }
    public string? Notes { get; set; }
    public int JournalCount { get; set; }
}

public class AccountingPeriodCreateDto
{
    public string PeriodName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
    public DateTime EndDate { get; set; } = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(1).AddDays(-1);
    public string? Notes { get; set; }
}

public class ClosePeriodDto
{
    public int PeriodId { get; set; }
    public string? Notes { get; set; }
}
#endregion

#region Journal Entries DTOs
public class JournalEntryDto
{
    public int Id { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public int AccountingPeriodId { get; set; }
    public string? PeriodName { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public JournalReferenceType ReferenceType { get; set; }
    public string ReferenceTypeName => ReferenceType switch
    {
        JournalReferenceType.Manual => "قيد يدوي",
        JournalReferenceType.SalesInvoice => "فاتورة مبيعات",
        JournalReferenceType.CustomerPayment => "سند قبض عميل",
        JournalReferenceType.PurchaseReceipt => "سند استلام مشتريات (GRN)",
        JournalReferenceType.SupplierPayment => "سند صرف مورد",
        JournalReferenceType.SalesFulfillment => "تكلفة بضاعة مباعة (COGS)",
        JournalReferenceType.Waste => "هالك وتوالف",
        JournalReferenceType.FinishedGoodsRelease => "إفراج إنتاج تام",
        JournalReferenceType.Reversal => "قيد عكسي",
        _ => ReferenceType.ToString()
    };
    public int? ReferenceId { get; set; }
    public string? ReferenceDocumentNumber { get; set; }
    
    public JournalEntryStatus Status { get; set; }
    public string StatusName => Status switch
    {
        JournalEntryStatus.Draft => "مسودة (Draft)",
        JournalEntryStatus.Posted => "مرحل (Posted)",
        JournalEntryStatus.Reversed => "معكوس (Reversed)",
        _ => Status.ToString()
    };
    
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }

    public int? ReversalOfJournalEntryId { get; set; }
    public string? ReversalOfJournalNumber { get; set; }
    public string? ReversalReason { get; set; }

    public int CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
    public int? PostedByUserId { get; set; }
    public string? PostedByName { get; set; }
    public DateTime? PostedAt { get; set; }

    public List<JournalEntryLineDto> Lines { get; set; } = new List<JournalEntryLineDto>();
}

public class JournalEntryLineDto
{
    public int Id { get; set; }
    public int JournalEntryId { get; set; }
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }

    public int? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public int? SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? ProductId { get; set; }
    public string? ProductName { get; set; }
    public int? MaterialId { get; set; }
    public string? MaterialName { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class JournalEntryCreateDto
{
    public DateTime EntryDate { get; set; } = DateTime.UtcNow.Date;
    public string Description { get; set; } = string.Empty;
    public string? ReferenceDocumentNumber { get; set; }
    public List<JournalEntryLineCreateDto> Lines { get; set; } = new List<JournalEntryLineCreateDto>();
}

public class JournalEntryLineCreateDto
{
    public int AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string? Description { get; set; }
    public int? CustomerId { get; set; }
    public int? SupplierId { get; set; }
    public int? ProductId { get; set; }
    public int? MaterialId { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class ReverseJournalDto
{
    public int JournalEntryId { get; set; }
    public string Reason { get; set; } = string.Empty;
}
#endregion

#region General Ledger & Trial Balance DTOs
public class GeneralLedgerQueryDto
{
    public int? AccountId { get; set; }
    public AccountType? AccountType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int? CustomerId { get; set; }
    public int? SupplierId { get; set; }
    public string? SearchTerm { get; set; }
}

public class GeneralLedgerAccountDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    
    public decimal OpeningBalance { get; set; }
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ClosingBalance { get; set; }

    public List<GeneralLedgerRowDto> Rows { get; set; } = new List<GeneralLedgerRowDto>();
}

public class GeneralLedgerRowDto
{
    public int JournalEntryId { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceDocumentNumber { get; set; }
    public JournalReferenceType ReferenceType { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal RunningBalance { get; set; }
    public string? CustomerName { get; set; }
    public string? SupplierName { get; set; }
}

public class TrialBalanceQueryDto
{
    public DateTime? AsOfDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime? FromDate { get; set; }
    public AccountType? AccountType { get; set; }
}

public class TrialBalanceDto
{
    public DateTime AsOfDate { get; set; }
    public DateTime? FromDate { get; set; }
    public decimal TotalOpeningDebit { get; set; }
    public decimal TotalOpeningCredit { get; set; }
    public decimal TotalPeriodDebit { get; set; }
    public decimal TotalPeriodCredit { get; set; }
    public decimal TotalClosingDebit { get; set; }
    public decimal TotalClosingCredit { get; set; }
    public bool IsBalanced => Math.Abs(TotalClosingDebit - TotalClosingCredit) < 0.01m;
    
    public List<TrialBalanceRowDto> Rows { get; set; } = new List<TrialBalanceRowDto>();
}

public class TrialBalanceRowDto
{
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public AccountType AccountType { get; set; }
    public bool IsControlAccount { get; set; }

    public decimal OpeningDebit { get; set; }
    public decimal OpeningCredit { get; set; }
    public decimal PeriodDebit { get; set; }
    public decimal PeriodCredit { get; set; }
    public decimal ClosingDebit { get; set; }
    public decimal ClosingCredit { get; set; }
    public decimal NetBalance => (ClosingDebit - ClosingCredit);
}
#endregion

#region Subledgers (Customer & Supplier) DTOs
public class CustomerLedgerDto
{
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal TotalInvoicedDebit { get; set; }
    public decimal TotalPaidCredit { get; set; }
    public decimal OutstandingReceivable { get; set; }
    public List<CustomerLedgerRowDto> Rows { get; set; } = new List<CustomerLedgerRowDto>();
}

public class CustomerLedgerRowDto
{
    public int JournalEntryId { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }  // Invoice Debt
    public decimal Credit { get; set; } // Payment Credit
    public decimal RunningBalance { get; set; }
}

public class SupplierLedgerDto
{
    public int SupplierId { get; set; }
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierCode { get; set; } = string.Empty;
    public decimal OpeningBalance { get; set; }
    public decimal TotalPurchaseCredit { get; set; }
    public decimal TotalPaymentDebit { get; set; }
    public decimal OutstandingPayable { get; set; }
    public List<SupplierLedgerRowDto> Rows { get; set; } = new List<SupplierLedgerRowDto>();
}

public class SupplierLedgerRowDto
{
    public int JournalEntryId { get; set; }
    public string JournalNumber { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Debit { get; set; }  // Payment to supplier (settlement)
    public decimal Credit { get; set; } // Purchase obligations (liability)
    public decimal RunningBalance { get; set; }
}
#endregion

#region Supplier Payments DTOs
public class SupplierPaymentDto
{
    public int Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public int SupplierId { get; set; }
    public string? SupplierName { get; set; }
    public int? PurchaseReceiptId { get; set; }
    public string? PurchaseReceiptNumber { get; set; }
    public int? PurchaseOrderId { get; set; }
    public string? PurchaseOrderNumber { get; set; }
    
    public DateTime PaymentDate { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName => PaymentMethod switch
    {
        PaymentMethod.Cash => "نقداً (Cash)",
        PaymentMethod.BankTransfer => "تحويل بنكي (Bank Transfer)",
        PaymentMethod.Card => "بطاقة بنكية (Card)",
        PaymentMethod.Cheque => "شيك (Cheque)",
        _ => PaymentMethod.ToString()
    };
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
    public PaymentStatus Status { get; set; }
    public string StatusName => Status == PaymentStatus.Recorded ? "مسدد ومثبت" : "ملغي";
    public int CreatedByUserId { get; set; }
    public string? CreatedByName { get; set; }
}

public class SupplierPaymentCreateDto
{
    public int SupplierId { get; set; }
    public int? PurchaseReceiptId { get; set; }
    public int? PurchaseOrderId { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow.Date;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;
    public string? ReferenceNumber { get; set; }
    public string? Notes { get; set; }
}
#endregion

#region Dashboard & Summary DTOs
public class AccountingDashboardDto
{
    public decimal TotalRevenue { get; set; }
    public decimal TotalCostOfGoodsSold { get; set; }
    public decimal GrossProfit => (TotalRevenue - TotalCostOfGoodsSold);
    public decimal TotalExpenses { get; set; }
    public decimal NetOperatingProfit => (GrossProfit - TotalExpenses);
    
    public decimal AccountsReceivableBalance { get; set; }
    public decimal AccountsPayableBalance { get; set; }
    public decimal TotalCashBalance { get; set; }
    public decimal TotalBankBalance { get; set; }
    public decimal TotalLiquidFunds => (TotalCashBalance + TotalBankBalance);
    
    public decimal OutputVatBalance { get; set; }
    public decimal InputVatBalance { get; set; }
    public decimal NetVatPayable => (OutputVatBalance - InputVatBalance);
    
    public decimal TotalInventoryValue { get; set; }
    public decimal RawMaterialInventoryValue { get; set; }
    public decimal PackagingInventoryValue { get; set; }
    public decimal FinishedGoodsInventoryValue { get; set; }

    public int OpenPeriodId { get; set; }
    public string OpenPeriodName { get; set; } = string.Empty;
    public int TotalPostedJournals { get; set; }

    public List<JournalEntryDto> RecentJournals { get; set; } = new List<JournalEntryDto>();
}

public class RevenueSummaryDto
{
    public decimal TotalSalesRevenue { get; set; }
    public decimal TotalDiscounts { get; set; }
    public decimal NetRevenue => (TotalSalesRevenue - TotalDiscounts);
    public int InvoicesCount { get; set; }
}

public class ExpenseSummaryDto
{
    public decimal CostOfGoodsSold { get; set; }
    public decimal WasteExpense { get; set; }
    public decimal OperatingExpenses { get; set; }
    public decimal TotalExpenses => (CostOfGoodsSold + WasteExpense + OperatingExpenses);
}

public class VatSummaryDto
{
    public decimal OutputVat { get; set; } // Sales VAT
    public decimal InputVat { get; set; }  // Purchases VAT
    public decimal NetVatPayable => (OutputVat - InputVat);
}

public class CashSummaryDto
{
    public decimal CashOnHand { get; set; }
    public decimal BankAccounts { get; set; }
    public decimal CardSettlement { get; set; }
    public decimal ChequesReceivable { get; set; }
    public decimal TotalLiquidity => (CashOnHand + BankAccounts + CardSettlement + ChequesReceivable);
}
#endregion

#region Settings / Mappings DTOs
public class AccountingSettingDto
{
    public int Id { get; set; }
    public AccountRole Role { get; set; }
    public string RoleName => Role switch
    {
        AccountRole.SalesRevenue => "إيرادات المبيعات (Sales Revenue)",
        AccountRole.AccountsReceivable => "المدينون / العملاء (Accounts Receivable)",
        AccountRole.OutputVat => "ضريبة مخرجات مبيعات (Output VAT)",
        AccountRole.InputVat => "ضريبة مدخلات مشتريات (Input VAT)",
        AccountRole.AccountsPayable => "الدائنون / الموردون (Accounts Payable)",
        AccountRole.RawMaterialInventory => "مخزون المواد الخام (Raw Materials)",
        AccountRole.PackagingInventory => "مخزون مواد التعبئة والتغليف (Packaging Materials)",
        AccountRole.FinishedGoodsInventory => "مخزون الإنتاج التام (Finished Goods)",
        AccountRole.CostOfGoodsSold => "تكلفة البضاعة المباعة (COGS)",
        AccountRole.WasteExpense => "مصروف الهالك والتوالف (Waste Expense)",
        AccountRole.Cash => "الخزينة النقدية (Cash on Hand)",
        AccountRole.Bank => "البنك الرئيسي (Main Bank)",
        AccountRole.CardSettlement => "تسوية البطاقات (Card Settlement)",
        AccountRole.ChequesReceivable => "شيكات تحت التحصيل (Cheques Receivable)",
        AccountRole.ProductionClearing => "وسيط تكاليف الإنتاج (Production Clearing)",
        _ => Role.ToString()
    };
    public int AccountId { get; set; }
    public string AccountCode { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AccountingSettingUpdateDto
{
    public AccountRole Role { get; set; }
    public int AccountId { get; set; }
}
#endregion
