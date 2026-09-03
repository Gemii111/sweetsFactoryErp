using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class JournalEntryRepository : BaseRepository<JournalEntry>, IJournalEntryRepository
{
    public JournalEntryRepository(AppDbContext context) : base(context) { }

    public async Task<JournalEntry?> GetWithLinesAsync(int id, bool trackChanges = false)
    {
        var query = _context.JournalEntries.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();

        return await query
            .Include(j => j.AccountingPeriod)
            .Include(j => j.CreatedByUser)
            .Include(j => j.PostedByUser)
            .Include(j => j.ReversalOfJournalEntry)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Customer)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Supplier)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Product)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Material)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<JournalEntry?> GetByNumberAsync(string journalNumber, bool trackChanges = false)
    {
        var query = _context.JournalEntries.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();

        return await query
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.JournalNumber == journalNumber.Trim());
    }

    public async Task<JournalEntry?> GetByReferenceAsync(JournalReferenceType referenceType, int referenceId, bool trackChanges = false)
    {
        var query = _context.JournalEntries.AsQueryable();
        if (!trackChanges) query = query.AsNoTracking();

        return await query
            .Include(j => j.Lines)
            .FirstOrDefaultAsync(j => j.ReferenceType == referenceType && j.ReferenceId == referenceId);
    }

    public async Task<string> GenerateNextJournalNumberAsync(DateTime date)
    {
        var datePrefix = $"JE-{date:yyyyMMdd}-";
        var count = await _context.JournalEntries
            .CountAsync(j => j.JournalNumber.StartsWith(datePrefix));

        return $"{datePrefix}{(count + 1):D4}";
    }

    public async Task<IEnumerable<JournalEntry>> GetPagedJournalsAsync(
        int? periodId,
        DateTime? fromDate,
        DateTime? toDate,
        JournalEntryStatus? status,
        JournalReferenceType? referenceType,
        string? searchTerm)
    {
        var query = _context.JournalEntries
            .AsNoTracking()
            .Include(j => j.AccountingPeriod)
            .Include(j => j.CreatedByUser)
            .Include(j => j.PostedByUser)
            .Include(j => j.Lines)
                .ThenInclude(l => l.Account)
            .AsQueryable();

        if (periodId.HasValue && periodId.Value > 0)
        {
            query = query.Where(j => j.AccountingPeriodId == periodId.Value);
        }

        if (fromDate.HasValue)
        {
            var from = fromDate.Value.Date;
            query = query.Where(j => j.EntryDate.Date >= from);
        }

        if (toDate.HasValue)
        {
            var to = toDate.Value.Date;
            query = query.Where(j => j.EntryDate.Date <= to);
        }

        if (status.HasValue)
        {
            query = query.Where(j => j.Status == status.Value);
        }

        if (referenceType.HasValue)
        {
            query = query.Where(j => j.ReferenceType == referenceType.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(j => j.JournalNumber.Contains(term) ||
                                     j.Description.Contains(term) ||
                                     (j.ReferenceDocumentNumber != null && j.ReferenceDocumentNumber.Contains(term)));
        }

        return await query
            .OrderByDescending(j => j.EntryDate)
            .ThenByDescending(j => j.Id)
            .ToListAsync();
    }

    public IQueryable<JournalEntryLine> GetPostedLinesQueryable()
    {
        return _context.JournalEntryLines
            .AsNoTracking()
            .Include(l => l.JournalEntry)
            .Include(l => l.Account)
            .Include(l => l.Customer)
            .Include(l => l.Supplier)
            .Include(l => l.Product)
            .Include(l => l.Material)
            .Where(l => l.JournalEntry != null && l.JournalEntry.Status == JournalEntryStatus.Posted);
    }
}
