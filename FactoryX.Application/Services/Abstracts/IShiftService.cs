using FactoryX.Application.DTOs.Requests.ShiftRequests;
using FactoryX.Application.DTOs.Responses.Shift;
using FactoryX.Application.DTOs.Responses.ShiftResponses;

namespace FactoryX.Application.Services.Abstracts;

public interface IShiftService
{
	Task<IEnumerable<GetAllShiftResponse>> GetAllAsync();
	Task<GetShiftResponse?> GetByIdAsync(int id);
	Task<InsertShiftResponse> CreateAsync(InsertShiftRequest request);
	Task UpdateAsync(UpdateShiftRequest request);
	Task DeleteAsync(DeleteShiftRequest request);
}