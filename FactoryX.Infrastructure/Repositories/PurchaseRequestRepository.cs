using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class PurchaseRequestRepository : BaseRepository<PurchaseRequest>, IPurchaseRequestRepository
{
    public PurchaseRequestRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<PurchaseRequest>> GetAllRequestsAsync(
        PurchaseRequestStatus? status = null,
        int? departmentId = null,
        int? requestedById = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var query = _context.PurchaseRequests.AsNoTracking()
            .Include(pr => pr.Department)
            .Include(pr => pr.RequestedByUser)
            .Include(pr => pr.ApprovedByUser)
            .Include(pr => pr.Items)
                .ThenInclude(i => i.Material)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(pr => pr.Status == status.Value);
        }

        if (departmentId.HasValue && departmentId.Value > 0)
        {
            query = query.Where(pr => pr.DepartmentId == departmentId.Value);
        }

        if (requestedById.HasValue && requestedById.Value > 0)
        {
            query = query.Where(pr => pr.RequestedByUserId == requestedById.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(pr => pr.RequestDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(pr => pr.RequestDate <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var cleanTerm = searchTerm.Trim().ToLower();
            query = query.Where(pr =>
                pr.RequestNumber.ToLower().Contains(cleanTerm) ||
                (pr.Notes != null && pr.Notes.ToLower().Contains(cleanTerm)) ||
                (pr.Department != null && pr.Department.Name.ToLower().Contains(cleanTerm)));
        }

        return await query.OrderByDescending(pr => pr.Id).ToListAsync();
    }

    public async Task<PurchaseRequest?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.PurchaseRequests : _context.PurchaseRequests.AsNoTracking();

        return await query
            .Include(pr => pr.Department)
            .Include(pr => pr.RequestedByUser)
            .Include(pr => pr.ApprovedByUser)
            .Include(pr => pr.Items)
                .ThenInclude(i => i.Material)
            .Include(pr => pr.PurchaseOrders)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<PurchaseRequest?> GetByNumberAsync(string requestNumber, bool trackChanges = false)
    {
        var query = trackChanges ? _context.PurchaseRequests : _context.PurchaseRequests.AsNoTracking();

        return await query
            .Include(pr => pr.Items)
                .ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(pr => pr.RequestNumber == requestNumber);
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1).AddTicks(-1);
        return await _context.PurchaseRequests.CountAsync(pr => pr.RequestDate >= start && pr.RequestDate <= end);
    }

    public async Task<bool> IsRequestNumberUniqueAsync(string requestNumber, int? excludeId = null)
    {
        var query = _context.PurchaseRequests.Where(pr => pr.RequestNumber == requestNumber);
        if (excludeId.HasValue)
        {
            query = query.Where(pr => pr.Id != excludeId.Value);
        }
        return !await query.AnyAsync();
    }
}
