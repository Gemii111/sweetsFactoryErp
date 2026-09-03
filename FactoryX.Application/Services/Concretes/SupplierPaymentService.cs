using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;

namespace FactoryX.Application.Services.Concretes;

public class SupplierPaymentService : ISupplierPaymentService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IAccountingPostingService _postingService;
    private readonly IMapper _mapper;

    public SupplierPaymentService(
        IRepositoryManager repositoryManager,
        IAccountingPostingService postingService,
        IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _postingService = postingService;
        _mapper = mapper;
    }

    public async Task<IEnumerable<SupplierPaymentDto>> GetAllPaymentsAsync()
    {
        var payments = await _repositoryManager.SupplierPaymentRepository.GetAllWithDetailsAsync();
        return _mapper.Map<IEnumerable<SupplierPaymentDto>>(payments);
    }

    public async Task<SupplierPaymentDto?> GetPaymentByIdAsync(int id)
    {
        var payment = await _repositoryManager.SupplierPaymentRepository.GetWithDetailsAsync(id);
        return payment == null ? null : _mapper.Map<SupplierPaymentDto>(payment);
    }

    public async Task<IEnumerable<SupplierPaymentDto>> GetPaymentsBySupplierAsync(int supplierId)
    {
        var payments = await _repositoryManager.SupplierPaymentRepository.GetBySupplierIdAsync(supplierId);
        return _mapper.Map<IEnumerable<SupplierPaymentDto>>(payments);
    }

    public async Task<SupplierPaymentDto> RecordPaymentAsync(SupplierPaymentCreateDto dto, int userId)
    {
        var supplier = await _repositoryManager.SupplierRepository.GetByIdAsync(dto.SupplierId);
        if (supplier == null)
        {
            throw new KeyNotFoundException($"المورد رقم #{dto.SupplierId} غير موجود.");
        }

        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException("يجب أن يكون مبلغ السداد أكبر من الصفر.");
        }

        var paymentNumber = await _repositoryManager.SupplierPaymentRepository.GenerateNextPaymentNumberAsync(dto.PaymentDate);

        var payment = new SupplierPayment
        {
            PaymentNumber = paymentNumber,
            SupplierId = dto.SupplierId,
            PurchaseReceiptId = dto.PurchaseReceiptId > 0 ? dto.PurchaseReceiptId : null,
            PurchaseOrderId = dto.PurchaseOrderId > 0 ? dto.PurchaseOrderId : null,
            PaymentDate = dto.PaymentDate.Date,
            Amount = dto.Amount,
            Currency = string.IsNullOrWhiteSpace(dto.Currency) ? "EGP" : dto.Currency.Trim(),
            PaymentMethod = dto.PaymentMethod,
            ReferenceNumber = dto.ReferenceNumber?.Trim(),
            Notes = dto.Notes?.Trim(),
            Status = PaymentStatus.Recorded,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryManager.SupplierPaymentRepository.Create(payment);
        await _repositoryManager.SaveAsync();

        // Automatic Accounting Posting: Dr AP, Cr Cash/Bank
        await _postingService.PostSupplierPaymentAsync(payment.Id, userId);

        var createdWithDetails = await _repositoryManager.SupplierPaymentRepository.GetWithDetailsAsync(payment.Id);
        return _mapper.Map<SupplierPaymentDto>(createdWithDetails);
    }
}
