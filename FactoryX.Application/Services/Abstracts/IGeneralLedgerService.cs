using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IGeneralLedgerService
{
    Task<IEnumerable<GeneralLedgerAccountDto>> GetGeneralLedgerAsync(GeneralLedgerQueryDto query);
    Task<TrialBalanceDto> GetTrialBalanceAsync(TrialBalanceQueryDto query);
    Task<CustomerLedgerDto?> GetCustomerLedgerAsync(int customerId, DateTime? fromDate, DateTime? toDate);
    Task<SupplierLedgerDto?> GetSupplierLedgerAsync(int supplierId, DateTime? fromDate, DateTime? toDate);
}
