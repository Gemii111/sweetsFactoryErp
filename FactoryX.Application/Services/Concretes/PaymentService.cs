using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;
using FactoryX.Infrastructure;

namespace FactoryX.Application.Services.Concretes;

public class PaymentService : IPaymentService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IAccountingPostingService _postingService;
    private readonly IMapper _mapper;
    private readonly AppDbContext _context;

    public PaymentService(
        IRepositoryManager repositoryManager,
        IAccountingPostingService postingService,
        IMapper mapper,
        AppDbContext context)
    {
        _repositoryManager = repositoryManager;
        _postingService = postingService;
        _mapper = mapper;
        _context = context;
    }

    public async Task<IEnumerable<PaymentDto>> GetAllPaymentsAsync(
        int? invoiceId = null,
        int? customerId = null,
        PaymentMethod? method = null,
        PaymentStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? searchTerm = null)
    {
        var payments = await _repositoryManager.PaymentRepository.GetAllPaymentsAsync(
            invoiceId, customerId, method, status, fromDate, toDate, searchTerm);

        return _mapper.Map<IEnumerable<PaymentDto>>(payments);
    }

    public async Task<PaymentDto?> GetPaymentByIdAsync(int id)
    {
        var payment = await _repositoryManager.PaymentRepository.GetByIdWithDetailsAsync(id);
        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto?> GetPaymentByNumberAsync(string paymentNumber)
    {
        var payment = await _repositoryManager.PaymentRepository.GetByPaymentNumberAsync(paymentNumber);
        return _mapper.Map<PaymentDto>(payment);
    }

    public async Task<PaymentDto> CreatePaymentAsync(CreatePaymentRequest request, int userId)
    {
        if (request.Amount <= 0)
        {
            throw new InvalidOperationException("يجب أن يكون مبلغ السداد أكبر من الصفر.");
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Lock and retrieve Invoice
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == request.InvoiceId);

            if (invoice == null)
            {
                throw new InvalidOperationException("الفاتورة المراد سدادها غير موجودة.");
            }

            if (invoice.Status == InvoiceStatus.Draft)
            {
                throw new InvalidOperationException("لا يمكن سداد فاتورة ما زالت في حالة مسودة. يرجى اعتماد وإصدار الفاتورة أولاً.");
            }

            if (invoice.Status == InvoiceStatus.Cancelled)
            {
                throw new InvalidOperationException("لا يمكن تسجيل مدفوعات على فاتورة ملغاة.");
            }

            if (invoice.Status == InvoiceStatus.Paid || invoice.RemainingAmount <= 0)
            {
                throw new InvalidOperationException("هذه الفاتورة مسددة بالكامل بالفعل ولا يوجد رصيد متبقٍ عليها.");
            }

            // 2. Overpayment Protection
            if (request.Amount > invoice.RemainingAmount)
            {
                throw new InvalidOperationException(
                    $"مبلغ السداد المطلوب ({request.Amount:N2} EGP) يتجاوز الرصيد المتبقي على الفاتورة ({invoice.RemainingAmount:N2} EGP).");
            }

            var user = await _repositoryManager.UserRepository.GetByIdAsync(userId);
            var paymentNumber = await GenerateNextPaymentNumberAsync(request.PaymentDate);

            // 3. Create Payment record
            var payment = new Payment
            {
                PaymentNumber = paymentNumber,
                InvoiceId = invoice.Id,
                CustomerId = invoice.CustomerId,
                PaymentDate = request.PaymentDate.Date,
                Amount = request.Amount,
                Currency = string.IsNullOrWhiteSpace(request.Currency) ? invoice.Currency : request.Currency.Trim(),
                PaymentMethod = request.PaymentMethod,
                ReferenceNumber = request.ReferenceNumber,
                Status = PaymentStatus.Recorded,
                Notes = request.Notes,
                ReceivedByUserId = userId,
                ReceivedByName = user?.FullName ?? user?.Username ?? "المسؤول"
            };

            _context.Payments.Add(payment);

            // 4. Update Invoice PaidAmount, RemainingAmount, and Status atomically
            invoice.PaidAmount += request.Amount;
            invoice.RemainingAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);

            if (invoice.RemainingAmount == 0)
            {
                invoice.Status = InvoiceStatus.Paid;
            }
            else if (invoice.PaidAmount > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Automatic Accounting Posting
            await _postingService.PostCustomerPaymentAsync(payment.Id, userId);

            return await GetPaymentByIdAsync(payment.Id) ?? _mapper.Map<PaymentDto>(payment);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> VoidPaymentAsync(VoidPaymentRequest request, int userId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .FirstOrDefaultAsync(p => p.Id == request.PaymentId);

            if (payment == null)
            {
                throw new InvalidOperationException("سند القبض / السداد غير موجود.");
            }

            if (payment.Status == PaymentStatus.Voided)
            {
                throw new InvalidOperationException("هذا السند ملغي بالفعل مسبقاً.");
            }

            var invoice = payment.Invoice;
            if (invoice == null)
            {
                invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.Id == payment.InvoiceId);
            }

            if (invoice == null)
            {
                throw new InvalidOperationException("الفاتورة المرتبطة بالسند غير موجودة.");
            }

            // 1. Mark Payment as Voided
            payment.Status = PaymentStatus.Voided;
            payment.VoidReason = request.Reason;
            payment.VoidedAt = DateTime.UtcNow;
            payment.VoidedByUserId = userId;

            // 2. Revert Invoice PaidAmount and RemainingAmount
            invoice.PaidAmount = Math.Max(0, invoice.PaidAmount - payment.Amount);
            invoice.RemainingAmount = Math.Max(0, invoice.TotalAmount - invoice.PaidAmount);

            if (invoice.PaidAmount == 0)
            {
                invoice.Status = InvoiceStatus.Issued;
            }
            else if (invoice.RemainingAmount > 0)
            {
                invoice.Status = InvoiceStatus.PartiallyPaid;
            }
            else
            {
                invoice.Status = InvoiceStatus.Paid;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Reverse Accounting Journal if posted
            var existingJournal = await _repositoryManager.JournalEntryRepository
                .GetByReferenceAsync(JournalReferenceType.CustomerPayment, payment.Id);
            if (existingJournal != null && existingJournal.Status == JournalEntryStatus.Posted)
            {
                await _postingService.ReverseJournalEntryAsync(existingJournal.Id, request.Reason, userId);
            }

            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<PaymentSummaryDto> GetSummaryAsync()
    {
        var payments = await _context.Payments.AsNoTracking().ToListAsync();

        return new PaymentSummaryDto
        {
            TotalPayments = payments.Count,
            TotalRecordedAmount = payments.Where(p => p.Status == PaymentStatus.Recorded).Sum(p => p.Amount),
            TotalVoidedAmount = payments.Where(p => p.Status == PaymentStatus.Voided).Sum(p => p.Amount),
            CashAmount = payments.Where(p => p.Status == PaymentStatus.Recorded && p.PaymentMethod == PaymentMethod.Cash).Sum(p => p.Amount),
            BankTransferAmount = payments.Where(p => p.Status == PaymentStatus.Recorded && p.PaymentMethod == PaymentMethod.BankTransfer).Sum(p => p.Amount),
            CardAmount = payments.Where(p => p.Status == PaymentStatus.Recorded && p.PaymentMethod == PaymentMethod.Card).Sum(p => p.Amount),
            ChequeAmount = payments.Where(p => p.Status == PaymentStatus.Recorded && p.PaymentMethod == PaymentMethod.Cheque).Sum(p => p.Amount),
            OtherAmount = payments.Where(p => p.Status == PaymentStatus.Recorded && p.PaymentMethod == PaymentMethod.Other).Sum(p => p.Amount)
        };
    }

    public async Task<string> GenerateNextPaymentNumberAsync(DateTime? date = null)
    {
        var targetDate = date ?? DateTime.UtcNow;
        var datePrefix = targetDate.ToString("yyyyMMdd");
        var prefix = $"PAY-{datePrefix}-";

        var latestToday = await _context.Payments
            .AsNoTracking()
            .Where(p => p.PaymentNumber.StartsWith(prefix))
            .OrderByDescending(p => p.PaymentNumber)
            .Select(p => p.PaymentNumber)
            .FirstOrDefaultAsync();

        int nextSeq = 1;
        if (!string.IsNullOrEmpty(latestToday) && latestToday.Length >= prefix.Length + 4)
        {
            var seqPart = latestToday.Substring(prefix.Length);
            if (int.TryParse(seqPart, out int parsed))
            {
                nextSeq = parsed + 1;
            }
        }

        return $"{prefix}{nextSeq:D4}";
    }
}
