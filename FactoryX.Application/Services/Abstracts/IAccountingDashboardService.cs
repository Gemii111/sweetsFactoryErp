using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IAccountingDashboardService
{
    Task<AccountingDashboardDto> GetDashboardDataAsync(int? periodId = null);
    Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<ExpenseSummaryDto> GetExpenseSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<VatSummaryDto> GetVatSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<CashSummaryDto> GetCashSummaryAsync();
}
