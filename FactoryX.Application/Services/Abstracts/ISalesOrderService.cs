using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrderDto>> GetAllOrdersAsync(
        SalesOrderStatus? status = null,
        int? customerId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<SalesOrderDto?> GetOrderByIdAsync(int id);
    Task<SalesOrderDto?> GetOrderByNumberAsync(string orderNumber);
    Task<SalesOrderDto> CreateOrderAsync(CreateSalesOrderRequest request);
    Task<SalesOrderDto> UpdateOrderAsync(UpdateSalesOrderRequest request);
    Task<bool> ConfirmOrderAsync(int id, int userId);
    Task<bool> CancelOrderAsync(int id, string? reason, int userId);
    Task<bool> CloseOrderAsync(int id, int userId);
    Task<SalesOrderSummaryDto> GetSummaryAsync();
    Task<string> GenerateNextOrderNumberAsync(DateTime? date = null);
    Task<SalesOrderFulfillmentInfoDto?> GetFulfillmentInfoAsync(int id);
}
