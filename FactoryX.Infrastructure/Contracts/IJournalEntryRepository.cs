using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IJournalEntryRepository : IBaseRepository<JournalEntry>
{
    Task<JournalEntry?> GetWithLinesAsync(int id, bool trackChanges = false);
    Task<JournalEntry?> GetByNumberAsync(string journalNumber, bool trackChanges = false);
    Task<JournalEntry?> GetByReferenceAsync(JournalReferenceType referenceType, int referenceId, bool trackChanges = false);
    Task<string> GenerateNextJournalNumberAsync(DateTime date);
    Task<IEnumerable<JournalEntry>> GetPagedJournalsAsync(int? periodId, DateTime? fromDate, DateTime? toDate, JournalEntryStatus? status, JournalReferenceType? referenceType, string? searchTerm);
    IQueryable<JournalEntryLine> GetPostedLinesQueryable();
}
