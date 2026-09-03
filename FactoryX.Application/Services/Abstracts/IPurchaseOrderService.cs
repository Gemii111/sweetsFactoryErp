using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IPurchaseOrderService
{
    Task<IEnumerable<PurchaseOrderDto>> GetAllOrdersAsync(
        PurchaseOrderStatus? status = null,
        int? supplierId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<PurchaseOrderDto?> GetOrderByIdAsync(int id);
    Task<PurchaseOrderDto> CreateOrderAsync(CreatePurchaseOrderRequest request, int userId);
    Task<PurchaseOrderDto> CreateOrderFromRequestAsync(int purchaseRequestId, int supplierId, int warehouseId, int userId);
    Task<PurchaseOrderDto> UpdateOrderAsync(UpdatePurchaseOrderRequest request);
    Task<PurchaseOrderDto> SubmitOrderAsync(int id, int userId);
    Task<PurchaseOrderDto> ApproveOrderAsync(int id, int userId);
    Task<PurchaseOrderDto> CancelOrderAsync(int id, int userId, string? reason);
    Task<PurchaseOrderDto> CloseOrderAsync(int id, int userId, string? reason);
    Task<PurchaseOrderSummaryDto> GetSummaryAsync();
    Task<POReceivingInfoDto?> GetReceivingInfoAsync(int purchaseOrderId);
}
