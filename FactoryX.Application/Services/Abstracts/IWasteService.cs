using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IWasteService
{
    Task<IEnumerable<WasteDto>> GetAllAsync(
        WasteType? wasteType = null,
        WasteStatus? status = null,
        int? batchId = null,
        int? productId = null,
        int? materialId = null,
        int? reasonId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<WasteDto?> GetByIdAsync(int id);
    Task<WasteDto?> GetByNumberAsync(string wasteNumber);
    Task<WasteSummaryDto> GetSummaryAsync();

    Task<WasteDto> CreateAsync(CreateWasteRequest request, int userId);
    Task<WasteDto> UpdateAsync(UpdateWasteRequest request, int userId);
    Task<WasteDto> SubmitForApprovalAsync(int id, int userId);
    Task<WasteDto> ApproveWasteAsync(ApproveWasteRequest request, int userId);
    Task<WasteDto> RejectWasteAsync(RejectWasteRequest request, int userId);
    Task<WasteDto> CancelWasteAsync(int id, int userId, string? reason = null);

    Task<string> GenerateWasteNumberAsync(DateTime date);
    Task<decimal> EstimateUnitCostAsync(WasteType wasteType, int? materialId, int? productId, int? batchId);
}
