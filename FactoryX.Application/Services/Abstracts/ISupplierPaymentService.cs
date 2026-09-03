using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface ISupplierPaymentService
{
    Task<IEnumerable<SupplierPaymentDto>> GetAllPaymentsAsync();
    Task<SupplierPaymentDto?> GetPaymentByIdAsync(int id);
    Task<IEnumerable<SupplierPaymentDto>> GetPaymentsBySupplierAsync(int supplierId);
    Task<SupplierPaymentDto> RecordPaymentAsync(SupplierPaymentCreateDto dto, int userId);
}
