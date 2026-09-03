using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IMaterialService
{
    Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync(MaterialFilterRequest? filter = null, bool trackChanges = false);
    Task<IEnumerable<MaterialDto>> GetActiveMaterialsAsync();
    Task<MaterialDto?> GetMaterialByIdAsync(int id, bool trackChanges = false);
    Task<MaterialDto> CreateMaterialAsync(CreateMaterialRequest request);
    Task<MaterialDto> UpdateMaterialAsync(UpdateMaterialRequest request);
    Task<bool> ToggleMaterialStatusAsync(int id);
    Task<bool> DeactivateMaterialAsync(int id);
    Task<bool> DeleteMaterialAsync(int id);

    Task<IEnumerable<MaterialDto>> GetLowStockMaterialsAsync();
    Task<IEnumerable<MaterialDto>> GetMaterialsBelowReorderLevelAsync();
    Task<IEnumerable<MaterialDto>> GetExpiredMaterialsAsync();
    Task<IEnumerable<MaterialDto>> GetMaterialsExpiringSoonAsync(int days = 30);
    Task<MaterialStockSummaryDto> GetStockSummaryAsync();

    decimal ConvertPurchaseToStockQuantity(decimal purchaseQuantity, decimal conversionFactor);
    Task<IEnumerable<StockBalanceDto>> GetMaterialStockBalancesAsync(int materialId);
    Task<IEnumerable<InventoryTransactionDto>> GetMaterialRecentTransactionsAsync(int materialId, int take = 10);
}
