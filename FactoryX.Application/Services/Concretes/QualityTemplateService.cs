using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using FluentValidation;

namespace FactoryX.Application.Services.Concretes;

public class QualityTemplateService : IQualityTemplateService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateQualityTemplateRequest> _createValidator;
    private readonly IValidator<UpdateQualityTemplateRequest> _updateValidator;

    public QualityTemplateService(
        IRepositoryManager repositoryManager,
        IMapper mapper,
        IValidator<CreateQualityTemplateRequest> createValidator,
        IValidator<UpdateQualityTemplateRequest> updateValidator)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    public async Task<IEnumerable<QualityTemplateDto>> GetAllTemplatesAsync(
        bool onlyActive = false, int? categoryId = null, int? productId = null)
    {
        var templates = await _repositoryManager.QualityTemplateRepository.GetAllTemplatesWithDetailsAsync(
            onlyActive, categoryId, productId);

        return _mapper.Map<IEnumerable<QualityTemplateDto>>(templates);
    }

    public async Task<QualityTemplateDto?> GetTemplateByIdAsync(int id)
    {
        var template = await _repositoryManager.QualityTemplateRepository.GetTemplateWithItemsAsync(id);
        return template == null ? null : _mapper.Map<QualityTemplateDto>(template);
    }

    public async Task<QualityTemplateDto?> GetTemplateByCodeAsync(string code)
    {
        var template = await _repositoryManager.QualityTemplateRepository.GetTemplateByCodeAsync(code);
        return template == null ? null : _mapper.Map<QualityTemplateDto>(template);
    }

    public async Task<QualityTemplateDto?> GetApplicableTemplateForProductAsync(int productId, int? categoryId = null)
    {
        var template = await _repositoryManager.QualityTemplateRepository.GetApplicableTemplateForProductAsync(productId, categoryId);
        return template == null ? null : _mapper.Map<QualityTemplateDto>(template);
    }

    public async Task<QualityTemplateDto> CreateTemplateAsync(CreateQualityTemplateRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var isUnique = await _repositoryManager.QualityTemplateRepository.IsCodeUniqueAsync(request.Code);
        if (!isUnique)
        {
            throw new InvalidOperationException($"كود قالب الفحص '{request.Code}' مستخدم مسبقاً. يرجى اختيار كود فريد.");
        }

        var template = new QualityTemplate
        {
            Code = request.Code.Trim().ToUpper(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            ProductCategoryId = request.ProductCategoryId > 0 ? request.ProductCategoryId : null,
            ProductId = request.ProductId > 0 ? request.ProductId : null,
            IsActive = request.IsActive,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = request.Items.Select((item, idx) => new QualityTemplateItem
            {
                SpecificationName = item.SpecificationName.Trim(),
                Description = item.Description?.Trim() ?? string.Empty,
                Sequence = item.Sequence > 0 ? item.Sequence : idx + 1,
                IsRequired = item.IsRequired,
                DataType = item.DataType,
                MinValue = item.MinValue,
                MaxValue = item.MaxValue,
                TargetValue = item.TargetValue,
                AllowedTextValues = item.AllowedTextValues?.Trim(),
                Unit = item.Unit?.Trim() ?? string.Empty,
                Notes = item.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList()
        };

        await _repositoryManager.QualityTemplateRepository.AddAsync(template);
        await _repositoryManager.SaveAsync();

        return (await GetTemplateByIdAsync(template.Id))!;
    }

    public async Task<QualityTemplateDto> UpdateTemplateAsync(UpdateQualityTemplateRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        var template = await _repositoryManager.QualityTemplateRepository.GetTemplateWithItemsAsync(request.Id, trackChanges: true);
        if (template == null)
        {
            throw new KeyNotFoundException($"قالب الفحص برقم #{request.Id} غير موجود.");
        }

        var isUnique = await _repositoryManager.QualityTemplateRepository.IsCodeUniqueAsync(request.Code, request.Id);
        if (!isUnique)
        {
            throw new InvalidOperationException($"كود قالب الفحص '{request.Code}' مستخدم مسبقاً.");
        }

        template.Code = request.Code.Trim().ToUpper();
        template.Name = request.Name.Trim();
        template.Description = request.Description?.Trim() ?? string.Empty;
        template.ProductCategoryId = request.ProductCategoryId > 0 ? request.ProductCategoryId : null;
        template.ProductId = request.ProductId > 0 ? request.ProductId : null;
        template.IsActive = request.IsActive;
        template.Notes = request.Notes?.Trim();
        template.UpdatedAt = DateTime.UtcNow;

        // Clear existing items and replace with updated ones
        template.Items.Clear();
        foreach (var (item, idx) in request.Items.Select((v, i) => (v, i)))
        {
            template.Items.Add(new QualityTemplateItem
            {
                QualityTemplateId = template.Id,
                SpecificationName = item.SpecificationName.Trim(),
                Description = item.Description?.Trim() ?? string.Empty,
                Sequence = item.Sequence > 0 ? item.Sequence : idx + 1,
                IsRequired = item.IsRequired,
                DataType = item.DataType,
                MinValue = item.MinValue,
                MaxValue = item.MaxValue,
                TargetValue = item.TargetValue,
                AllowedTextValues = item.AllowedTextValues?.Trim(),
                Unit = item.Unit?.Trim() ?? string.Empty,
                Notes = item.Notes?.Trim(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _repositoryManager.QualityTemplateRepository.Update(template);
        await _repositoryManager.SaveAsync();

        return (await GetTemplateByIdAsync(template.Id))!;
    }

    public async Task<bool> ToggleActiveAsync(int id)
    {
        var template = await _repositoryManager.QualityTemplateRepository.GetByIdAsync(id);
        if (template == null)
        {
            throw new KeyNotFoundException($"قالب الفحص برقم #{id} غير موجود.");
        }

        template.IsActive = !template.IsActive;
        template.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.QualityTemplateRepository.Update(template);
        await _repositoryManager.SaveAsync();

        return template.IsActive;
    }
}
