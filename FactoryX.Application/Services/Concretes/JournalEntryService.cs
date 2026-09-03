using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class JournalEntryService : IJournalEntryService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IAccountService _accountService;
    private readonly IAccountingPeriodService _periodService;
    private readonly IMapper _mapper;

    public JournalEntryService(
        IRepositoryManager repositoryManager,
        IAccountService accountService,
        IAccountingPeriodService periodService,
        IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _accountService = accountService;
        _periodService = periodService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<JournalEntryDto>> GetJournalsAsync(
        int? periodId,
        DateTime? fromDate,
        DateTime? toDate,
        JournalEntryStatus? status,
        JournalReferenceType? referenceType,
        string? searchTerm)
    {
        var journals = await _repositoryManager.JournalEntryRepository.GetPagedJournalsAsync(
            periodId, fromDate, toDate, status, referenceType, searchTerm);

        return _mapper.Map<IEnumerable<JournalEntryDto>>(journals);
    }

    public async Task<JournalEntryDto?> GetJournalByIdAsync(int id)
    {
        var journal = await _repositoryManager.JournalEntryRepository.GetWithLinesAsync(id);
        return journal == null ? null : _mapper.Map<JournalEntryDto>(journal);
    }

    public async Task<JournalEntryDto?> GetJournalByNumberAsync(string journalNumber)
    {
        var journal = await _repositoryManager.JournalEntryRepository.GetByNumberAsync(journalNumber);
        return journal == null ? null : _mapper.Map<JournalEntryDto>(journal);
    }

    public async Task<JournalEntryDto> CreateManualJournalAsync(JournalEntryCreateDto dto, int userId)
    {
        if (dto.Lines == null || dto.Lines.Count < 2)
        {
            throw new InvalidOperationException("يجب أن يحتوي القيد المحاسبي على طرفين (بندين) على الأقل.");
        }

        var totalDebit = dto.Lines.Sum(l => l.Debit);
        var totalCredit = dto.Lines.Sum(l => l.Credit);

        if (totalDebit <= 0 || Math.Abs(totalDebit - totalCredit) >= 0.01m)
        {
            throw new InvalidOperationException($"القيد غير متوازن! إجمالي المدين ({totalDebit:N2}) لا يساوي إجمالي الدائن ({totalCredit:N2}).");
        }

        // Validate Period
        var period = await _repositoryManager.AccountingPeriodRepository.GetPeriodForDateAsync(dto.EntryDate);
        if (period == null)
        {
            await _periodService.EnsureOpenPeriodExistsAsync();
            period = await _repositoryManager.AccountingPeriodRepository.GetPeriodForDateAsync(dto.EntryDate);
        }

        if (period == null || period.Status == AccountingPeriodStatus.Closed)
        {
            throw new InvalidOperationException($"لا يمكن ترحيل قيد في فترة مالية مغلقة أو غير محددة لتاريخ '{dto.EntryDate:yyyy-MM-dd}'.");
        }

        var journalNumber = await _repositoryManager.JournalEntryRepository.GenerateNextJournalNumberAsync(dto.EntryDate);

        var journal = new JournalEntry
        {
            JournalNumber = journalNumber,
            EntryDate = dto.EntryDate.Date,
            AccountingPeriodId = period.Id,
            Description = dto.Description.Trim(),
            ReferenceType = JournalReferenceType.Manual,
            ReferenceDocumentNumber = dto.ReferenceDocumentNumber?.Trim(),
            Status = JournalEntryStatus.Posted,
            TotalDebit = totalDebit,
            TotalCredit = totalCredit,
            CreatedByUserId = userId,
            PostedByUserId = userId,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var lineDto in dto.Lines)
        {
            var account = await _repositoryManager.AccountRepository.GetByIdAsync(lineDto.AccountId);
            if (account == null || !account.IsActive)
            {
                throw new InvalidOperationException($"الحساب رقم #{lineDto.AccountId} غير موجود أو معطل.");
            }

            if (account.IsControlAccount)
            {
                throw new InvalidOperationException($"لا يمكن الترحيل المباشر على حساب رئيسي/تجميعي ({account.AccountCode} - {account.AccountNameAr}). يجب اختيار حساب فرعي.");
            }

            journal.Lines.Add(new JournalEntryLine
            {
                AccountId = lineDto.AccountId,
                Debit = lineDto.Debit,
                Credit = lineDto.Credit,
                Description = lineDto.Description?.Trim() ?? dto.Description.Trim(),
                CustomerId = lineDto.CustomerId > 0 ? lineDto.CustomerId : null,
                SupplierId = lineDto.SupplierId > 0 ? lineDto.SupplierId : null,
                ProductId = lineDto.ProductId > 0 ? lineDto.ProductId : null,
                MaterialId = lineDto.MaterialId > 0 ? lineDto.MaterialId : null,
                ReferenceNumber = lineDto.ReferenceNumber?.Trim() ?? dto.ReferenceDocumentNumber?.Trim()
            });
        }

        _repositoryManager.JournalEntryRepository.Create(journal);
        await _repositoryManager.SaveAsync();

        var createdWithDetails = await _repositoryManager.JournalEntryRepository.GetWithLinesAsync(journal.Id);
        return _mapper.Map<JournalEntryDto>(createdWithDetails);
    }

    public async Task<JournalEntryDto> ReverseJournalEntryAsync(ReverseJournalDto dto, int userId)
    {
        var original = await _repositoryManager.JournalEntryRepository.GetWithLinesAsync(dto.JournalEntryId, trackChanges: true);
        if (original == null)
        {
            throw new KeyNotFoundException($"القيد المحاسبي رقم #{dto.JournalEntryId} غير موجود.");
        }

        if (original.Status != JournalEntryStatus.Posted)
        {
            throw new InvalidOperationException($"لا يمكن عكس قيد بحالة '{original.Status}'. فقط القيود المرحلة (Posted) يمكن عكسها.");
        }

        var reversalDate = DateTime.UtcNow.Date;
        var period = await _repositoryManager.AccountingPeriodRepository.GetPeriodForDateAsync(reversalDate);
        if (period == null || period.Status == AccountingPeriodStatus.Closed)
        {
            await _periodService.EnsureOpenPeriodExistsAsync();
            period = await _repositoryManager.AccountingPeriodRepository.GetPeriodForDateAsync(reversalDate);
        }

        if (period == null || period.Status == AccountingPeriodStatus.Closed)
        {
            throw new InvalidOperationException("لا يمكن إنشاء قيد عكسي لعدم وجود فترة مالية مفتوحة لتاريخ اليوم.");
        }

        var reversalJournalNumber = await _repositoryManager.JournalEntryRepository.GenerateNextJournalNumberAsync(reversalDate);

        var reversalJournal = new JournalEntry
        {
            JournalNumber = reversalJournalNumber,
            EntryDate = reversalDate,
            AccountingPeriodId = period.Id,
            Description = $"قيد عكسي للقيد رقم [{original.JournalNumber}]: {dto.Reason.Trim()}",
            ReferenceType = JournalReferenceType.Reversal,
            ReferenceId = original.Id,
            ReferenceDocumentNumber = original.JournalNumber,
            Status = JournalEntryStatus.Posted,
            TotalDebit = original.TotalCredit,
            TotalCredit = original.TotalDebit,
            ReversalOfJournalEntryId = original.Id,
            ReversalReason = dto.Reason.Trim(),
            CreatedByUserId = userId,
            PostedByUserId = userId,
            PostedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        // Swap Debits and Credits
        foreach (var origLine in original.Lines)
        {
            reversalJournal.Lines.Add(new JournalEntryLine
            {
                AccountId = origLine.AccountId,
                Debit = origLine.Credit,   // Flipped
                Credit = origLine.Debit,   // Flipped
                Description = $"عكس: {origLine.Description}",
                CustomerId = origLine.CustomerId,
                SupplierId = origLine.SupplierId,
                ProductId = origLine.ProductId,
                MaterialId = origLine.MaterialId,
                ReferenceNumber = origLine.ReferenceNumber
            });
        }

        original.Status = JournalEntryStatus.Reversed;
        original.ReversalReason = dto.Reason.Trim();
        original.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.JournalEntryRepository.Update(original);
        _repositoryManager.JournalEntryRepository.Create(reversalJournal);
        await _repositoryManager.SaveAsync();

        var createdWithDetails = await _repositoryManager.JournalEntryRepository.GetWithLinesAsync(reversalJournal.Id);
        return _mapper.Map<JournalEntryDto>(createdWithDetails);
    }
}
