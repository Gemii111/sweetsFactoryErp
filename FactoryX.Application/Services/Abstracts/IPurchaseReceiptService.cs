using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IPurchaseReceiptService
{
    Task<IEnumerable<PurchaseReceiptDto>> GetAllReceiptsAsync(
        PurchaseReceiptStatus? status = null,
        int? purchaseOrderId = null,
        int? supplierId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<PurchaseReceiptDto?> GetReceiptByIdAsync(int id);
    Task<PurchaseReceiptDto> CreateAndPostReceiptAsync(CreatePurchaseReceiptRequest request, int userId);
    Task<PurchaseReceiptSummaryDto> GetSummaryAsync();
}
