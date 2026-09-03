using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class PurchaseReceiptRepository : BaseRepository<PurchaseReceipt>, IPurchaseReceiptRepository
{
    public PurchaseReceiptRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<PurchaseReceipt>> GetAllReceiptsAsync(
        PurchaseReceiptStatus? status = null,
        int? purchaseOrderId = null,
        int? supplierId = null,
        int? warehouseId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var query = _context.PurchaseReceipts.AsNoTracking()
            .Include(pr => pr.PurchaseOrder)
            .Include(pr => pr.Supplier)
            .Include(pr => pr.Warehouse)
            .Include(pr => pr.ReceivedByUser)
            .Include(pr => pr.Items)
                .ThenInclude(i => i.Material)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(pr => pr.Status == status.Value);
        }

        if (purchaseOrderId.HasValue && purchaseOrderId.Value > 0)
        {
            query = query.Where(pr => pr.PurchaseOrderId == purchaseOrderId.Value);
        }

        if (supplierId.HasValue && supplierId.Value > 0)
        {
            query = query.Where(pr => pr.SupplierId == supplierId.Value);
        }

        if (warehouseId.HasValue && warehouseId.Value > 0)
        {
            query = query.Where(pr => pr.WarehouseId == warehouseId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(pr => pr.ReceiptDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            var endOfDay = toDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(pr => pr.ReceiptDate <= endOfDay);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var cleanTerm = searchTerm.Trim().ToLower();
            query = query.Where(pr =>
                pr.ReceiptNumber.ToLower().Contains(cleanTerm) ||
                (pr.Supplier != null && pr.Supplier.Name.ToLower().Contains(cleanTerm)) ||
                (pr.PurchaseOrder != null && pr.PurchaseOrder.OrderNumber.ToLower().Contains(cleanTerm)) ||
                (pr.Notes != null && pr.Notes.ToLower().Contains(cleanTerm)));
        }

        return await query.OrderByDescending(pr => pr.Id).ToListAsync();
    }

    public async Task<PurchaseReceipt?> GetByIdWithDetailsAsync(int id, bool trackChanges = false)
    {
        var query = trackChanges ? _context.PurchaseReceipts : _context.PurchaseReceipts.AsNoTracking();

        return await query
            .Include(pr => pr.PurchaseOrder!)
                .ThenInclude(po => po.Items)
            .Include(pr => pr.Supplier!)
                .ThenInclude(s => s.Category)
            .Include(pr => pr.Warehouse)
            .Include(pr => pr.ReceivedByUser)
            .Include(pr => pr.Items!)
                .ThenInclude(i => i.Material)
            .Include(pr => pr.Items!)
                .ThenInclude(i => i.Warehouse)
            .Include(pr => pr.Items!)
                .ThenInclude(i => i.Location)
            .Include(pr => pr.Items!)
                .ThenInclude(i => i.InventoryTransaction)
            .FirstOrDefaultAsync(pr => pr.Id == id);
    }

    public async Task<PurchaseReceipt?> GetByReceiptNumberAsync(string receiptNumber, bool trackChanges = false)
    {
        var query = trackChanges ? _context.PurchaseReceipts : _context.PurchaseReceipts.AsNoTracking();

        return await query
            .Include(pr => pr.Supplier)
            .Include(pr => pr.Items)
                .ThenInclude(i => i.Material)
            .FirstOrDefaultAsync(pr => pr.ReceiptNumber == receiptNumber);
    }

    public async Task<int> GetCountForDateAsync(DateTime date)
    {
        var start = date.Date;
        var end = start.AddDays(1).AddTicks(-1);
        return await _context.PurchaseReceipts.CountAsync(pr => pr.ReceiptDate >= start && pr.ReceiptDate <= end);
    }

    public async Task<bool> IsReceiptNumberUniqueAsync(string receiptNumber, int? excludeId = null)
    {
        var query = _context.PurchaseReceipts.Where(pr => pr.ReceiptNumber == receiptNumber);
        if (excludeId.HasValue)
        {
            query = query.Where(pr => pr.Id != excludeId.Value);
        }
        return !await query.AnyAsync();
    }

    public async Task<IEnumerable<PurchaseReceipt>> GetReceiptsForOrderAsync(int purchaseOrderId)
    {
        return await _context.PurchaseReceipts.AsNoTracking()
            .Where(pr => pr.PurchaseOrderId == purchaseOrderId)
            .Include(pr => pr.Items)
                .ThenInclude(i => i.Material)
            .OrderByDescending(pr => pr.ReceiptDate)
            .ToListAsync();
    }
}
