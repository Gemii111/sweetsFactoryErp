using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class AccountingPeriodRepository : BaseRepository<AccountingPeriod>, IAccountingPeriodRepository
{
    public AccountingPeriodRepository(AppDbContext context) : base(context) { }

    public async Task<AccountingPeriod?> GetPeriodForDateAsync(DateTime date, bool trackChanges = false)
    {
        var targetDate = date.Date;
        var query = _context.AccountingPeriods.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(p => p.StartDate.Date <= targetDate && p.EndDate.Date >= targetDate);
    }

    public async Task<AccountingPeriod?> GetByNameAsync(string periodName, bool trackChanges = false)
    {
        var query = _context.AccountingPeriods.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();
        return await query.FirstOrDefaultAsync(p => p.PeriodName == periodName.Trim());
    }

    public async Task<IEnumerable<AccountingPeriod>> GetPeriodsOrderedAsync()
    {
        return await _context.AccountingPeriods
            .AsNoTracking()
            .Include(p => p.ClosedByUser)
            .OrderByDescending(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<bool> HasUnpostedDraftsAsync(int periodId)
    {
        return await _context.JournalEntries
            .AnyAsync(j => j.AccountingPeriodId == periodId && j.Status == JournalEntryStatus.Draft);
    }
}
