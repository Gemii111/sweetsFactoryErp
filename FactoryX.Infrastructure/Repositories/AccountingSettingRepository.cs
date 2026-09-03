using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class AccountingSettingRepository : BaseRepository<AccountingSetting>, IAccountingSettingRepository
{
    public AccountingSettingRepository(AppDbContext context) : base(context) { }

    public async Task<AccountingSetting?> GetByRoleAsync(AccountRole role)
    {
        return await _context.AccountingSettings
            .AsNoTracking()
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s => s.Role == role);
    }

    public async Task<int?> GetAccountIdByRoleAsync(AccountRole role)
    {
        var setting = await _context.AccountingSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Role == role);

        if (setting != null) return setting.AccountId;

        // Fallback: check if an Account has this AccountRole assigned directly
        var directAccount = await _context.Accounts
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccountRole == role && a.IsActive);

        return directAccount?.Id;
    }

    public async Task<IEnumerable<AccountingSetting>> GetAllWithAccountsAsync()
    {
        return await _context.AccountingSettings
            .AsNoTracking()
            .Include(s => s.Account)
            .ToListAsync();
    }
}
