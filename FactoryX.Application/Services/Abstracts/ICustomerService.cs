using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface ICustomerService
{
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(string? searchTerm = null, CustomerType? type = null, bool? isActive = null);
    Task<CustomerDto?> GetCustomerByIdAsync(int id);
    Task<CustomerDto?> GetCustomerByCodeAsync(string code);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request);
    Task<CustomerDto> UpdateCustomerAsync(UpdateCustomerRequest request);
    Task<bool> ToggleActiveStatusAsync(int id);
    Task<CustomerSummaryDto> GetSummaryAsync();
    Task<string> GenerateNextCustomerCodeAsync();
}
