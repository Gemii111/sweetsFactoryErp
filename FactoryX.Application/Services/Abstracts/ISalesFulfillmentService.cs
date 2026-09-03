using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface ISalesFulfillmentService
{
    Task<IEnumerable<SalesFulfillmentDto>> GetAllFulfillmentsAsync(
        SalesFulfillmentStatus? status = null,
        int? salesOrderId = null,
        int? customerId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<SalesFulfillmentDto?> GetFulfillmentByIdAsync(int id);
    Task<SalesFulfillmentDto?> GetFulfillmentByNumberAsync(string fulfillmentNumber);
    Task<SalesFulfillmentDto> CreateFulfillmentAsync(CreateSalesFulfillmentRequest request, int userId);
    Task<SalesFulfillmentSummaryDto> GetSummaryAsync();
    Task<string> GenerateNextFulfillmentNumberAsync(DateTime? date = null);
}
