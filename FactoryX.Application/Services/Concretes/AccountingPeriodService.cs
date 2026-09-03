using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class AccountingPeriodService : IAccountingPeriodService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public AccountingPeriodService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AccountingPeriodDto>> GetAllPeriodsAsync()
    {
        await EnsureOpenPeriodExistsAsync();
        var periods = await _repositoryManager.AccountingPeriodRepository.GetPeriodsOrderedAsync();
        var dtos = _mapper.Map<IEnumerable<AccountingPeriodDto>>(periods).ToList();

        foreach (var p in dtos)
        {
            var count = (await _repositoryManager.JournalEntryRepository
                .GetAllAsync(j => j.AccountingPeriodId == p.Id)).Count();
            p.JournalCount = count;
        }

        return dtos;
    }

    public async Task<AccountingPeriodDto?> GetPeriodByIdAsync(int id)
    {
        var period = await _repositoryManager.AccountingPeriodRepository.GetByIdAsync(id);
        return period == null ? null : _mapper.Map<AccountingPeriodDto>(period);
    }

    public async Task<AccountingPeriodDto?> GetPeriodForDateAsync(DateTime date)
    {
        await EnsureOpenPeriodExistsAsync();
        var period = await _repositoryManager.AccountingPeriodRepository.GetPeriodForDateAsync(date);
        return period == null ? null : _mapper.Map<AccountingPeriodDto>(period);
    }

    public async Task<AccountingPeriodDto> CreatePeriodAsync(AccountingPeriodCreateDto dto)
    {
        var nameTrimmed = dto.PeriodName.Trim();
        var existing = await _repositoryManager.AccountingPeriodRepository.GetByNameAsync(nameTrimmed);
        if (existing != null)
        {
            throw new InvalidOperationException($"الفترة المالية باسم '{nameTrimmed}' موجودة بالفعل.");
        }

        var period = new AccountingPeriod
        {
            PeriodName = nameTrimmed,
            StartDate = dto.StartDate.Date,
            EndDate = dto.EndDate.Date,
            Status = AccountingPeriodStatus.Open,
            Notes = dto.Notes?.Trim()
        };

        _repositoryManager.AccountingPeriodRepository.Create(period);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<AccountingPeriodDto>(period);
    }

    public async Task ClosePeriodAsync(ClosePeriodDto dto, int userId)
    {
        var period = await _repositoryManager.AccountingPeriodRepository.GetByIdAsync(dto.PeriodId, trackChanges: true);
        if (period == null)
        {
            throw new KeyNotFoundException($"الفترة المالية رقم #{dto.PeriodId} غير موجودة.");
        }

        if (period.Status == AccountingPeriodStatus.Closed)
        {
            throw new InvalidOperationException("هذه الفترة المالية مغلقة بالفعل.");
        }

        if (await _repositoryManager.AccountingPeriodRepository.HasUnpostedDraftsAsync(dto.PeriodId))
        {
            throw new InvalidOperationException("لا يمكن إغلاق الفترة المالية لوجود قيود يومية معلقة بحالة مسودة (Draft). يرجى ترحيلها أو إلغاؤها أولاً.");
        }

        period.Status = AccountingPeriodStatus.Closed;
        period.ClosedAt = DateTime.UtcNow;
        period.ClosedByUserId = userId;
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            period.Notes = (period.Notes + "\n" + dto.Notes).Trim();
        }

        _repositoryManager.AccountingPeriodRepository.Update(period);
        await _repositoryManager.SaveAsync();
    }

    public async Task EnsureOpenPeriodExistsAsync()
    {
        var periods = await _repositoryManager.AccountingPeriodRepository.GetAllAsync();
        if (periods.Any()) return;

        var currentYear = DateTime.UtcNow.Year;
        var defaultPeriod = new AccountingPeriod
        {
            PeriodName = $"FY{currentYear}",
            StartDate = new DateTime(currentYear, 1, 1),
            EndDate = new DateTime(currentYear, 12, 31),
            Status = AccountingPeriodStatus.Open,
            Notes = $"السنة المالية الافتراضية {currentYear}"
        };

        _repositoryManager.AccountingPeriodRepository.Create(defaultPeriod);
        await _repositoryManager.SaveAsync();
    }
}
