using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IPackagingOrderService
{
    Task<IEnumerable<PackagingOrderDto>> GetAllOrdersAsync(
        PackagingOrderStatus? status = null,
        int? batchId = null,
        int? productId = null,
        int? bomId = null,
        int? operatorId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<PackagingOrderDto> GetOrderByIdAsync(int id);
    Task<PackagingOrderDto> CreateOrderAsync(CreatePackagingOrderRequest request, int userId);
    Task<List<PackagingRequirementDto>> CalculateOrderRequirementsAsync(int bomId, decimal packQuantity, int? versionId = null, int? warehouseId = null);
    Task<PackagingOrderDto> StartOrderAsync(int orderId, int userId);
    Task<PackagingOrderDto> PauseOrderAsync(PausePackagingOrderRequest request, int userId);
    Task<PackagingOrderDto> ResumeOrderAsync(int orderId, int userId);
    Task<PackagingOrderDto> ExecuteAndCompleteOrderAsync(ExecutePackagingOrderRequest request, int userId);
    Task<PackagingOrderDto> CancelOrderAsync(CancelPackagingOrderRequest request, int userId);
    Task<decimal> CalculateTheoreticalMaxPacksAsync(int batchId, int packagingBomId);
}
