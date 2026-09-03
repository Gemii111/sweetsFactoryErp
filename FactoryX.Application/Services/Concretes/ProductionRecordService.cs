using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.DTOs.Requests.ProductionRecordRequests;
using FactoryX.Application.DTOs.Responses.ProductionRecord;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class ProductionRecordService : IProductionRecordService
{
	private readonly IRepositoryManager _repositoryManager;
	private readonly IMapper _mapper;

	public ProductionRecordService(IRepositoryManager repositoryManager, IMapper mapper)
	{
		_repositoryManager = repositoryManager;
		_mapper = mapper;
	}

	public async Task<IEnumerable<ProductionRecordDto>> GetAllAsync()
	{
		var records = await _repositoryManager.ProductionRecordRepository.GetAllAsync();
		var workOrders = (await _repositoryManager.WorkOrderRepository.GetAllAsync()).ToDictionary(w => w.Id, w => w);
		var operators = (await _repositoryManager.OperatorRepository.GetAllAsync()).ToDictionary(o => o.Id, o => o);

		return records.Select(r => ToDto(r, workOrders, operators));
	}

	public async Task<ProductionRecordDto?> GetByIdAsync(int id)
	{
		var record = await _repositoryManager.ProductionRecordRepository.GetByIdAsync(id);
		if (record == null) return null;
		var workOrder = await _repositoryManager.WorkOrderRepository.GetByIdAsync(record.WorkOrderId);
		var op = await _repositoryManager.OperatorRepository.GetByIdAsync(record.OperatorId);

		return ToDto(record, workOrder, op);
	}

	public async Task<InsertProductionRecordResponse> CreateAsync(InsertProductionRecordRequest request)
	{
		var productionRecord = _mapper.Map<ProductionRecord>(request);
		_repositoryManager.ProductionRecordRepository.Create(productionRecord);
		await _repositoryManager.SaveAsync();

		return _mapper.Map<InsertProductionRecordResponse>(productionRecord);
	}

	public async Task UpdateAsync(UpdateProductionRecordRequest request)
	{
		var productionRecord = _mapper.Map<ProductionRecord>(request);
		_repositoryManager.ProductionRecordRepository.Update(productionRecord);
		await _repositoryManager.SaveAsync();
	}

	public async Task DeleteAsync(DeleteProductionRecordRequest request)
	{
		var entity = await _repositoryManager.ProductionRecordRepository.GetByIdAsync(request.Id, trackChanges: true);
		if (entity != null)
		{
			_repositoryManager.ProductionRecordRepository.Remove(entity);
			await _repositoryManager.SaveAsync();
		}
	}

	private static ProductionRecordDto ToDto(ProductionRecord r, IDictionary<int, WorkOrder>? workOrders = null, IDictionary<int, Operator>? operators = null)
	{
		var dto = new ProductionRecordDto
		{
			Id = r.Id,
			WorkOrderId = r.WorkOrderId,
			OperatorId = r.OperatorId,
			QuantityProduced = r.QuantityProduced,
			Timestamp = r.Timestamp,
			WorkOrderName = workOrders != null && workOrders.TryGetValue(r.WorkOrderId, out var w) ? $"WO-{w.Id}" : null,
			OperatorName = operators != null && operators.TryGetValue(r.OperatorId, out var o) ? o.Name : null
		};
		return dto;
	}

	private static ProductionRecordDto ToDto(ProductionRecord r, WorkOrder? workOrder, Operator? op)
	{
		var dto = new ProductionRecordDto
		{
			Id = r.Id,
			WorkOrderId = r.WorkOrderId,
			OperatorId = r.OperatorId,
			QuantityProduced = r.QuantityProduced,
			Timestamp = r.Timestamp,
			WorkOrderName = workOrder != null ? $"WO-{workOrder.Id}" : null,
			OperatorName = op?.Name
		};
		return dto;
	}
}