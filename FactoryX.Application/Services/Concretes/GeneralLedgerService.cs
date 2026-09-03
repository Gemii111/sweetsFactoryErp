using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class GeneralLedgerService : IGeneralLedgerService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IAccountService _accountService;

    public GeneralLedgerService(IRepositoryManager repositoryManager, IAccountService accountService)
    {
        _repositoryManager = repositoryManager;
        _accountService = accountService;
    }

    public async Task<IEnumerable<GeneralLedgerAccountDto>> GetGeneralLedgerAsync(GeneralLedgerQueryDto query)
    {
        await _accountService.SeedDefaultChartOfAccountsAsync();
        var accountsQuery = (await _repositoryManager.AccountRepository.GetAllAsync()).AsQueryable();

        if (query.AccountId.HasValue && query.AccountId.Value > 0)
        {
            accountsQuery = accountsQuery.Where(a => a.Id == query.AccountId.Value);
        }
        else if (query.AccountType.HasValue)
        {
            accountsQuery = accountsQuery.Where(a => a.AccountType == query.AccountType.Value);
        }

        var accounts = accountsQuery.OrderBy(a => a.AccountCode).ToList();
        var result = new List<GeneralLedgerAccountDto>();

        var fromDate = query.FromDate?.Date;
        var toDate = query.ToDate?.Date;

        var allPostedLines = await _repositoryManager.JournalEntryRepository.GetPostedLinesQueryable().ToListAsync();

        foreach (var account in accounts)
        {
            var isDebitNormal = account.AccountType == AccountType.Asset || account.AccountType == AccountType.Expense;

            // 1. Opening Balance Calculation (All posted transactions before fromDate)
            decimal openingDebit = 0;
            decimal openingCredit = 0;

            if (fromDate.HasValue)
            {
                var preLines = allPostedLines.Where(l => l.AccountId == account.Id &&
                                                         l.JournalEntry != null &&
                                                         l.JournalEntry.EntryDate.Date < fromDate.Value);
                if (query.CustomerId.HasValue && query.CustomerId.Value > 0)
                {
                    preLines = preLines.Where(l => l.CustomerId == query.CustomerId.Value);
                }
                if (query.SupplierId.HasValue && query.SupplierId.Value > 0)
                {
                    preLines = preLines.Where(l => l.SupplierId == query.SupplierId.Value);
                }

                openingDebit = preLines.Sum(l => l.Debit);
                openingCredit = preLines.Sum(l => l.Credit);
            }

            var openingBalance = isDebitNormal ? (openingDebit - openingCredit) : (openingCredit - openingDebit);

            // 2. Period Lines
            var periodLines = allPostedLines.Where(l => l.AccountId == account.Id && l.JournalEntry != null);
            if (fromDate.HasValue)
            {
                periodLines = periodLines.Where(l => l.JournalEntry!.EntryDate.Date >= fromDate.Value);
            }
            if (toDate.HasValue)
            {
                periodLines = periodLines.Where(l => l.JournalEntry!.EntryDate.Date <= toDate.Value);
            }
            if (query.CustomerId.HasValue && query.CustomerId.Value > 0)
            {
                periodLines = periodLines.Where(l => l.CustomerId == query.CustomerId.Value);
            }
            if (query.SupplierId.HasValue && query.SupplierId.Value > 0)
            {
                periodLines = periodLines.Where(l => l.SupplierId == query.SupplierId.Value);
            }

            var orderedLines = periodLines
                .OrderBy(l => l.JournalEntry!.EntryDate)
                .ThenBy(l => l.JournalEntryId)
                .ToList();

            // Skip accounts with zero opening and zero period activity if querying all
            if (!query.AccountId.HasValue && openingBalance == 0 && !orderedLines.Any())
            {
                continue;
            }

            var accDto = new GeneralLedgerAccountDto
            {
                AccountId = account.Id,
                AccountCode = account.AccountCode,
                AccountName = account.AccountName,
                AccountNameAr = account.AccountNameAr,
                AccountType = account.AccountType,
                OpeningBalance = openingBalance,
                TotalDebit = orderedLines.Sum(l => l.Debit),
                TotalCredit = orderedLines.Sum(l => l.Credit)
            };

            decimal runningBal = openingBalance;

            foreach (var line in orderedLines)
            {
                if (isDebitNormal)
                {
                    runningBal += (line.Debit - line.Credit);
                }
                else
                {
                    runningBal += (line.Credit - line.Debit);
                }

                accDto.Rows.Add(new GeneralLedgerRowDto
                {
                    JournalEntryId = line.JournalEntryId,
                    JournalNumber = line.JournalEntry?.JournalNumber ?? $"JE-{line.JournalEntryId}",
                    Date = line.JournalEntry?.EntryDate ?? DateTime.UtcNow,
                    Description = line.Description ?? line.JournalEntry?.Description ?? string.Empty,
                    ReferenceDocumentNumber = line.JournalEntry?.ReferenceDocumentNumber ?? line.ReferenceNumber,
                    ReferenceType = line.JournalEntry?.ReferenceType ?? JournalReferenceType.Manual,
                    Debit = line.Debit,
                    Credit = line.Credit,
                    RunningBalance = runningBal,
                    CustomerName = line.Customer?.Name,
                    SupplierName = line.Supplier?.Name
                });
            }

            accDto.ClosingBalance = runningBal;
            result.Add(accDto);
        }

        return result;
    }

    public async Task<TrialBalanceDto> GetTrialBalanceAsync(TrialBalanceQueryDto query)
    {
        await _accountService.SeedDefaultChartOfAccountsAsync();
        var asOf = (query.AsOfDate ?? DateTime.UtcNow).Date;
        var fromDate = query.FromDate?.Date;

        var allAccounts = (await _repositoryManager.AccountRepository.GetAllAsync())
            .Where(a => a.IsActive && !a.IsControlAccount)
            .OrderBy(a => a.AccountCode)
            .ToList();

        if (query.AccountType.HasValue)
        {
            allAccounts = allAccounts.Where(a => a.AccountType == query.AccountType.Value).ToList();
        }

        var allPostedLines = await _repositoryManager.JournalEntryRepository.GetPostedLinesQueryable().ToListAsync();

        var tb = new TrialBalanceDto
        {
            AsOfDate = asOf,
            FromDate = fromDate
        };

        foreach (var acc in allAccounts)
        {
            decimal openingDebit = 0;
            decimal openingCredit = 0;

            if (fromDate.HasValue)
            {
                var preLines = allPostedLines.Where(l => l.AccountId == acc.Id &&
                                                         l.JournalEntry != null &&
                                                         l.JournalEntry.EntryDate.Date < fromDate.Value);
                openingDebit = preLines.Sum(l => l.Debit);
                openingCredit = preLines.Sum(l => l.Credit);
            }

            var periodLines = allPostedLines.Where(l => l.AccountId == acc.Id && l.JournalEntry != null);
            if (fromDate.HasValue)
            {
                periodLines = periodLines.Where(l => l.JournalEntry!.EntryDate.Date >= fromDate.Value &&
                                                     l.JournalEntry!.EntryDate.Date <= asOf);
            }
            else
            {
                periodLines = periodLines.Where(l => l.JournalEntry!.EntryDate.Date <= asOf);
            }

            var periodDebit = periodLines.Sum(l => l.Debit);
            var periodCredit = periodLines.Sum(l => l.Credit);

            var totalDebit = openingDebit + periodDebit;
            var totalCredit = openingCredit + periodCredit;

            decimal closingDebit = 0;
            decimal closingCredit = 0;

            if (totalDebit >= totalCredit)
            {
                closingDebit = totalDebit - totalCredit;
            }
            else
            {
                closingCredit = totalCredit - totalDebit;
            }

            // Skip zero balance accounts if not queried specifically
            if (openingDebit == 0 && openingCredit == 0 && periodDebit == 0 && periodCredit == 0)
            {
                continue;
            }

            var row = new TrialBalanceRowDto
            {
                AccountId = acc.Id,
                AccountCode = acc.AccountCode,
                AccountName = acc.AccountName,
                AccountNameAr = acc.AccountNameAr,
                AccountType = acc.AccountType,
                IsControlAccount = acc.IsControlAccount,
                OpeningDebit = openingDebit,
                OpeningCredit = openingCredit,
                PeriodDebit = periodDebit,
                PeriodCredit = periodCredit,
                ClosingDebit = closingDebit,
                ClosingCredit = closingCredit
            };

            tb.Rows.Add(row);
            tb.TotalOpeningDebit += openingDebit;
            tb.TotalOpeningCredit += openingCredit;
            tb.TotalPeriodDebit += periodDebit;
            tb.TotalPeriodCredit += periodCredit;
            tb.TotalClosingDebit += closingDebit;
            tb.TotalClosingCredit += closingCredit;
        }

        return tb;
    }

    public async Task<CustomerLedgerDto?> GetCustomerLedgerAsync(int customerId, DateTime? fromDate, DateTime? toDate)
    {
        var customer = await _repositoryManager.CustomerRepository.GetByIdAsync(customerId);
        if (customer == null) return null;

        var arAccountId = await _repositoryManager.AccountingSettingRepository.GetAccountIdByRoleAsync(AccountRole.AccountsReceivable);
        if (!arAccountId.HasValue)
        {
            await _accountService.SeedDefaultChartOfAccountsAsync();
            arAccountId = await _repositoryManager.AccountingSettingRepository.GetAccountIdByRoleAsync(AccountRole.AccountsReceivable);
        }

        var allLines = await _repositoryManager.JournalEntryRepository.GetPostedLinesQueryable()
            .Where(l => l.AccountId == arAccountId && l.CustomerId == customerId)
            .ToListAsync();

        var from = fromDate?.Date;
        var to = toDate?.Date;

        decimal openingDebit = 0;
        decimal openingCredit = 0;

        if (from.HasValue)
        {
            var pre = allLines.Where(l => l.JournalEntry != null && l.JournalEntry.EntryDate.Date < from.Value);
            openingDebit = pre.Sum(l => l.Debit);
            openingCredit = pre.Sum(l => l.Credit);
        }

        var openingBal = openingDebit - openingCredit;

        var periodLines = allLines.AsQueryable();
        if (from.HasValue) periodLines = periodLines.Where(l => l.JournalEntry!.EntryDate.Date >= from.Value);
        if (to.HasValue) periodLines = periodLines.Where(l => l.JournalEntry!.EntryDate.Date <= to.Value);

        var ordered = periodLines
            .OrderBy(l => l.JournalEntry!.EntryDate)
            .ThenBy(l => l.JournalEntryId)
            .ToList();

        var dto = new CustomerLedgerDto
        {
            CustomerId = customer.Id,
            CustomerName = customer.Name,
            CustomerCode = customer.Code,
            OpeningBalance = openingBal,
            TotalInvoicedDebit = ordered.Sum(l => l.Debit),
            TotalPaidCredit = ordered.Sum(l => l.Credit)
        };

        decimal running = openingBal;
        foreach (var l in ordered)
        {
            running += (l.Debit - l.Credit);
            dto.Rows.Add(new CustomerLedgerRowDto
            {
                JournalEntryId = l.JournalEntryId,
                JournalNumber = l.JournalEntry?.JournalNumber ?? string.Empty,
                Date = l.JournalEntry?.EntryDate ?? DateTime.UtcNow,
                DocumentType = l.JournalEntry?.ReferenceType.ToString() ?? "Journal",
                DocumentNumber = l.JournalEntry?.ReferenceDocumentNumber ?? l.ReferenceNumber,
                Description = l.Description ?? l.JournalEntry?.Description ?? string.Empty,
                Debit = l.Debit,
                Credit = l.Credit,
                RunningBalance = running
            });
        }

        dto.OutstandingReceivable = running;
        return dto;
    }

    public async Task<SupplierLedgerDto?> GetSupplierLedgerAsync(int supplierId, DateTime? fromDate, DateTime? toDate)
    {
        var supplier = await _repositoryManager.SupplierRepository.GetByIdAsync(supplierId);
        if (supplier == null) return null;

        var apAccountId = await _repositoryManager.AccountingSettingRepository.GetAccountIdByRoleAsync(AccountRole.AccountsPayable);
        if (!apAccountId.HasValue)
        {
            await _accountService.SeedDefaultChartOfAccountsAsync();
            apAccountId = await _repositoryManager.AccountingSettingRepository.GetAccountIdByRoleAsync(AccountRole.AccountsPayable);
        }

        var allLines = await _repositoryManager.JournalEntryRepository.GetPostedLinesQueryable()
            .Where(l => l.AccountId == apAccountId && l.SupplierId == supplierId)
            .ToListAsync();

        var from = fromDate?.Date;
        var to = toDate?.Date;

        decimal openingDebit = 0;
        decimal openingCredit = 0;

        if (from.HasValue)
        {
            var pre = allLines.Where(l => l.JournalEntry != null && l.JournalEntry.EntryDate.Date < from.Value);
            openingDebit = pre.Sum(l => l.Debit);
            openingCredit = pre.Sum(l => l.Credit);
        }

        // For Liabilities (AP), normal balance is Credit
        var openingBal = openingCredit - openingDebit;

        var periodLines = allLines.AsQueryable();
        if (from.HasValue) periodLines = periodLines.Where(l => l.JournalEntry!.EntryDate.Date >= from.Value);
        if (to.HasValue) periodLines = periodLines.Where(l => l.JournalEntry!.EntryDate.Date <= to.Value);

        var ordered = periodLines
            .OrderBy(l => l.JournalEntry!.EntryDate)
            .ThenBy(l => l.JournalEntryId)
            .ToList();

        var dto = new SupplierLedgerDto
        {
            SupplierId = supplier.Id,
            SupplierName = supplier.Name,
            SupplierCode = supplier.Code,
            OpeningBalance = openingBal,
            TotalPaymentDebit = ordered.Sum(l => l.Debit),
            TotalPurchaseCredit = ordered.Sum(l => l.Credit)
        };

        decimal running = openingBal;
        foreach (var l in ordered)
        {
            running += (l.Credit - l.Debit);
            dto.Rows.Add(new SupplierLedgerRowDto
            {
                JournalEntryId = l.JournalEntryId,
                JournalNumber = l.JournalEntry?.JournalNumber ?? string.Empty,
                Date = l.JournalEntry?.EntryDate ?? DateTime.UtcNow,
                DocumentType = l.JournalEntry?.ReferenceType.ToString() ?? "Journal",
                DocumentNumber = l.JournalEntry?.ReferenceDocumentNumber ?? l.ReferenceNumber,
                Description = l.Description ?? l.JournalEntry?.Description ?? string.Empty,
                Debit = l.Debit,
                Credit = l.Credit,
                RunningBalance = running
            });
        }

        dto.OutstandingPayable = running;
        return dto;
    }
}
