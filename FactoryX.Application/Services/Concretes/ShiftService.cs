using AutoMapper;
using FactoryX.Application.DTOs.Requests.ShiftRequests;
using FactoryX.Application.DTOs.Responses.Shift;
using FactoryX.Application.DTOs.Responses.ShiftResponses;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class ShiftService : IShiftService
{
	private readonly IRepositoryManager _repositoryManager;
	private readonly IMapper _mapper;

	public ShiftService(IRepositoryManager repositoryManager, IMapper mapper)
	{
		_repositoryManager = repositoryManager;
		_mapper = mapper;
	}

	public async Task<IEnumerable<GetAllShiftResponse>> GetAllAsync()
	{
		var shifts = await _repositoryManager.ShiftRepository.GetAllAsync();
		return _mapper.Map<IEnumerable<GetAllShiftResponse>>(shifts);
	}

	public async Task<GetShiftResponse?> GetByIdAsync(int id)
	{
		var shift = await _repositoryManager.ShiftRepository.GetByIdAsync(id);
		return shift == null ? null : _mapper.Map<GetShiftResponse>(shift);
	}

	public async Task<InsertShiftResponse> CreateAsync(InsertShiftRequest request)
	{
		var shift = _mapper.Map<Shift>(request);
		_repositoryManager.ShiftRepository.Create(shift);
		await _repositoryManager.SaveAsync();

		return _mapper.Map<InsertShiftResponse>(shift);
	}

	public async Task UpdateAsync(UpdateShiftRequest request)
	{
		var shift = _mapper.Map<Shift>(request);
		_repositoryManager.ShiftRepository.Update(shift);
		await _repositoryManager.SaveAsync();
	}

	public async Task DeleteAsync(DeleteShiftRequest request)
	{
		var entity = await _repositoryManager.ShiftRepository.GetByIdAsync(id: request.Id, trackChanges: true);
		if (entity != null)
		{
			_repositoryManager.ShiftRepository.Remove(entity);
			await _repositoryManager.SaveAsync();
		}
	}
}