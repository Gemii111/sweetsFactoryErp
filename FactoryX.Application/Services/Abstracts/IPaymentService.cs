using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync(
        int? invoiceId = null,
        int? customerId = null,
        PaymentMethod? method = null,
        PaymentStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<PaymentDto?> GetPaymentByIdAsync(int id);
    Task<PaymentDto?> GetPaymentByNumberAsync(string paymentNumber);
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request, int userId);
    Task<bool> VoidPaymentAsync(VoidPaymentRequest request, int userId);
    Task<PaymentSummaryDto> GetSummaryAsync();
    Task<string> GenerateNextPaymentNumberAsync(DateTime? date = null);
}
