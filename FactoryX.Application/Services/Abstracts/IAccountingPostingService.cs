using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IAccountingPostingService
{
    Task<JournalEntry?> PostSalesInvoiceAsync(int invoiceId, int userId);
    Task<JournalEntry?> PostCustomerPaymentAsync(int paymentId, int userId);
    Task<JournalEntry?> PostPurchaseReceiptAsync(int receiptId, int userId);
    Task<JournalEntry?> PostSupplierPaymentAsync(int supplierPaymentId, int userId);
    Task<JournalEntry?> PostSalesFulfillmentAsync(int fulfillmentId, int userId);
    Task<JournalEntry?> PostWasteAsync(int wasteId, int userId);
    Task<JournalEntry?> PostFinishedGoodsReleaseAsync(int releaseId, int userId);
    Task<JournalEntry?> ReverseJournalEntryAsync(int journalEntryId, string reason, int userId);
}
