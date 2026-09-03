using AutoMapper;
using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure.Contracts;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class AccountService : IAccountService
{
    private readonly IRepositoryManager _repositoryManager;
    private readonly IMapper _mapper;

    public AccountService(IRepositoryManager repositoryManager, IMapper mapper)
    {
        _repositoryManager = repositoryManager;
        _mapper = mapper;
    }

    public async Task<IEnumerable<AccountDto>> GetAllAccountsAsync()
    {
        await SeedDefaultChartOfAccountsAsync();
        var accounts = await _repositoryManager.AccountRepository.GetHierarchyAsync();
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<IEnumerable<AccountTreeNodeDto>> GetAccountTreeAsync()
    {
        await SeedDefaultChartOfAccountsAsync();
        var allAccounts = await _repositoryManager.AccountRepository.GetHierarchyAsync();
        var postedLines = await _repositoryManager.JournalEntryRepository.GetPostedLinesQueryable().ToListAsync();

        var balanceMap = postedLines
            .GroupBy(l => l.AccountId)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var first = g.First();
                    var type = first.Account?.AccountType ?? AccountType.Asset;
                    var debits = g.Sum(x => x.Debit);
                    var credits = g.Sum(x => x.Credit);
                    return (type == AccountType.Asset || type == AccountType.Expense) ? (debits - credits) : (credits - debits);
                });

        var rootAccounts = allAccounts.Where(a => a.ParentAccountId == null).OrderBy(a => a.AccountCode).ToList();

        var result = new List<AccountTreeNodeDto>();
        foreach (var root in rootAccounts)
        {
            result.Add(BuildTreeNode(root, allAccounts, balanceMap));
        }

        return result;
    }

    private AccountTreeNodeDto BuildTreeNode(Account account, IEnumerable<Account> allAccounts, Dictionary<int, decimal> balanceMap)
    {
        balanceMap.TryGetValue(account.Id, out decimal directBalance);

        var node = new AccountTreeNodeDto
        {
            Id = account.Id,
            AccountCode = account.AccountCode,
            AccountName = account.AccountName,
            AccountNameAr = account.AccountNameAr,
            AccountType = account.AccountType,
            IsControlAccount = account.IsControlAccount,
            IsActive = account.IsActive,
            Balance = directBalance
        };

        var children = allAccounts.Where(a => a.ParentAccountId == account.Id).OrderBy(a => a.AccountCode).ToList();
        foreach (var child in children)
        {
            var childNode = BuildTreeNode(child, allAccounts, balanceMap);
            node.Children.Add(childNode);
            node.Balance += childNode.Balance;
        }

        return node;
    }

    public async Task<IEnumerable<AccountDto>> GetActivePostableAccountsAsync()
    {
        await SeedDefaultChartOfAccountsAsync();
        var accounts = await _repositoryManager.AccountRepository.GetActivePostableAccountsAsync();
        return _mapper.Map<IEnumerable<AccountDto>>(accounts);
    }

    public async Task<AccountDto?> GetAccountByIdAsync(int id)
    {
        var account = await _repositoryManager.AccountRepository.GetWithDetailsAsync(id);
        return account == null ? null : _mapper.Map<AccountDto>(account);
    }

    public async Task<AccountDto?> GetAccountByCodeAsync(string accountCode)
    {
        var account = await _repositoryManager.AccountRepository.GetByCodeAsync(accountCode);
        return account == null ? null : _mapper.Map<AccountDto>(account);
    }

    public async Task<AccountDto> CreateAccountAsync(AccountCreateDto dto)
    {
        var codeTrimmed = dto.AccountCode.Trim();
        if (await _repositoryManager.AccountRepository.ExistsCodeAsync(codeTrimmed))
        {
            throw new InvalidOperationException($"رمز الحساب '{codeTrimmed}' مستخدم بالفعل.");
        }

        var account = new Account
        {
            AccountCode = codeTrimmed,
            AccountName = dto.AccountName.Trim(),
            AccountNameAr = dto.AccountNameAr.Trim(),
            AccountType = dto.AccountType,
            ParentAccountId = dto.ParentAccountId > 0 ? dto.ParentAccountId : null,
            IsActive = dto.IsActive,
            IsControlAccount = dto.IsControlAccount,
            AccountRole = dto.AccountRole,
            Notes = dto.Notes?.Trim()
        };

        _repositoryManager.AccountRepository.Create(account);
        await _repositoryManager.SaveAsync();

        if (dto.AccountRole != AccountRole.None)
        {
            await UpdateAccountingSettingAsync(new AccountingSettingUpdateDto
            {
                Role = dto.AccountRole,
                AccountId = account.Id
            });
        }

        return _mapper.Map<AccountDto>(account);
    }

    public async Task<AccountDto> UpdateAccountAsync(AccountUpdateDto dto)
    {
        var account = await _repositoryManager.AccountRepository.GetByIdAsync(dto.Id, trackChanges: true);
        if (account == null)
        {
            throw new KeyNotFoundException($"الحساب رقم #{dto.Id} غير موجود.");
        }

        var codeTrimmed = dto.AccountCode.Trim();
        if (await _repositoryManager.AccountRepository.ExistsCodeAsync(codeTrimmed, dto.Id))
        {
            throw new InvalidOperationException($"رمز الحساب '{codeTrimmed}' مستخدم بالفعل لحساب آخر.");
        }

        account.AccountCode = codeTrimmed;
        account.AccountName = dto.AccountName.Trim();
        account.AccountNameAr = dto.AccountNameAr.Trim();
        account.AccountType = dto.AccountType;
        account.ParentAccountId = dto.ParentAccountId > 0 ? dto.ParentAccountId : null;
        account.IsActive = dto.IsActive;
        account.IsControlAccount = dto.IsControlAccount;
        account.AccountRole = dto.AccountRole;
        account.Notes = dto.Notes?.Trim();
        account.UpdatedAt = DateTime.UtcNow;

        _repositoryManager.AccountRepository.Update(account);
        await _repositoryManager.SaveAsync();

        if (dto.AccountRole != AccountRole.None)
        {
            await UpdateAccountingSettingAsync(new AccountingSettingUpdateDto
            {
                Role = dto.AccountRole,
                AccountId = account.Id
            });
        }

        return _mapper.Map<AccountDto>(account);
    }

    public async Task ToggleActiveAsync(int id)
    {
        var account = await _repositoryManager.AccountRepository.GetByIdAsync(id, trackChanges: true);
        if (account == null)
        {
            throw new KeyNotFoundException($"الحساب رقم #{id} غير موجود.");
        }

        account.IsActive = !account.IsActive;
        account.UpdatedAt = DateTime.UtcNow;
        _repositoryManager.AccountRepository.Update(account);
        await _repositoryManager.SaveAsync();
    }

    public async Task DeleteAccountAsync(int id)
    {
        var account = await _repositoryManager.AccountRepository.GetWithDetailsAsync(id);
        if (account == null)
        {
            throw new KeyNotFoundException($"الحساب رقم #{id} غير موجود.");
        }

        if (account.ChildAccounts.Any())
        {
            throw new InvalidOperationException("لا يمكن حذف حساب رئيسي يحتوي على حسابات فرعية. قم بحذف أو نقل الحسابات الفرعية أولاً.");
        }

        if (await _repositoryManager.AccountRepository.HasJournalLinesAsync(id))
        {
            throw new InvalidOperationException("لا يمكن حذف هذا الحساب لوجود قيود يومية وحركات مسجلة عليه. يمكنك تعطيل الحساب بدلاً من حذفه.");
        }

        _repositoryManager.AccountRepository.Remove(account);
        await _repositoryManager.SaveAsync();
    }

    public async Task<IEnumerable<AccountingSettingDto>> GetAccountingSettingsAsync()
    {
        await SeedDefaultChartOfAccountsAsync();
        var settings = await _repositoryManager.AccountingSettingRepository.GetAllWithAccountsAsync();
        var result = new List<AccountingSettingDto>();

        foreach (var s in settings)
        {
            result.Add(new AccountingSettingDto
            {
                Id = s.Id,
                Role = s.Role,
                AccountId = s.AccountId,
                AccountCode = s.Account?.AccountCode ?? string.Empty,
                AccountName = s.Account?.AccountName ?? string.Empty,
                AccountNameAr = s.Account?.AccountNameAr ?? string.Empty,
                Description = s.Description
            });
        }

        return result;
    }

    public async Task UpdateAccountingSettingAsync(AccountingSettingUpdateDto dto)
    {
        var setting = await _repositoryManager.AccountingSettingRepository.GetByRoleAsync(dto.Role);
        if (setting == null)
        {
            setting = new AccountingSetting
            {
                Role = dto.Role,
                AccountId = dto.AccountId,
                Description = $"Auto-mapped account for {dto.Role}"
            };
            _repositoryManager.AccountingSettingRepository.Create(setting);
        }
        else
        {
            setting.AccountId = dto.AccountId;
            setting.UpdatedAt = DateTime.UtcNow;
            _repositoryManager.AccountingSettingRepository.Update(setting);
        }

        await _repositoryManager.SaveAsync();
    }

    public async Task SeedDefaultChartOfAccountsAsync()
    {
        var existingCount = (await _repositoryManager.AccountRepository.GetAllAsync()).Count();
        if (existingCount > 0) return;

        var defaultAccounts = new List<(string Code, string Name, string NameAr, AccountType Type, string? ParentCode, bool IsControl, AccountRole Role)>
        {
            // Assets (1000)
            ("1000", "Assets", "الأصول", AccountType.Asset, null, true, AccountRole.None),
            ("1100", "Cash and Banks", "النقدية والبنوك", AccountType.Asset, "1000", true, AccountRole.None),
            ("1101", "Main Cash on Hand", "الخزينة الرئيسية", AccountType.Asset, "1100", false, AccountRole.Cash),
            ("1102", "Main Bank Account", "الحساب البنكي الرئيسي", AccountType.Asset, "1100", false, AccountRole.Bank),
            ("1103", "Card Settlement Account", "حساب تسوية البطاقات والـ POS", AccountType.Asset, "1100", false, AccountRole.CardSettlement),
            ("1104", "Cheques Receivable", "أوراق القبض والشيكات", AccountType.Asset, "1100", false, AccountRole.ChequesReceivable),

            ("1200", "Receivables", "المدينون والعملاء", AccountType.Asset, "1000", true, AccountRole.None),
            ("1201", "Accounts Receivable", "العملاء والمدينون", AccountType.Asset, "1200", false, AccountRole.AccountsReceivable),

            ("1300", "Inventory Assets", "المخزون السلعي", AccountType.Asset, "1000", true, AccountRole.None),
            ("1301", "Raw Materials Inventory", "مخزون المواد الخام", AccountType.Asset, "1300", false, AccountRole.RawMaterialInventory),
            ("1302", "Packaging Materials Inventory", "مخزون مواد التعبئة والتغليف", AccountType.Asset, "1300", false, AccountRole.PackagingInventory),
            ("1303", "Finished Goods Inventory", "مخزون الإنتاج التام", AccountType.Asset, "1300", false, AccountRole.FinishedGoodsInventory),

            ("1400", "Tax Assets", "الأصول الضريبية", AccountType.Asset, "1000", true, AccountRole.None),
            ("1401", "Input VAT (Purchases)", "ضريبة القيمة المضافة على المشتريات (مدخلات)", AccountType.Asset, "1400", false, AccountRole.InputVat),

            // Liabilities (2000)
            ("2000", "Liabilities", "الالتزامات والخصوم", AccountType.Liability, null, true, AccountRole.None),
            ("2100", "Payables", "الدائنون والموردون", AccountType.Liability, "2000", true, AccountRole.None),
            ("2101", "Accounts Payable", "الموردون والدائنون", AccountType.Liability, "2100", false, AccountRole.AccountsPayable),

            ("2200", "Tax Liabilities", "الالتزامات الضريبية", AccountType.Liability, "2000", true, AccountRole.None),
            ("2201", "Output VAT (Sales)", "ضريبة القيمة المضافة على المبيعات (مخرجات)", AccountType.Liability, "2200", false, AccountRole.OutputVat),

            // Equity (3000)
            ("3000", "Equity", "حقوق الملكية", AccountType.Equity, null, true, AccountRole.None),
            ("3101", "Paid-in Capital", "رأس المال المدفوع", AccountType.Equity, "3000", false, AccountRole.None),
            ("3201", "Retained Earnings", "الأرباح المبقاة والمحتجزة", AccountType.Equity, "3000", false, AccountRole.None),

            // Revenue (4000)
            ("4000", "Revenue", "الإيرادات", AccountType.Revenue, null, true, AccountRole.None),
            ("4101", "Sales Revenue", "إيرادات مبيعات حلويات المولد", AccountType.Revenue, "4000", false, AccountRole.SalesRevenue),

            // COGS (5000)
            ("5000", "Cost of Goods Sold", "تكلفة المبيعات والإنتاج", AccountType.Expense, null, true, AccountRole.None),
            ("5101", "Cost of Goods Sold (COGS)", "تكلفة البضاعة المباعة", AccountType.Expense, "5000", false, AccountRole.CostOfGoodsSold),
            ("5201", "Production Clearing Account", "وسيط تكاليف الإنتاج", AccountType.Expense, "5000", false, AccountRole.ProductionClearing),

            // Expenses (6000)
            ("6000", "Operating Expenses", "المصروفات التشغيلية والعمومية", AccountType.Expense, null, true, AccountRole.None),
            ("6101", "Waste and Rejection Expense", "مصروف الهالك والتوالف", AccountType.Expense, "6000", false, AccountRole.WasteExpense),
            ("6201", "General and Administrative Expenses", "مصروفات إدارية وعمومية", AccountType.Expense, "6000", false, AccountRole.None)
        };

        var createdMap = new Dictionary<string, Account>();

        foreach (var item in defaultAccounts)
        {
            var acc = new Account
            {
                AccountCode = item.Code,
                AccountName = item.Name,
                AccountNameAr = item.NameAr,
                AccountType = item.Type,
                IsControlAccount = item.IsControl,
                IsActive = true,
                AccountRole = item.Role,
                ParentAccountId = item.ParentCode != null && createdMap.TryGetValue(item.ParentCode, out var parent) ? parent.Id : null
            };

            _repositoryManager.AccountRepository.Create(acc);
            await _repositoryManager.SaveAsync();
            createdMap[item.Code] = acc;

            if (item.Role != AccountRole.None)
            {
                var setting = new AccountingSetting
                {
                    Role = item.Role,
                    AccountId = acc.Id,
                    Description = $"Default mapped account for {item.Role}"
                };
                _repositoryManager.AccountingSettingRepository.Create(setting);
            }
        }

        await _repositoryManager.SaveAsync();
    }
}
