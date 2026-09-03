using FactoryX.Domain.Entities;

namespace FactoryX.Infrastructure.Contracts;

public interface ICustomerRepository : IBaseRepository<Customer>
{
    Task<IEnumerable<Customer>> GetAllCustomersAsync(string? searchTerm = null, CustomerType? type = null, bool? isActive = null, bool trackChanges = false);
    Task<Customer?> GetByIdWithDetailsAsync(int id, bool trackChanges = false);
    Task<Customer?> GetByCodeAsync(string code, bool trackChanges = false);
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    Task<int> GetCountAsync();
}
