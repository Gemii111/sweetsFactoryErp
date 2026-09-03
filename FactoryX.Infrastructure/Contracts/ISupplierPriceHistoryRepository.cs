using FactoryX.Domain.Entities;
using FactoryX.Domain.Interfaces;

namespace FactoryX.Infrastructure.Contracts;

public interface ISupplierPriceHistoryRepository : IBaseRepository<SupplierPriceHistory>
{
    Task<IEnumerable<SupplierPriceHistory>> GetHistoryAsync(int? supplierId = null, int? materialId = null);
    Task<SupplierPriceHistory?> GetLatestPriceAsync(int supplierId, int materialId);
}
