using AutoMapper;
using FactoryX.Application.DTOs.Requests.OperatorRequests;
using FactoryX.Application.DTOs.Responses.Operator;
using FactoryX.Application.DTOs.Responses.OperatorResponses;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class OperatorService : IOperatorService
{
	private readonly IRepositoryManager _repositoryManager;
	private readonly IMapper _mapper;

	public OperatorService(IRepositoryManager repositoryManager, IMapper mapper)
	{
		_repositoryManager = repositoryManager;
		_mapper = mapper;
	}

	public async Task<IEnumerable<GetAllOperatorResponse>> GetAllAsync()
	{
		var operators = await _repositoryManager.OperatorRepository.GetAllAsync();
		return _mapper.Map<IEnumerable<GetAllOperatorResponse>>(operators);
	}

	public async Task<GetOperatorResponse?> GetByIdAsync(int id)
	{
		var op = await _repositoryManager.OperatorRepository.GetByIdAsync(id);
		return op == null ? null : _mapper.Map<GetOperatorResponse>(op);
	}

	public async Task<InsertOperatorResponse> CreateAsync(InsertOperatorRequest dto)
	{
		var entity = _mapper.Map<Operator>(dto);
		_repositoryManager.OperatorRepository.Create(entity);
		await _repositoryManager.SaveAsync();
		return _mapper.Map<InsertOperatorResponse>(entity);
	}

	public async Task UpdateAsync(UpdateOperatorRequest dto)
	{
		var entity = _mapper.Map<Operator>(dto);
		_repositoryManager.OperatorRepository.Update(entity);
		await _repositoryManager.SaveAsync();
	}

	public async Task DeleteAsync(DeleteOperatorRequest request)
	{
		var entity = await _repositoryManager.OperatorRepository.GetByIdAsync(id: request.Id, trackChanges: true);
		if (entity != null)
		{
			_repositoryManager.OperatorRepository.Remove(entity);
			await _repositoryManager.SaveAsync();
		}
	}
}