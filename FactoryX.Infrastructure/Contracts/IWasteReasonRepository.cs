using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IWasteReasonRepository : IBaseRepository<WasteReason>
{
    Task<IEnumerable<WasteReason>> GetAllReasonsAsync(bool onlyActive = false);
    Task<WasteReason?> GetByCodeAsync(string code);
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    Task<bool> HasAssociatedWastesAsync(int id);
}
