using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IJournalEntryService
{
    Task<IEnumerable<JournalEntryDto>> GetJournalsAsync(int? periodId, DateTime? fromDate, DateTime? toDate, JournalEntryStatus? status, JournalReferenceType? referenceType, string? searchTerm);
    Task<JournalEntryDto?> GetJournalByIdAsync(int id);
    Task<JournalEntryDto?> GetJournalByNumberAsync(string journalNumber);
    Task<JournalEntryDto> CreateManualJournalAsync(JournalEntryCreateDto dto, int userId);
    Task<JournalEntryDto> ReverseJournalEntryAsync(ReverseJournalDto dto, int userId);
}
