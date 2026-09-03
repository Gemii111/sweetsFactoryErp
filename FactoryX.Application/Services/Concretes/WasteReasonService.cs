using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;

namespace FactoryX.Application.Services.Concretes;

public class WasteReasonService : IWasteReasonService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateWasteReasonRequest> _createValidator;
    private readonly IValidator<UpdateWasteReasonRequest> _updateValidator;

    public WasteReasonService(
        IRepositoryManager repositoryManager,
        IMapper _mapper,
        IValidator<CreateWasteReasonRequest> createValidator,
        IValidator<UpdateWasteReasonRequest> updateValidator)
    {
        _repositoryManager = repositoryManager;
        this._mapper = _mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<WasteReasonDto>> GetAllAsync(bool onlyActive = false)
    {
        var reasons = await _repositoryManager.WasteReasonRepository.GetAllReasonsAsync(onlyActive);
        return _mapper.Map<IEnumerable<WasteReasonDto>>(reasons);
    }

    public async Task<WasteReasonDto?> GetByIdAsync(int id)
    {
        var reason = await _repositoryManager.WasteReasonRepository.GetByIdAsync(id);
        return reason == null ? null : _mapper.Map<WasteReasonDto>(reason);
    }

    public async Task<WasteReasonDto?> GetByCodeAsync(string code)
    {
        var reason = await _repositoryManager.WasteReasonRepository.GetByCodeAsync(code);
        return reason == null ? null : _mapper.Map<WasteReasonDto>(reason);
    }

    public async Task<WasteReasonDto> CreateAsync(CreateWasteReasonRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var isUnique = await _repositoryManager.WasteReasonRepository.IsCodeUniqueAsync(request.Code);
        if (!isUnique)
        {
            throw new InvalidOperationException($"كود سبب الهالك '{request.Code}' مستخدم مسبقاً. يرجى اختيار كود فريد.");
        }

        var entity = new WasteReason
        {
            Code = request.Code.Trim().ToUpper(),
            Reason = request.Reason.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repositoryManager.WasteReasonRepository.Create(entity);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<WasteReasonDto>(entity);
    }

    public async Task<WasteReasonDto> UpdateAsync(UpdateWasteReasonRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var entity = await _repositoryManager.WasteReasonRepository.GetByIdAsync(request.Id, trackChanges: true);
        if (entity == null)
        {
            throw new KeyNotFoundException($"سبب الهالك برقم #{request.Id} غير موجود.");
        }

        var isUnique = await _repositoryManager.WasteReasonRepository.IsCodeUniqueAsync(request.Code, request.Id);
        if (!isUnique)
        {
            throw new InvalidOperationException($"كود سبب الهالك '{request.Code}' مستخدم مسبقاً.");
        }

        entity.Code = request.Code.Trim().ToUpper();
        entity.Reason = request.Reason.Trim();
        entity.Description = request.Description?.Trim() ?? string.Empty;
        entity.IsActive = request.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WasteReasonRepository.Update(entity);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<WasteReasonDto>(entity);
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var entity = await _repositoryManager.WasteReasonRepository.GetByIdAsync(id, trackChanges: true);
        if (entity == null)
        {
            throw new KeyNotFoundException($"سبب الهالك برقم #{id} غير موجود.");
        }

        entity.IsActive = !entity.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.WasteReasonRepository.Update(entity);
        await _repositoryManager.SaveAsync();

        return entity.IsActive;
    }
}
