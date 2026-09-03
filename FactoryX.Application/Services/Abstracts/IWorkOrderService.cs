using FactoryX.Application.DTOs;
using FactoryX.Application.DTOs.Requests.WorkOrderRequests;
using FactoryX.Application.DTOs.Responses.WorkOrder;
using FactoryX.Application.DTOs.Responses.WorkOrderResponses;

namespace FactoryX.Application.Services.Abstracts;

public interface IWorkOrderService
{
    // Modern Phase 7 Production Order Operations
    Task<IEnumerable<ProductionOrderDto>> GetProductionOrdersAsync(ProductionOrderFilterRequest? filter = null);
    Task<ProductionOrderDto?> GetProductionOrderByIdAsync(int id);
    Task<ProductionOrderDto> CreateProductionOrderAsync(CreateProductionOrderRequest request);
    Task<ProductionOrderDto> UpdateProductionOrderAsync(UpdateProductionOrderRequest request);
    Task<ProductionOrderDto> ReleaseProductionOrderAsync(int id);
    Task<ProductionOrderDto> StartProductionOrderAsync(int id);
    Task<ProductionOrderDto> CompleteProductionOrderAsync(int id);
    Task<ProductionOrderDto> CancelProductionOrderAsync(int id, string? cancellationReason = null);
    Task<bool> DeleteProductionOrderAsync(int id);
    Task<ProductionOrderSummaryDto> GetProductionOrderSummaryAsync();
    Task<string> GenerateOrderNumberAsync();

    // Legacy compatibility methods
    Task<IEnumerable<GetAllWorkOrderResponse>> GetAllAsync();
    Task<GetWorkOrderResponse?> GetByIdAsync(int id);
    Task<InsertWorkOrderResponse> CreateAsync(InsertWorkOrderRequest request);
    Task UpdateAsync(UpdateWorkOrderRequest request);
    Task DeleteAsync(DeleteWorkOrderRequest request);
}