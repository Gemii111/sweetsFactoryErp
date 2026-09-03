using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IQualityTemplateService
{
    Task<IEnumerable<QualityTemplateDto>> GetAllTemplatesAsync(bool onlyActive = false, int? categoryId = null, int? productId = null);
    Task<QualityTemplateDto?> GetTemplateByIdAsync(int id);
    Task<QualityTemplateDto?> GetTemplateByCodeAsync(string code);
    Task<QualityTemplateDto?> GetApplicableTemplateForProductAsync(int productId, int? categoryId = null);
    Task<QualityTemplateDto> CreateTemplateAsync(CreateQualityTemplateRequest request);
    Task<QualityTemplateDto> UpdateTemplateAsync(UpdateQualityTemplateRequest request);
    Task<bool> ToggleActiveAsync(int id);
}
