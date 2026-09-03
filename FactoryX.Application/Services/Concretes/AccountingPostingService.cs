using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class AccountingPostingService : IAccountingPostingService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IAccountService _accountService;
    private readonly IAccountingPeriodService _periodService;

    public AccountingPostingService(
        IRepositoryManager repositoryManager,
        IAccountService accountService,
        IAccountingPeriodService periodService)
    {
        _repositoryManager = repositoryManager;
        _accountService = accountService;
        _periodService = periodService;
    }

    private async Task<int> GetRequiredAccountIdAsync(AccountRole role)
    {
        await _accountService.SeedDefaultChartOfAccountsAsync();
        var accountId = await _repositoryManager.AccountingSettingRepository.GetAccountIdByRoleAsync(role);
        if (!accountId.HasValue || accountId.Value <= 0)
        {
            throw new InvalidOperationException($"لم يتم ضبط الحساب المالي المرتبط بالدور المحاسبي '{role}'. يرجى مراجعة إعدادات شجرة الحسابات.");
        }
        return accountId.Value;
    }

    private async Task<AccountingPeriod> GetOpenPeriodForDateAsync(DateTime date)
    {
        await _periodService.EnsureOpenPeriodExistsAsync();
        var period = await _repositoryManager.AccountingPeriodRepository.GetPeriodForDateAsync(date);
        if (period == null || period.Status == AccountingPeriodStatus.Closed)
        {
            throw new InvalidOperationException($"لا يمكن إنشاء قيد محاسبي آلي في فترة مالية مغلقة أو غير محددة لتاريخ '{date:yyyy-MM-dd}'.");
        }
        return period;
    }

    public async Task<JournalEntry?> PostSalesInvoiceAsync(int invoiceId, int userId)
    {
        var existing = await _repositoryManager.JournalEntryRepository
            .GetByReferenceAsync(JournalReferenceType.SalesInvoice, invoiceId);
        if (existing != null) return existing;

        var invoice = await _repositoryManager.InvoiceRepository.GetByIdWithDetailsAsync(invoiceId);
        if (invoice == null || invoice.Status == InvoiceStatus.Draft || invoice.Status == InvoiceStatus.Cancelled)
        {
            return null;
        }

        var period = await GetOpenPeriodForDateAsync(invoice.InvoiceDate);
        var arAccountId = await GetRequiredAccountIdAsync(AccountRole.AccountsReceivable);
        var revenueAccountId = await GetRequiredAccountIdAsync(AccountRole.SalesRevenue);
        var vatAccountId = await GetRequiredAccountIdAsync(AccountRole.OutputVat);

        var netRevenue = Math.Max(0, invoice.SubTotal - invoice.DiscountAmount);
        var totalDebit = invoice.TotalAmount;
        var totalCredit = netRevenue + invoice.TaxAmount;

        // Ensure rounding balance
        if (Math.Abs(totalDebit - totalCredit) > 0.01m)
        {
            totalCredit = totalDebit;
        }

        var journalNumber = await _repositoryManager.JournalEntryRepository.GenerateNextJournalNumberAsync(invoice.InvoiceDate);
        var customerName = invoice.Customer?.Name ?? $"عميل #{invoice.CustomerId}";

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            EntryDate = invoice.InvoiceDate.Date,
            AccountingPeriodId = period.Id,
            Description = $"فاتورة مبيعات رقم [{invoice.InvoiceNumber}] - {customerName}",
            ReferenceType = JournalReferenceType.SalesInvoice,
            ReferenceId = invoice.Id,
            ReferenceDocumentNumber = invoice.InvoiceNumber,
            Status = JournalEntryStatus.Posted,
            TotalDebit = totalDebit,
            TotalCredit = totalDebit,
            CreatedByUserId = userId,
            PostedByUserId = userId,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // 1. Dr Accounts Receivable (Total Invoice Amount)
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = arAccountId,
            Debit = totalDebit,
            Credit = 0,
            Description = $"استحقاق فاتورة مبيعات رقم {invoice.InvoiceNumber} على العميل {customerName}",
            CustomerId = invoice.CustomerId,
            ReferenceNumber = invoice.InvoiceNumber
        });

        // 2. Cr Sales Revenue (Net Revenue)
        if (netRevenue > 0)
        {
            journal.Lines.Add(new JournalEntryLine
            {
                AccountId = revenueAccountId,
                Debit = 0,
                Credit = netRevenue,
                Description = $"إيرادات مبيعات فاتورة رقم {invoice.InvoiceNumber}",
                CustomerId = invoice.CustomerId,
                ReferenceNumber = invoice.InvoiceNumber
            });
        }

        // 3. Cr Output VAT (Tax Amount)
        if (invoice.TaxAmount > 0)
        {
            journal.Lines.Add(new JournalEntryLine
            {
                AccountId = vatAccountId,
                Debit = 0,
                Credit = invoice.TaxAmount,
                Description = $"ضريبة القيمة المضافة (14%) على فاتورة مبيعات رقم {invoice.InvoiceNumber}",
                CustomerId = invoice.CustomerId,
                ReferenceNumber = invoice.InvoiceNumber
            });
        }

        _repositoryManager.JournalEntryRepository.Create(journal);
        await _repositoryManager.SaveAsync();

        return journal;
    }

    public async Task<JournalEntry?> PostCustomerPaymentAsync(int paymentId, int userId)
    {
        var existing = await _repositoryManager.JournalEntryRepository
            .GetByReferenceAsync(JournalReferenceType.CustomerPayment, paymentId);
        if (existing != null) return existing;

        var payment = await _repositoryManager.PaymentRepository.GetByIdWithDetailsAsync(paymentId);
        if (payment == null || payment.Status == PaymentStatus.Voided || payment.Amount <= 0)
        {
            return null;
        }

        var period = await GetOpenPeriodForDateAsync(payment.PaymentDate);
        var arAccountId = await GetRequiredAccountIdAsync(AccountRole.AccountsReceivable);

        var cashRole = payment.PaymentMethod switch
        {
            PaymentMethod.BankTransfer => AccountRole.Bank,
            PaymentMethod.Card => AccountRole.CardSettlement,
            PaymentMethod.Cheque => AccountRole.ChequesReceivable,
            _ => AccountRole.Cash
        };

        var liquidAccountId = await GetRequiredAccountIdAsync(cashRole);
        var customerName = payment.Customer?.Name ?? $"عميل #{payment.CustomerId}";
        var journalNumber = await _repositoryManager.JournalEntryRepository.GenerateNextJournalNumberAsync(payment.PaymentDate);

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            EntryDate = payment.PaymentDate.Date,
            AccountingPeriodId = period.Id,
            Description = $"سند تحصيل وقبض رقم [{payment.PaymentNumber}] من العميل {customerName}",
            ReferenceType = JournalReferenceType.CustomerPayment,
            ReferenceId = payment.Id,
            ReferenceDocumentNumber = payment.PaymentNumber,
            Status = JournalEntryStatus.Posted,
            TotalDebit = payment.Amount,
            TotalCredit = payment.Amount,
            CreatedByUserId = userId,
            PostedByUserId = userId,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // 1. Dr Cash / Bank / Card
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = liquidAccountId,
            Debit = payment.Amount,
            Credit = 0,
            Description = $"استلام دفعة نقدية/بنكية بموجب سند {payment.PaymentNumber} (طريقة الدفع: {payment.PaymentMethod})",
            CustomerId = payment.CustomerId,
            ReferenceNumber = payment.PaymentNumber
        });

        // 2. Cr Accounts Receivable
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = arAccountId,
            Debit = 0,
            Credit = payment.Amount,
            Description = $"تسوية رصيد عميل بموجب سند تحصيل رقم {payment.PaymentNumber}",
            CustomerId = payment.CustomerId,
            ReferenceNumber = payment.PaymentNumber
        });

        _repositoryManager.JournalEntryRepository.Create(journal);
        await _repositoryManager.SaveAsync();

        return journal;
    }

    public async Task<JournalEntry?> PostPurchaseReceiptAsync(int receiptId, int userId)
    {
        var existing = await _repositoryManager.JournalEntryRepository
            .GetByReferenceAsync(JournalReferenceType.PurchaseReceipt, receiptId);
        if (existing != null) return existing;

        var receipt = await _repositoryManager.PurchaseReceiptRepository.GetByIdWithDetailsAsync(receiptId);
        if (receipt == null || receipt.Status != PurchaseReceiptStatus.Posted || receipt.TotalCost <= 0)
        {
            return null;
        }

        var period = await GetOpenPeriodForDateAsync(receipt.ReceiptDate);
        var apAccountId = await GetRequiredAccountIdAsync(AccountRole.AccountsPayable);
        var rawMatInventoryAccountId = await GetRequiredAccountIdAsync(AccountRole.RawMaterialInventory);
        var packagingInventoryAccountId = await GetRequiredAccountIdAsync(AccountRole.PackagingInventory);

        var totalCost = receipt.TotalCost;
        var supplierName = receipt.Supplier?.Name ?? $"مورد #{receipt.SupplierId}";
        var journalNumber = await _repositoryManager.JournalEntryRepository.GenerateNextJournalNumberAsync(receipt.ReceiptDate);

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            EntryDate = receipt.ReceiptDate.Date,
            AccountingPeriodId = period.Id,
            Description = $"سند استلام بضاعة ومشتريات (GRN) رقم [{receipt.ReceiptNumber}] من المورد {supplierName}",
            ReferenceType = JournalReferenceType.PurchaseReceipt,
            ReferenceId = receipt.Id,
            ReferenceDocumentNumber = receipt.ReceiptNumber,
            Status = JournalEntryStatus.Posted,
            TotalDebit = totalCost,
            TotalCredit = totalCost,
            CreatedByUserId = userId,
            PostedByUserId = userId,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // 1. Dr Raw Materials / Packaging Inventory based on items
        decimal rawCost = 0;
        decimal pkgCost = 0;

        foreach (var item in receipt.Items)
        {
            if (item.AcceptedQuantity <= 0) continue;
            var itemCost = item.AcceptedQuantity * item.UnitPrice;
            
            // Check if packaging
            if (item.Material?.IsPackagingMaterial == true)
            {
                pkgCost += itemCost;
            }
            else
            {
                rawCost += itemCost;
            }
        }

        if (rawCost == 0 && pkgCost == 0)
        {
            rawCost = totalCost;
        }

        if (rawCost > 0)
        {
            journal.Lines.Add(new JournalEntryLine
            {
                AccountId = rawMatInventoryAccountId,
                Debit = rawCost,
                Credit = 0,
                Description = $"إضافة مخزون مواد خام مستلمة بموجب سند {receipt.ReceiptNumber}",
                SupplierId = receipt.SupplierId,
                ReferenceNumber = receipt.ReceiptNumber
            });
        }

        if (pkgCost > 0)
        {
            journal.Lines.Add(new JournalEntryLine
            {
                AccountId = packagingInventoryAccountId,
                Debit = pkgCost,
                Credit = 0,
                Description = $"إضافة مخزون مواد تعبئة وتغليف مستلمة بموجب سند {receipt.ReceiptNumber}",
                SupplierId = receipt.SupplierId,
                ReferenceNumber = receipt.ReceiptNumber
            });
        }

        // 2. Cr Accounts Payable (Supplier)
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = apAccountId,
            Debit = 0,
            Credit = totalCost,
            Description = $"استحقاق التزام للمورد {supplierName} بموجب سند استلام مشتريات {receipt.ReceiptNumber}",
            SupplierId = receipt.SupplierId,
            ReferenceNumber = receipt.ReceiptNumber
        });

        _repositoryManager.JournalEntryRepository.Create(journal);
        await _repositoryManager.SaveAsync();

        return journal;
    }

    public async Task<JournalEntry?> PostSupplierPaymentAsync(int supplierPaymentId, int userId)
    {
        var existing = await _repositoryManager.JournalEntryRepository
            .GetByReferenceAsync(JournalReferenceType.SupplierPayment, supplierPaymentId);
        if (existing != null) return existing;

        var payment = await _repositoryManager.SupplierPaymentRepository.GetWithDetailsAsync(supplierPaymentId);
        if (payment == null || payment.Status == PaymentStatus.Voided || payment.Amount <= 0)
        {
            return null;
        }

        var period = await GetOpenPeriodForDateAsync(payment.PaymentDate);
        var apAccountId = await GetRequiredAccountIdAsync(AccountRole.AccountsPayable);

        var cashRole = payment.PaymentMethod switch
        {
            PaymentMethod.BankTransfer => AccountRole.Bank,
            PaymentMethod.Card => AccountRole.CardSettlement,
            PaymentMethod.Cheque => AccountRole.ChequesReceivable,
            _ => AccountRole.Cash
        };

        var liquidAccountId = await GetRequiredAccountIdAsync(cashRole);
        var supplierName = payment.Supplier?.Name ?? $"مورد #{payment.SupplierId}";
        var journalNumber = await _repositoryManager.JournalEntryRepository.GenerateNextJournalNumberAsync(payment.PaymentDate);

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            EntryDate = payment.PaymentDate.Date,
            AccountingPeriodId = period.Id,
            Description = $"سند سداد وصرف لمورد رقم [{payment.PaymentNumber}] إلى {supplierName}",
            ReferenceType = JournalReferenceType.SupplierPayment,
            ReferenceId = payment.Id,
            ReferenceDocumentNumber = payment.PaymentNumber,
            Status = JournalEntryStatus.Posted,
            TotalDebit = payment.Amount,
            TotalCredit = payment.Amount,
            CreatedByUserId = userId,
            PostedByUserId = userId,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // 1. Dr Accounts Payable (Supplier Settlement)
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = apAccountId,
            Debit = payment.Amount,
            Credit = 0,
            Description = $"سداد وسداد مستحقات المورد {supplierName} بموجب سند {payment.PaymentNumber}",
            SupplierId = payment.SupplierId,
            ReferenceNumber = payment.PaymentNumber
        });

        // 2. Cr Cash / Bank
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = liquidAccountId,
            Debit = 0,
            Credit = payment.Amount,
            Description = $"صرف نقدية/تحويل بنكي لسداد مورد بموجب سند {payment.PaymentNumber}",
            SupplierId = payment.SupplierId,
            ReferenceNumber = payment.PaymentNumber
        });

        _repositoryManager.JournalEntryRepository.Create(journal);
        await _repositoryManager.SaveAsync();

        return journal;
    }

    public async Task<JournalEntry?> PostSalesFulfillmentAsync(int fulfillmentId, int userId)
    {
        var existing = await _repositoryManager.JournalEntryRepository
            .GetByReferenceAsync(JournalReferenceType.SalesFulfillment, fulfillmentId);
        if (existing != null) return existing;

        var fulfillment = await _repositoryManager.SalesFulfillmentRepository.GetByIdWithDetailsAsync(fulfillmentId);
        if (fulfillment == null || fulfillment.Status != SalesFulfillmentStatus.Shipped || fulfillment.TotalCost <= 0)
        {
            return null;
        }

        var period = await GetOpenPeriodForDateAsync(fulfillment.FulfillmentDate);
        var cogsAccountId = await GetRequiredAccountIdAsync(AccountRole.CostOfGoodsSold);
        var fgInventoryAccountId = await GetRequiredAccountIdAsync(AccountRole.FinishedGoodsInventory);

        var totalCost = fulfillment.TotalCost;
        var journalNumber = await _repositoryManager.JournalEntryRepository.GenerateNextJournalNumberAsync(fulfillment.FulfillmentDate);

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            EntryDate = fulfillment.FulfillmentDate.Date,
            AccountingPeriodId = period.Id,
            Description = $"إثبات تكلفة البضاعة المباعة (COGS) لشحنة تسليم مبيعات [{fulfillment.FulfillmentNumber}]",
            ReferenceType = JournalReferenceType.SalesFulfillment,
            ReferenceId = fulfillment.Id,
            ReferenceDocumentNumber = fulfillment.FulfillmentNumber,
            Status = JournalEntryStatus.Posted,
            TotalDebit = totalCost,
            TotalCredit = totalCost,
            CreatedByUserId = userId,
            PostedByUserId = userId,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // 1. Dr Cost of Goods Sold (COGS)
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = cogsAccountId,
            Debit = totalCost,
            Credit = 0,
            Description = $"تكلفة مبيعات منتجات تامة مسلمة بموجب شحنة {fulfillment.FulfillmentNumber}",
            CustomerId = fulfillment.CustomerId,
            ReferenceNumber = fulfillment.FulfillmentNumber
        });

        // 2. Cr Finished Goods Inventory
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = fgInventoryAccountId,
            Debit = 0,
            Credit = totalCost,
            Description = $"صرف مخزون إنتاج تام للعميل بموجب شحنة تسليم {fulfillment.FulfillmentNumber}",
            CustomerId = fulfillment.CustomerId,
            ReferenceNumber = fulfillment.FulfillmentNumber
        });

        _repositoryManager.JournalEntryRepository.Create(journal);
        await _repositoryManager.SaveAsync();

        return journal;
    }

    public async Task<JournalEntry?> PostWasteAsync(int wasteId, int userId)
    {
        var existing = await _repositoryManager.JournalEntryRepository
            .GetByReferenceAsync(JournalReferenceType.Waste, wasteId);
        if (existing != null) return existing;

        var waste = await _repositoryManager.WasteRepository.GetWasteWithDetailsAsync(wasteId);
        if (waste == null || waste.Status != WasteStatus.Approved || waste.TotalCost <= 0)
        {
            return null;
        }

        var period = await GetOpenPeriodForDateAsync(waste.WasteDate);
        var wasteExpenseAccountId = await GetRequiredAccountIdAsync(AccountRole.WasteExpense);
        
        // Select inventory credit account based on WasteType / linkages
        var inventoryAccountId = (waste.ProductId.HasValue && waste.ProductId > 0)
            ? await GetRequiredAccountIdAsync(AccountRole.FinishedGoodsInventory)
            : await GetRequiredAccountIdAsync(AccountRole.RawMaterialInventory);

        var totalCost = waste.TotalCost;
        var journalNumber = await _repositoryManager.JournalEntryRepository.GenerateNextJournalNumberAsync(waste.WasteDate);

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            EntryDate = waste.WasteDate.Date,
            AccountingPeriodId = period.Id,
            Description = $"إثبات خسائر هالك وتوالف معتمد رقم [{waste.WasteNumber}] - {waste.ReasonDescription}",
            ReferenceType = JournalReferenceType.Waste,
            ReferenceId = waste.Id,
            ReferenceDocumentNumber = waste.WasteNumber,
            Status = JournalEntryStatus.Posted,
            TotalDebit = totalCost,
            TotalCredit = totalCost,
            CreatedByUserId = userId,
            PostedByUserId = userId,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // 1. Dr Waste Expense
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = wasteExpenseAccountId,
            Debit = totalCost,
            Credit = 0,
            Description = $"مصروف هالك وتوالف لسجل رقم {waste.WasteNumber}",
            ProductId = waste.ProductId,
            MaterialId = waste.MaterialId,
            ReferenceNumber = waste.WasteNumber
        });

        // 2. Cr Inventory
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = inventoryAccountId,
            Debit = 0,
            Credit = totalCost,
            Description = $"تخفيض المخزون نتيجة تلف وهالك معتمد بسجل {waste.WasteNumber}",
            ProductId = waste.ProductId,
            MaterialId = waste.MaterialId,
            ReferenceNumber = waste.WasteNumber
        });

        _repositoryManager.JournalEntryRepository.Create(journal);
        await _repositoryManager.SaveAsync();

        return journal;
    }

    public async Task<JournalEntry?> PostFinishedGoodsReleaseAsync(int releaseId, int userId)
    {
        var existing = await _repositoryManager.JournalEntryRepository
            .GetByReferenceAsync(JournalReferenceType.FinishedGoodsRelease, releaseId);
        if (existing != null) return existing;

        var release = await _repositoryManager.FinishedGoodsReleaseRepository.GetByIdWithDetailsAsync(releaseId);
        if (release == null || release.TotalCost <= 0)
        {
            return null;
        }

        var period = await GetOpenPeriodForDateAsync(release.ReleasedAt);
        var fgInventoryAccountId = await GetRequiredAccountIdAsync(AccountRole.FinishedGoodsInventory);
        var productionClearingAccountId = await GetRequiredAccountIdAsync(AccountRole.ProductionClearing);

        var totalCost = release.TotalCost;
        var journalNumber = await _repositoryManager.JournalEntryRepository.GenerateNextJournalNumberAsync(release.ReleasedAt);

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            EntryDate = release.ReleasedAt.Date,
            AccountingPeriodId = period.Id,
            Description = $"إثبات قيمة إنتاج تام مفرج عنه للمخزن رقم [{release.ReleaseNumber}]",
            ReferenceType = JournalReferenceType.FinishedGoodsRelease,
            ReferenceId = release.Id,
            ReferenceDocumentNumber = release.ReleaseNumber,
            Status = JournalEntryStatus.Posted,
            TotalDebit = totalCost,
            TotalCredit = totalCost,
            CreatedByUserId = userId,
            PostedByUserId = userId,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // 1. Dr Finished Goods Inventory
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = fgInventoryAccountId,
            Debit = totalCost,
            Credit = 0,
            Description = $"إضافة قيمة منتج تام مفرج عنه بسجل {release.ReleaseNumber}",
            ProductId = release.ProductId,
            ReferenceNumber = release.ReleaseNumber
        });

        // 2. Cr Production Clearing
        journal.Lines.Add(new JournalEntryLine
        {
            AccountId = productionClearingAccountId,
            Debit = 0,
            Credit = totalCost,
            Description = $"تسوية وسيط تكاليف إنتاج مفرج عنه بسجل {release.ReleaseNumber}",
            ProductId = release.ProductId,
            ReferenceNumber = release.ReleaseNumber
        });

        _repositoryManager.JournalEntryRepository.Create(journal);
        await _repositoryManager.SaveAsync();

        return journal;
    }

    public async Task<JournalEntry?> ReverseJournalEntryAsync(int journalEntryId, string reason, int userId)
    {
        var original = await _repositoryManager.JournalEntryRepository.GetWithLinesAsync(journalEntryId, trackChanges: true);
        if (original == null || original.Status != JournalEntryStatus.Posted)
        {
            return null;
        }

        var reversalDate = DateTime.UtcNow.Date;
        var period = await GetOpenPeriodForDateAsync(reversalDate);
        var reversalJournalNumber = await _repositoryManager.JournalEntryRepository.GenerateNextJournalNumberAsync(reversalDate);

        var reversalJournal = new JournalEntry
        {
            JournalNumber = reversalJournalNumber,
            EntryDate = reversalDate,
            AccountingPeriodId = period.Id,
            Description = $"قيد عكسي للقيد رقم [{original.JournalNumber}]: {reason}",
            ReferenceType = JournalReferenceType.Reversal,
            ReferenceId = original.Id,
            ReferenceDocumentNumber = original.JournalNumber,
            Status = JournalEntryStatus.Posted,
            TotalDebit = original.TotalCredit,
            TotalCredit = original.TotalDebit,
            ReversalOfJournalEntryId = original.Id,
            ReversalReason = reason,
            CreatedByUserId = userId,
            PostedByUserId = userId,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var origLine in original.Lines)
        {
            reversalJournal.Lines.Add(new JournalEntryLine
            {
                AccountId = origLine.AccountId,
                Debit = origLine.Credit,
                Credit = origLine.Debit,
                Description = $"عكس: {origLine.Description}",
                CustomerId = origLine.CustomerId,
                SupplierId = origLine.SupplierId,
                ProductId = origLine.ProductId,
                MaterialId = origLine.MaterialId,
                ReferenceNumber = origLine.ReferenceNumber
            });
        }

        original.Status = JournalEntryStatus.Reversed;
        original.ReversalReason = reason;
        original.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.JournalEntryRepository.Update(original);
        _repositoryManager.JournalEntryRepository.Create(reversalJournal);
        await _repositoryManager.SaveAsync();

        return reversalJournal;
    }
}
