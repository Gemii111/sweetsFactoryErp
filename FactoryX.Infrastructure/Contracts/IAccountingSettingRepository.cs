using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IAccountingSettingRepository : IBaseRepository<AccountingSetting>
{
    Task<AccountingSetting?> GetByRoleAsync(AccountRole role);
    Task<int?> GetAccountIdByRoleAsync(AccountRole role);
    Task<IEnumerable<AccountingSetting>> GetAllWithAccountsAsync();
}
