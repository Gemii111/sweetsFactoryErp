using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IInvoiceRepository : IBaseRepository<Invoice>
{
    Task<IEnumerable<Invoice>> GetAllInvoicesAsync(
        InvoiceStatus? status = null,
        int? customerId = null,
        int? salesOrderId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool trackChanges = false);

    Task<Invoice?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber, bool trackChanges = false);
    Task<bool> IsInvoiceNumberUniqueAsync(string invoiceNumber, int? excludeId = null);
    Task<int> GetCountForDateAsync(DateTime date);
    Task<IEnumerable<Invoice>> GetInvoicesByCustomerIdAsync(int customerId, bool trackChanges = false);
    Task<IEnumerable<Invoice>> GetInvoicesBySalesOrderIdAsync(int salesOrderId, bool trackChanges = false);
}
