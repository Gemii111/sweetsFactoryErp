using FactoryX.Application.DTOs;
using FactoryX.Domain.Entities;

namespace FactoryX.Application.Services.Abstracts;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceDto>> GetAllInvoicesAsync(
        InvoiceStatus? status = null,
        int? customerId = null,
        int? salesOrderId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null);

    Task<InvoiceDto?> GetInvoiceByIdAsync(int id);
    Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber);
    Task<IEnumerable<InvoiceableOrderDto>> GetInvoiceableOrdersAsync(int? customerId = null);
    Task<InvoiceDto> CreateInvoiceAsync(CreateInvoiceRequest request, int userId);
    Task<InvoiceDto> IssueInvoiceAsync(int invoiceId, int userId);
    Task<bool> CancelInvoiceAsync(int invoiceId, string reason, int userId);
    Task<InvoiceSummaryDto> GetSummaryAsync();
    Task<string> GenerateNextInvoiceNumberAsync(DateTime? date = null);
}
