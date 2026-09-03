using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class CustomerService : ICustomerService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public CustomerService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(string? searchTerm = null, CustomerType? type = null, bool? isActive = null)
    {
        var customers = await _repositoryManager.CustomerRepository.GetAllCustomersAsync(searchTerm, type, isActive);
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
    {
        var customer = await _repositoryManager.CustomerRepository.GetByIdWithDetailsAsync(id);
        if (customer == null) return null;

        var dto = _mapper.Map<CustomerDto>(customer);

        // Populate recent sales orders
        if (customer.SalesOrders != null && customer.SalesOrders.Any())
        {
            dto.RecentSalesOrders = customer.SalesOrders
                .OrderByDescending(so => so.Id)
                .Take(10)
                .Select(so => _mapper.Map<SalesOrderDto>(so))
                .ToList();

            dto.SalesOrdersCount = customer.SalesOrders.Count;
            dto.TotalSalesValue = customer.SalesOrders
                .Where(so => so.Status != SalesOrderStatus.Cancelled)
                .Sum(so => so.TotalAmount);
        }

        if (customer.SalesFulfillments != null)
        {
            dto.SalesFulfillmentsCount = customer.SalesFulfillments.Count;
        }

        return dto;
    }

    public async Task<CustomerDto?> GetCustomerByCodeAsync(string code)
    {
        var customer = await _repositoryManager.CustomerRepository.GetByCodeAsync(code);
        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var code = string.IsNullOrWhiteSpace(request.Code)
            ? await GenerateNextCustomerCodeAsync()
            : request.Code.Trim().ToUpper();

        var isUnique = await _repositoryManager.CustomerRepository.IsCodeUniqueAsync(code);
        if (!isUnique)
        {
            throw new InvalidOperationException($"كود العميل '{code}' مستخدم بالفعل، يرجى اختيار كود آخر.");
        }

        var customer = _mapper.Map<Customer>(request);
        customer.Code = code;
        customer.Name = request.Name.Trim();
        customer.ArabicName = request.ArabicName?.Trim();
        customer.ContactPerson = request.ContactPerson?.Trim();
        customer.Phone = request.Phone?.Trim();
        customer.Mobile = request.Mobile?.Trim();
        customer.Email = request.Email?.Trim();
        customer.Address = request.Address?.Trim();
        customer.TaxNumber = request.TaxNumber?.Trim();
        customer.Notes = request.Notes?.Trim();
        customer.CreatedAt = DateTime.UtcNow;
        customer.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.CustomerRepository.Create(customer);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<CustomerDto> UpdateCustomerAsync(UpdateCustomerRequest request)
    {
        var customer = await _repositoryManager.CustomerRepository.GetByIdAsync(request.Id, trackChanges: true);
        if (customer == null)
        {
            throw new KeyNotFoundException($"العميل بالمعرف #{request.Id} غير موجود.");
        }

        var code = request.Code.Trim().ToUpper();
        var isUnique = await _repositoryManager.CustomerRepository.IsCodeUniqueAsync(code, request.Id);
        if (!isUnique)
        {
            throw new InvalidOperationException($"كود العميل '{code}' مستخدم بالفعل لعميل آخر.");
        }

        customer.Code = code;
        customer.Name = request.Name.Trim();
        customer.ArabicName = request.ArabicName?.Trim();
        customer.Type = request.Type;
        customer.ContactPerson = request.ContactPerson?.Trim();
        customer.Phone = request.Phone?.Trim();
        customer.Mobile = request.Mobile?.Trim();
        customer.Email = request.Email?.Trim();
        customer.Address = request.Address?.Trim();
        customer.TaxNumber = request.TaxNumber?.Trim();
        customer.Notes = request.Notes?.Trim();
        customer.CreditLimit = request.CreditLimit;
        customer.IsActive = request.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.CustomerRepository.Update(customer);
        await _repositoryManager.SaveAsync();

        return _mapper.Map<CustomerDto>(customer);
    }

    public async Task<bool> ToggleActiveStatusAsync(int id)
    {
        var customer = await _repositoryManager.CustomerRepository.GetByIdAsync(id, trackChanges: true);
        if (customer == null)
        {
            throw new KeyNotFoundException($"العميل بالمعرف #{id} غير موجود.");
        }

        customer.IsActive = !customer.IsActive;
        customer.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.CustomerRepository.Update(customer);
        await _repositoryManager.SaveAsync();

        return customer.IsActive;
    }

    public async Task<CustomerSummaryDto> GetSummaryAsync()
    {
        var all = (await _repositoryManager.CustomerRepository.GetAllCustomersAsync()).ToList();

        return new CustomerSummaryDto
        {
            TotalCustomers = all.Count,
            ActiveCustomers = all.Count(c => c.IsActive),
            InactiveCustomers = all.Count(c => !c.IsActive),
            WholesaleCount = all.Count(c => c.Type == CustomerType.Wholesale),
            RetailCount = all.Count(c => c.Type == CustomerType.Retail),
            DistributorCount = all.Count(c => c.Type == CustomerType.Distributor)
        };
    }

    public async Task<string> GenerateNextCustomerCodeAsync()
    {
        var count = await _repositoryManager.CustomerRepository.GetCountAsync();
        var nextNum = count + 1;
        var code = $"CUS-{nextNum:D4}";

        while (!await _repositoryManager.CustomerRepository.IsCodeUniqueAsync(code))
        {
            nextNum++;
            code = $"CUS-{nextNum:D4}";
        }

        return code;
    }
}
