using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IAccountingPeriodService
{
    Task<IEnumerable<AccountingPeriodDto>> GetAllPeriodsAsync();
    Task<AccountingPeriodDto?> GetPeriodByIdAsync(int id);
    Task<AccountingPeriodDto?> GetPeriodForDateAsync(DateTime date);
    Task<AccountingPeriodDto> CreatePeriodAsync(AccountingPeriodCreateDto dto);
    Task ClosePeriodAsync(ClosePeriodDto dto, int userId);
    Task EnsureOpenPeriodExistsAsync();
}
