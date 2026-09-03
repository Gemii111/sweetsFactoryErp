using FactoryX.Domain.Entities;
using FactoryX.Domain.Interfaces;

namespace FactoryX.Infrastructure.Contracts;

public interface IPurchaseReceiptRepository : IBaseRepository<PurchaseReceipt>
{
    Task<IEnumerable<PurchaseReceipt>> GetAllReceiptsAsync(
        PurchaseReceiptStatus? status = null,
        int? purchaseOrderId = null,
        int? supplierId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<PurchaseReceipt?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<PurchaseReceipt?> GetByReceiptNumberAsync(string receiptNumber, bool trackChanges = false);
    Task<int> GetCountForDateAsync(DateTime date);
    Task<bool> IsReceiptNumberUniqueAsync(string receiptNumber, int? excludeId = null);
    Task<IEnumerable<PurchaseReceipt>> GetReceiptsForOrderAsync(int purchaseOrderId);
}
