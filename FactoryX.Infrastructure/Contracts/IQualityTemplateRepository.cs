using FactoryX.Domain.Entities;
using FactoryX.Domain.Interfaces;

namespace FactoryX.Infrastructure.Contracts;

public interface IQualityTemplateRepository : IRepository<QualityTemplate>
{
    Task<IEnumerable<QualityTemplate>> GetAllTemplatesWithDetailsAsync(bool onlyActive = false, int? categoryId = null, int? productId = null);
    Task<QualityTemplate?> GetTemplateWithItemsAsync(int id, bool trackChanges = false);
    Task<QualityTemplate?> GetTemplateByCodeAsync(string code, bool trackChanges = false);
    Task<QualityTemplate?> GetApplicableTemplateForProductAsync(int productId, int? categoryId = null);
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
}
