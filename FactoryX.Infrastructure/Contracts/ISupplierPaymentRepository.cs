using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface ISupplierPaymentRepository : IBaseRepository<SupplierPayment>
{
    Task<SupplierPayment?> GetWithDetailsAsync(int id);
    Task<string> GenerateNextPaymentNumberAsync(DateTime date);
    Task<IEnumerable<SupplierPayment>> GetBySupplierIdAsync(int supplierId);
    Task<IEnumerable<SupplierPayment>> GetAllWithDetailsAsync();
}
