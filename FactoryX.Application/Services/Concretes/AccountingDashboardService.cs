using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class AccountingDashboardService : IAccountingDashboardService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IAccountService _accountService;
    private readonly IAccountingPeriodService _periodService;
    private readonly IMapper _mapper;

    public AccountingDashboardService(
        IRepositoryManager repositoryManager,
        IAccountService accountService,
        IAccountingPeriodService periodService,
        IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _accountService = accountService;
        _periodService = periodService;
        _mapper = mapper;
    }

    private async Task<decimal> GetAccountRoleBalanceAsync(AccountRole role, List<JournalEntryLine> allPostedLines)
    {
        var accountId = await _repositoryManager.AccountingSettingRepository.GetAccountIdByRoleAsync(role);
        if (!accountId.HasValue) return 0;

        var lines = allPostedLines.Where(l => l.AccountId == accountId.Value).ToList();
        if (!lines.Any()) return 0;

        var accType = lines.First().Account?.AccountType ?? AccountType.Asset;
        var debits = lines.Sum(l => l.Debit);
        var credits = lines.Sum(l => l.Credit);

        return (accType == AccountType.Asset || accType == AccountType.Expense)
            ? (debits - credits)
            : (credits - debits);
    }

    public async Task<AccountingDashboardDto> GetDashboardDataAsync(int? periodId = null)
    {
        await _accountService.SeedDefaultChartOfAccountsAsync();
        await _periodService.EnsureOpenPeriodExistsAsync();

        var periods = await _repositoryManager.AccountingPeriodRepository.GetAllAsync();
        var currentPeriod = periodId.HasValue && periodId.Value > 0
            ? periods.FirstOrDefault(p => p.Id == periodId.Value)
            : periods.FirstOrDefault(p => p.Status == AccountingPeriodStatus.Open) ?? periods.OrderByDescending(p => p.StartDate).FirstOrDefault();

        var query = _repositoryManager.JournalEntryRepository.GetPostedLinesQueryable();
        if (currentPeriod != null)
        {
            query = query.Where(l => l.JournalEntry!.AccountingPeriodId == currentPeriod.Id);
        }

        var postedLines = await query.ToListAsync();

        var revenue = await GetAccountRoleBalanceAsync(AccountRole.SalesRevenue, postedLines);
        var cogs = await GetAccountRoleBalanceAsync(AccountRole.CostOfGoodsSold, postedLines);
        var waste = await GetAccountRoleBalanceAsync(AccountRole.WasteExpense, postedLines);
        
        var arBalance = await GetAccountRoleBalanceAsync(AccountRole.AccountsReceivable, postedLines);
        var apBalance = await GetAccountRoleBalanceAsync(AccountRole.AccountsPayable, postedLines);
        
        var cash = await GetAccountRoleBalanceAsync(AccountRole.Cash, postedLines);
        var bank = await GetAccountRoleBalanceAsync(AccountRole.Bank, postedLines);
        
        var outputVat = await GetAccountRoleBalanceAsync(AccountRole.OutputVat, postedLines);
        var inputVat = await GetAccountRoleBalanceAsync(AccountRole.InputVat, postedLines);

        var rawInventory = await GetAccountRoleBalanceAsync(AccountRole.RawMaterialInventory, postedLines);
        var pkgInventory = await GetAccountRoleBalanceAsync(AccountRole.PackagingInventory, postedLines);
        var fgInventory = await GetAccountRoleBalanceAsync(AccountRole.FinishedGoodsInventory, postedLines);

        var recentJournalsQuery = await _repositoryManager.JournalEntryRepository.GetPagedJournalsAsync(
            currentPeriod?.Id, null, null, JournalEntryStatus.Posted, null, null);

        var recentJournalsDto = _mapper.Map<IEnumerable<JournalEntryDto>>(recentJournalsQuery.Take(8)).ToList();

        return new AccountingDashboardDto
        {
            OpenPeriodId = currentPeriod?.Id ?? 0,
            OpenPeriodName = currentPeriod?.PeriodName ?? "All Periods",
            TotalPostedJournals = recentJournalsQuery.Count(),
            TotalRevenue = revenue,
            TotalCostOfGoodsSold = cogs,
            TotalExpenses = waste,
            AccountsReceivableBalance = arBalance,
            AccountsPayableBalance = apBalance,
            TotalCashBalance = cash,
            TotalBankBalance = bank,
            OutputVatBalance = outputVat,
            InputVatBalance = inputVat,
            RawMaterialInventoryValue = rawInventory,
            PackagingInventoryValue = pkgInventory,
            FinishedGoodsInventoryValue = fgInventory,
            TotalInventoryValue = rawInventory + pkgInventory + fgInventory,
            RecentJournals = recentJournalsDto
        };
    }

    public async Task<RevenueSummaryDto> GetRevenueSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var postedLines = await _repositoryManager.JournalEntryRepository.GetPostedLinesQueryable().ToListAsync();
        if (fromDate.HasValue) postedLines = postedLines.Where(l => l.JournalEntry!.EntryDate.Date >= fromDate.Value.Date).ToList();
        if (toDate.HasValue) postedLines = postedLines.Where(l => l.JournalEntry!.EntryDate.Date <= toDate.Value.Date).ToList();

        var revenue = await GetAccountRoleBalanceAsync(AccountRole.SalesRevenue, postedLines);
        var invoiceCount = postedLines
            .Where(l => l.JournalEntry?.ReferenceType == JournalReferenceType.SalesInvoice)
            .Select(l => l.JournalEntryId)
            .Distinct()
            .Count();

        return new RevenueSummaryDto
        {
            TotalSalesRevenue = revenue,
            TotalDiscounts = 0,
            InvoicesCount = invoiceCount
        };
    }

    public async Task<ExpenseSummaryDto> GetExpenseSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var postedLines = await _repositoryManager.JournalEntryRepository.GetPostedLinesQueryable().ToListAsync();
        if (fromDate.HasValue) postedLines = postedLines.Where(l => l.JournalEntry!.EntryDate.Date >= fromDate.Value.Date).ToList();
        if (toDate.HasValue) postedLines = postedLines.Where(l => l.JournalEntry!.EntryDate.Date <= toDate.Value.Date).ToList();

        var cogs = await GetAccountRoleBalanceAsync(AccountRole.CostOfGoodsSold, postedLines);
        var waste = await GetAccountRoleBalanceAsync(AccountRole.WasteExpense, postedLines);

        return new ExpenseSummaryDto
        {
            CostOfGoodsSold = cogs,
            WasteExpense = waste,
            OperatingExpenses = 0
        };
    }

    public async Task<VatSummaryDto> GetVatSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var postedLines = await _repositoryManager.JournalEntryRepository.GetPostedLinesQueryable().ToListAsync();
        if (fromDate.HasValue) postedLines = postedLines.Where(l => l.JournalEntry!.EntryDate.Date >= fromDate.Value.Date).ToList();
        if (toDate.HasValue) postedLines = postedLines.Where(l => l.JournalEntry!.EntryDate.Date <= toDate.Value.Date).ToList();

        var outputVat = await GetAccountRoleBalanceAsync(AccountRole.OutputVat, postedLines);
        var inputVat = await GetAccountRoleBalanceAsync(AccountRole.InputVat, postedLines);

        return new VatSummaryDto
        {
            OutputVat = outputVat,
            InputVat = inputVat
        };
    }

    public async Task<CashSummaryDto> GetCashSummaryAsync()
    {
        var postedLines = await _repositoryManager.JournalEntryRepository.GetPostedLinesQueryable().ToListAsync();

        var cash = await GetAccountRoleBalanceAsync(AccountRole.Cash, postedLines);
        var bank = await GetAccountRoleBalanceAsync(AccountRole.Bank, postedLines);
        var card = await GetAccountRoleBalanceAsync(AccountRole.CardSettlement, postedLines);
        var cheques = await GetAccountRoleBalanceAsync(AccountRole.ChequesReceivable, postedLines);

        return new CashSummaryDto
        {
            CashOnHand = cash,
            BankAccounts = bank,
            CardSettlement = card,
            ChequesReceivable = cheques
        };
    }
}
