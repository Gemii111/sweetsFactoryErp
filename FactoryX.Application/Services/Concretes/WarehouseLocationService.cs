using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class WarehouseLocationService : IWarehouseLocationService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public WarehouseLocationService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<WarehouseLocationDto>> GetAllLocationsAsync()
    {
        var locations = await _repositoryManager.WarehouseLocationRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<WarehouseLocationDto>>(locations);
    }

    public async Task<IEnumerable<WarehouseLocationDto>> GetByWarehouseIdAsync(int warehouseId)
    {
        var locations = await _repositoryManager.WarehouseLocationRepository.GetByWarehouseIdAsync(warehouseId);
        return _mapper.Map<IEnumerable<WarehouseLocationDto>>(locations);
    }

    public async Task<WarehouseLocationDto?> GetByIdAsync(int id)
    {
        var location = await _repositoryManager.WarehouseLocationRepository.GetByIdAsync(id);
        return _mapper.Map<WarehouseLocationDto>(location);
    }

    public async Task<WarehouseLocationDto> CreateAsync(CreateWarehouseLocationRequest request)
    {
        var location = _mapper.Map<WarehouseLocation>(request);
        location.IsActive = true;

        _repositoryManager.WarehouseLocationRepository.Create(location);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<WarehouseLocationDto>(location);
    }

    public async Task UpdateAsync(UpdateWarehouseLocationRequest request)
    {
        var location = await _repositoryManager.WarehouseLocationRepository.GetByIdAsync(request.Id, trackChanges: true);
        if (location == null)
            throw new Exception($"Warehouse Location with ID {request.Id} not found.");

        _mapper.Map(request, location);
        location.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WarehouseLocationRepository.Update(location);
        await _repositoryManager.SaveAsync();
    }

    public async Task ToggleActiveAsync(int id)
    {
        var location = await _repositoryManager.WarehouseLocationRepository.GetByIdAsync(id, trackChanges: true);
        if (location != null)
        {
            location.IsActive = !location.IsActive;
            location.UpdatedAt = DateTime.UtcNow;
            _repositoryManager.WarehouseLocationRepository.Update(location);
            await _repositoryManager.SaveAsync();
        }
    }
}
