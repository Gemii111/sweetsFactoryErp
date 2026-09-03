using FactoryX.Application.DTOs.Requests.MachineRequests;
using FactoryX.Application.DTOs.Responses.MachineResponses;

namespace FactoryX.Application.Services.Abstracts;

public interface IMachineService
{
	Task<IEnumerable<GetAllMachinesResponse>> GetAllAsync();
	Task<GetMachineResponse?> GetByIdAsync(int id);
	Task<InsertMachineResponse> CreateAsync(InsertMachineRequest request);
	Task UpdateAsync(UpdateMachineRequest request);
	Task DeleteAsync(DeleteMachineRequest request);
}