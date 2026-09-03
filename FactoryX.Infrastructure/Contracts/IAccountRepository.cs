using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IAccountRepository : IBaseRepository<Account>
{
    Task<Account?> GetByCodeAsync(string accountCode, bool trackChanges = false);
    Task<bool> ExistsCodeAsync(string accountCode, int? excludeId = null);
    Task<IEnumerable<Account>> GetHierarchyAsync();
    Task<IEnumerable<Account>> GetActivePostableAccountsAsync();
    Task<Account?> GetWithDetailsAsync(int id);
    Task<bool> HasJournalLinesAsync(int accountId);
}
