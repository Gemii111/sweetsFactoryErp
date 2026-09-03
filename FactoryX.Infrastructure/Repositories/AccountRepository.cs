using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class AccountRepository : BaseRepository<Account>, IAccountRepository
{
    public AccountRepository(AppDbContext context) : base(context) { }

    public async Task<Account?> GetByCodeAsync(string accountCode, bool trackChanges = false)
    {
        var query = _context.Accounts.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(a => a.AccountCode == accountCode.Trim());
    }

    public async Task<bool> ExistsCodeAsync(string accountCode, int? excludeId = null)
    {
        return await _context.Accounts
            .AnyAsync(a => a.AccountCode == accountCode.Trim() && (!excludeId.HasValue || a.Id != excludeId.Value));
    }

    public async Task<IEnumerable<Account>> GetHierarchyAsync()
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(a => a.ParentAccount)
            .Include(a => a.ChildAccounts)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();
    }

    public async Task<IEnumerable<Account>> GetActivePostableAccountsAsync()
    {
        return await _context.Accounts
            .AsNoTracking()
            .Where(a => a.IsActive && !a.IsControlAccount)
            .OrderBy(a => a.AccountCode)
            .ToListAsync();
    }

    public async Task<Account?> GetWithDetailsAsync(int id)
    {
        return await _context.Accounts
            .AsNoTracking()
            .Include(a => a.ParentAccount)
            .Include(a => a.ChildAccounts)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> HasJournalLinesAsync(int accountId)
    {
        return await _context.JournalEntryLines.AnyAsync(l => l.AccountId == accountId);
    }
}
