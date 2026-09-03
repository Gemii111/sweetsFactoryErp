using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface ICustomerStatementService
{
    Task<CustomerStatementDto?> GetCustomerStatementAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<IEnumerable<CustomerBalanceSummaryDto>> GetAllCustomerBalancesAsync(string? searchTerm = null);
    Task<CustomerBalanceSummaryDto?> GetCustomerBalanceSummaryAsync(int customerId);
}
