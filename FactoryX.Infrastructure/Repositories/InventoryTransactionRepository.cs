using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Infrastructure.Repositories;

public class InventoryTransactionRepository : BaseRepository<InventoryTransaction>, IInventoryTransactionRepository
{
    public InventoryTransactionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<InventoryTransaction>> GetFilteredTransactionsAsync(
        int? warehouseId, int? materialId, int? productId, InventoryTransactionType? transactionType, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.InventoryTransactions
            .Include(t => t.Warehouse)
            .Include(t => t.SourceLocation)
            .Include(t => t.DestinationLocation)
            .Include(t => t.Material)
            .Include(t => t.Product)
            .Include(t => t.User)
            .AsQueryable();

        if (warehouseId.HasValue)
            query = query.Where(t => t.WarehouseId == warehouseId.Value);

        if (materialId.HasValue)
            query = query.Where(t => t.MaterialId == materialId.Value);

        if (productId.HasValue)
            query = query.Where(t => t.ProductId == productId.Value);

        if (transactionType.HasValue)
            query = query.Where(t => t.TransactionType == transactionType.Value);

        if (startDate.HasValue)
            query = query.Where(t => t.TransactionDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(t => t.TransactionDate <= endDate.Value);

        return await query.OrderByDescending(t => t.TransactionDate).ToListAsync();
    }
}
