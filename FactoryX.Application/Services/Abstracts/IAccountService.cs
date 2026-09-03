using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IAccountService
{
    Task<IEnumerable<AccountDto>> GetAllAccountsAsync();
    Task<IEnumerable<AccountTreeNodeDto>> GetAccountTreeAsync();
    Task<IEnumerable<AccountDto>> GetActivePostableAccountsAsync();
    Task<AccountDto?> GetAccountByIdAsync(int id);
    Task<AccountDto?> GetAccountByCodeAsync(string accountCode);
    Task<AccountDto> CreateAccountAsync(AccountCreateDto dto);
    Task<AccountDto> UpdateAccountAsync(AccountUpdateDto dto);
    Task ToggleActiveAsync(int id);
    Task DeleteAccountAsync(int id);
    Task<IEnumerable<AccountingSettingDto>> GetAccountingSettingsAsync();
    Task UpdateAccountingSettingAsync(AccountingSettingUpdateDto dto);
    Task SeedDefaultChartOfAccountsAsync();
}
