using FactoryX.Application.DTOs;
using FactoryX.Application.DTOs.Requests.ProductionRecordRequests;
using FactoryX.Application.DTOs.Responses.ProductionRecord;

namespace FactoryX.Application.Services.Abstracts;

public interface IProductionRecordService
{
	Task<IEnumerable<ProductionRecordDto>> GetAllAsync();
	Task<ProductionRecordDto?> GetByIdAsync(int id);
	Task<InsertProductionRecordResponse> CreateAsync(InsertProductionRecordRequest request);
	Task UpdateAsync(UpdateProductionRecordRequest request);
	Task DeleteAsync(DeleteProductionRecordRequest request);
}