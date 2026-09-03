using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class CustomerStatementService : ICustomerStatementService
{
    private readonly AppDbContext _context;

    public CustomerStatementService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CustomerStatementDto?> GetCustomerStatementAsync(int customerId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var customer = await _context.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null) return null;

        var invoicesQuery = _context.Invoices
            .AsNoTracking()
            .Where(i => i.CustomerId == customerId && i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Draft);

        var paymentsQuery = _context.Payments
            .AsNoTracking()
            .Include(p => p.Invoice)
            .Where(p => p.CustomerId == customerId && p.Status == PaymentStatus.Recorded);

        // 1. Calculate Opening Balance if fromDate is specified
        decimal openingBalance = 0;
        if (fromDate.HasValue)
        {
            var priorInvoiced = await invoicesQuery
                .Where(i => i.InvoiceDate < fromDate.Value.Date)
                .SumAsync(i => i.TotalAmount);

            var priorPaid = await paymentsQuery
                .Where(p => p.PaymentDate < fromDate.Value.Date)
                .SumAsync(p => p.Amount);

            openingBalance = priorInvoiced - priorPaid;
        }

        // 2. Filter period documents
        var periodInvoices = await invoicesQuery
            .Where(i => (!fromDate.HasValue || i.InvoiceDate >= fromDate.Value.Date) &&
                        (!toDate.HasValue || i.InvoiceDate <= toDate.Value.Date.AddDays(1).AddTicks(-1)))
            .ToListAsync();

        var periodPayments = await paymentsQuery
            .Where(p => (!fromDate.HasValue || p.PaymentDate >= fromDate.Value.Date) &&
                        (!toDate.HasValue || p.PaymentDate <= toDate.Value.Date.AddDays(1).AddTicks(-1)))
            .ToListAsync();

        // 3. Build unified timeline
        var rawLines = new List<CustomerStatementLineDto>();

        foreach (var inv in periodInvoices)
        {
            rawLines.Add(new CustomerStatementLineDto
            {
                Date = inv.InvoiceDate,
                DocumentNumber = inv.InvoiceNumber,
                DocumentType = "فاتورة بيع",
                Description = $"فاتورة بيع لأمر رقم [{inv.SalesOrderId}]",
                Reference = $"حالة الفاتورة: {inv.Status}",
                DebitAmount = inv.TotalAmount,
                CreditAmount = 0
            });
        }

        foreach (var pay in periodPayments)
        {
            rawLines.Add(new CustomerStatementLineDto
            {
                Date = pay.PaymentDate,
                DocumentNumber = pay.PaymentNumber,
                DocumentType = "سند قبض وسداد",
                Description = $"سداد لفاتورة رقم [{pay.Invoice?.InvoiceNumber ?? pay.InvoiceId.ToString()}] ({pay.PaymentMethod})",
                Reference = pay.ReferenceNumber,
                DebitAmount = 0,
                CreditAmount = pay.Amount
            });
        }

        // 4. Sort and calculate running balances
        var sortedLines = rawLines.OrderBy(l => l.Date).ThenBy(l => l.DocumentNumber).ToList();
        decimal currentRunning = openingBalance;

        foreach (var line in sortedLines)
        {
            currentRunning += (line.DebitAmount - line.CreditAmount);
            line.RunningBalance = currentRunning;
        }

        decimal totalInvoicedInPeriod = sortedLines.Sum(l => l.DebitAmount);
        decimal totalPaidInPeriod = sortedLines.Sum(l => l.CreditAmount);

        return new CustomerStatementDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerCode = customer.Code,
            CustomerType = customer.Type.ToString(),
            Phone = customer.Phone ?? customer.Mobile,
            Email = customer.Email,
            Address = customer.Address,
            CreditLimit = customer.CreditLimit,
            FromDate = fromDate,
            ToDate = toDate,
            OpeningBalance = openingBalance,
            TotalInvoiced = totalInvoicedInPeriod,
            TotalPaid = totalPaidInPeriod,
            ClosingBalance = currentRunning,
            Lines = sortedLines
        };
    }

    public async Task<IEnumerable<CustomerBalanceSummaryDto>> GetAllCustomerBalancesAsync(string? searchTerm = null)
    {
        var query = _context.Customers
            .AsNoTracking()
            .Include(c => c.Invoices)
            .Include(c => c.Payments)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var clean = searchTerm.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(clean) || c.Code.ToLower().Contains(clean));
        }

        var customers = await query.OrderBy(c => c.Name).ToListAsync();

        return customers.Select(c =>
        {
            var validInvoices = c.Invoices?.Where(i => i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Draft).ToList() ?? new();
            var validPayments = c.Payments?.Where(p => p.Status == PaymentStatus.Recorded).ToList() ?? new();

            var totalInvoiced = validInvoices.Sum(i => i.TotalAmount);
            var totalPaid = validPayments.Sum(p => p.Amount);

            return new CustomerBalanceSummaryDto
            {
                CustomerId = c.Id,
                CustomerName = c.Name,
                CustomerCode = c.Code,
                CustomerType = c.Type.ToString(),
                Phone = c.Phone ?? c.Mobile,
                CreditLimit = c.CreditLimit,
                TotalInvoiced = totalInvoiced,
                TotalPaid = totalPaid,
                InvoicesCount = validInvoices.Count,
                PaymentsCount = validPayments.Count
            };
        });
    }

    public async Task<CustomerBalanceSummaryDto?> GetCustomerBalanceSummaryAsync(int customerId)
    {
        var c = await _context.Customers
            .AsNoTracking()
            .Include(c => c.Invoices)
            .Include(c => c.Payments)
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (c == null) return null;

        var validInvoices = c.Invoices?.Where(i => i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Draft).ToList() ?? new();
        var validPayments = c.Payments?.Where(p => p.Status == PaymentStatus.Recorded).ToList() ?? new();

        var totalInvoiced = validInvoices.Sum(i => i.TotalAmount);
        var totalPaid = validPayments.Sum(p => p.Amount);

        return new CustomerBalanceSummaryDto
        {
            CustomerId = c.Id,
            CustomerName = c.Name,
            CustomerCode = c.Code,
            CustomerType = c.Type.ToString(),
            Phone = c.Phone ?? c.Mobile,
            CreditLimit = c.CreditLimit,
            TotalInvoiced = totalInvoiced,
            TotalPaid = totalPaid,
            InvoicesCount = validInvoices.Count,
            PaymentsCount = validPayments.Count
        };
    }
}
