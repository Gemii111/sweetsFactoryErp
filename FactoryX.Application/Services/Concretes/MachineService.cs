using AutoMapper;
using FactoryX.Application.DTOs.Requests.MachineRequests;
using FactoryX.Application.DTOs.Responses.MachineResponses;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class MachineService : IMachineService
{
	private readonly IRepositoryManager _repositoryManager;
	private readonly IMapper _mapper;

	public MachineService(IRepositoryManager repositoryManager, IMapper mapper)
	{
		_repositoryManager = repositoryManager;
		_mapper = mapper;
	}

	public async Task<IEnumerable<GetAllMachinesResponse>> GetAllAsync()
	{
		var machines = await _repositoryManager.MachineRepository.GetAllAsync();
		var result = _mapper.Map<IEnumerable<GetAllMachinesResponse>>(machines);
		return result;
	}

	public async Task<GetMachineResponse?> GetByIdAsync(int id)
	{
		var machine = await _repositoryManager.MachineRepository.GetByIdAsync(id);
		var result = _mapper.Map<GetMachineResponse?>(machine);
		return result;
	}

	public async Task<InsertMachineResponse> CreateAsync(InsertMachineRequest request)
	{
		var machine = _mapper.Map<Machine>(request);
		_repositoryManager.MachineRepository.Create(machine);
		await _repositoryManager.SaveAsync();
		return _mapper.Map<InsertMachineResponse>(machine);
	}

	public async Task UpdateAsync(UpdateMachineRequest request)
	{
		var machine = _mapper.Map<Machine>(request);
		_repositoryManager.MachineRepository.Update(machine);
		await _repositoryManager.SaveAsync();
	}

	public async Task DeleteAsync(DeleteMachineRequest request)
	{
		var machine = await _repositoryManager.MachineRepository.GetByIdAsync(id: request.Id, trackChanges: true);
		if (machine != null)
		{
			_repositoryManager.MachineRepository.Remove(machine);
			await _repositoryManager.SaveAsync();
		}
	}
}