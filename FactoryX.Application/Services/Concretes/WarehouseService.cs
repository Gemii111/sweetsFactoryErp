using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class WarehouseService : IWarehouseService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public WarehouseService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
    {
        var warehouses = await _repositoryManager.WarehouseRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<WarehouseDto>>(warehouses);
    }

    public async Task<WarehouseDto?> GetByIdAsync(int id)
    {
        var warehouse = await _repositoryManager.WarehouseRepository.GetWithLocationsAsync(id);
        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request)
    {
        var warehouse = _mapper.Map<Warehouse>(request);
        warehouse.IsActive = true;
        
        _repositoryManager.WarehouseRepository.Create(warehouse);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<WarehouseDto>(warehouse);
    }

    public async Task UpdateAsync(UpdateWarehouseRequest request)
    {
        var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(request.Id, trackChanges: true);
        if (warehouse == null)
            throw new Exception($"Warehouse with ID {request.Id} not found.");

        _mapper.Map(request, warehouse);
        warehouse.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WarehouseRepository.Update(warehouse);
        await _repositoryManager.SaveAsync();
    }

    public async Task ToggleActiveAsync(int id)
    {
        var warehouse = await _repositoryManager.WarehouseRepository.GetByIdAsync(id, trackChanges: true);
        if (warehouse != null)
        {
            warehouse.IsActive = !warehouse.IsActive;
            warehouse.UpdatedAt = DateTime.UtcNow;
            _repositoryManager.WarehouseRepository.Update(warehouse);
            await _repositoryManager.SaveAsync();
        }
    }
}
