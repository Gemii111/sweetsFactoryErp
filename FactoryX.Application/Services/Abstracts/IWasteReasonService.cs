using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IWasteReasonService
{
    Task<IEnumerable<WasteReasonDto>> GetAllAsync(bool onlyActive = false);
    Task<WasteReasonDto?> GetByIdAsync(int id);
    Task<WasteReasonDto?> GetByCodeAsync(string code);
    Task<WasteReasonDto> CreateAsync(CreateWasteReasonRequest request);
    Task<WasteReasonDto> UpdateAsync(UpdateWasteReasonRequest request);
    Task<bool> ToggleActiveAsync(int id);
}
