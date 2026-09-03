using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface IPaymentRepository : IBaseRepository<Payment>
{
    Task<IEnumerable<Payment>> GetAllPaymentsAsync(
        int? invoiceId = null,
        int? customerId = null,
        PaymentMethod? method = null,
        PaymentStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null,
        bool trackChanges = false);

    Task<Payment?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<Payment?> GetByPaymentNumberAsync(string paymentNumber, bool trackChanges = false);
    Task<bool> IsPaymentNumberUniqueAsync(string paymentNumber, int? excludeId = null);
    Task<int> GetCountForDateAsync(DateTime date);
    Task<IEnumerable<Payment>> GetPaymentsByCustomerIdAsync(int customerId, bool trackChanges = false);
    Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId, bool trackChanges = false);
}
