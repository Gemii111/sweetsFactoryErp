using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class CustomerRepository : BaseRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync(
        string? searchTerm = null,
        CustomerType? type = null,
        bool? isActive = null,
        bool trackChanges = false)
    {
        var query = trackChanges ? _context.Customers : _context.Customers.AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(c => c.IsActive == isActive.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(c => c.Type == type.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var cleanTerm = searchTerm.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(cleanTerm) ||
                (c.ArabicName != null && c.ArabicName.ToLower().Contains(cleanTerm)) ||
                c.Code.ToLower().Contains(cleanTerm) ||
                (c.Phone != null && c.Phone.Contains(cleanTerm)) ||
                (c.ContactPerson != null && c.ContactPerson.ToLower().Contains(cleanTerm)));
        }

        return await query
            .Include(c => c.SalesOrders)
            .Include(c => c.SalesFulfillments)
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<Customer?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Customers : _context.Customers.AsNoTracking();

        return await query
            .Include(c => c.SalesOrders!)
                .ThenInclude(so => so.Items!)
                    .ThenInclude(i => i.Product)
            .Include(c => c.SalesFulfillments!)
                .ThenInclude(sf => sf.Items)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByCodeAsync(string code, bool trackChanges = false)
    {
        var query = trackChanges ? _context.Customers : _context.Customers.AsNoTracking();
        return await query.FirstOrDefaultAsync(c => c.Code.ToLower() == code.Trim().ToLower());
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        var cleanCode = code.Trim().ToLower();
        return !await _context.Customers.AnyAsync(c =>
            c.Code.ToLower() == cleanCode &&
            (!excludeId.HasValue || c.Id != excludeId.Value));
    }

    public async Task<int> GetCountAsync()
    {
        return await _context.Customers.CountAsync();
    }
}
