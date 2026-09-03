using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IAccountingPeriodRepository : IBaseRepository<AccountingPeriod>
{
    Task<AccountingPeriod?> GetPeriodForDateAsync(DateTime date, bool trackChanges = false);
    Task<AccountingPeriod?> GetByNameAsync(string periodName, bool trackChanges = false);
    Task<IEnumerable<AccountingPeriod>> GetPeriodsOrderedAsync();
    Task<bool> HasUnpostedDraftsAsync(int periodId);
}
