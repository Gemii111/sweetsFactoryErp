using FactoryX.Application.DTOs.Requests.OperatorRequests;
using FactoryX.Application.DTOs.Responses.Operator;
using FactoryX.Application.DTOs.Responses.OperatorResponses;

namespace FactoryX.Application.Services.Abstracts;

public interface IOperatorService
{
    Task<IEnumerable<GetAllOperatorResponse>> GetAllAsync();
    Task<GetOperatorResponse?> GetByIdAsync(int id);
    Task<InsertOperatorResponse> CreateAsync(InsertOperatorRequest request);
    Task UpdateAsync(UpdateOperatorRequest request);
    Task DeleteAsync(DeleteOperatorRequest request);
}