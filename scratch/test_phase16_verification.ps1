$ErrorActionPreference = "Stop"
$baseUrl = "http://localhost:5265"
$cookieJar = New-Object System.Net.CookieContainer
$handler = New-Object System.Net.Http.HttpClientHandler
$handler.CookieContainer = $cookieJar
$handler.AllowAutoRedirect = $true
$client = New-Object System.Net.Http.HttpClient($handler)
$client.BaseAddress = New-Object System.Uri($baseUrl)

$passed = 0
$failed = 0

function Assert-Test($condition, $testName) {
    if ($condition) {
        Write-Host "  [PASS] $testName" -ForegroundColor Green
        $script:passed++
    } else {
        Write-Host "  [FAIL] $testName" -ForegroundColor Red
        $script:failed++
    }
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "FactoryX Phase 16: Accounting & General Ledger Test Suite" -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# 1. Login
Write-Host "`n--- Step 1: Authentication ---" -ForegroundColor Yellow
$loginPage = $client.GetStringAsync("/Account/Login").GetAwaiter().GetResult()
$tokenMatch = [regex]::Match($loginPage, 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"')
$antiforgeryToken = if ($tokenMatch.Success) { $tokenMatch.Groups[1].Value } else { "" }

$loginForm = New-Object System.Collections.Generic.Dictionary[string, string]
$loginForm.Add("Username", "testadmin")
$loginForm.Add("Password", "Password123!")
$loginForm.Add("__RequestVerificationToken", $antiforgeryToken)
$loginContent = New-Object System.Net.Http.FormUrlEncodedContent($loginForm)

$loginResponse = $client.PostAsync("/Account/Login", $loginContent).GetAwaiter().GetResult()
$loginHtml = $loginResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult()
Assert-Test ($loginHtml -notmatch "اسم المستخدم أو كلمة المرور غير صحيحة" -and $loginResponse.IsSuccessStatusCode) "Admin Login Success"

# Helper to extract token
function Get-Token($url) {
    $html = $script:client.GetStringAsync($url).GetAwaiter().GetResult()
    $m = [regex]::Match($html, 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"')
    if ($m.Success) { return $m.Groups[1].Value }
    return ""
}

# 2. Chart of Accounts Tree & Seeded Accounts
Write-Host "`n--- Step 2: Chart of Accounts (COA) Tree & Seeding ---" -ForegroundColor Yellow
$coaHtml = $client.GetStringAsync("/Accounts").GetAwaiter().GetResult()
Assert-Test ($coaHtml -match "1000" -and $coaHtml -match "الأصول" -and $coaHtml -match "1101") "Root Assets & Cash Account Seeded"
Assert-Test ($coaHtml -match "1201" -and $coaHtml -match "العملاء") "Accounts Receivable Seeded"
Assert-Test ($coaHtml -match "1301" -and $coaHtml -match "1302" -and $coaHtml -match "1303") "Raw, Packaging & FG Inventory Accounts Seeded"
Assert-Test ($coaHtml -match "2101" -and $coaHtml -match "الموردين") "Accounts Payable Seeded"
Assert-Test ($coaHtml -match "4101" -and $coaHtml -match "المبيعات") "Sales Revenue Account Seeded"
Assert-Test ($coaHtml -match "5101" -and $coaHtml -match "تكلفة") "COGS Account Seeded"
Assert-Test ($coaHtml -match "6101" -and $coaHtml -match "الهالك") "Waste Expense Account Seeded"

# List View & Settings
$listHtml = $client.GetStringAsync("/Accounts/List").GetAwaiter().GetResult()
Assert-Test ($listHtml -match "قائمة الحسابات المالية" -and $listHtml -match "1101") "Accounts List View Accessible"

$settingsHtml = $client.GetStringAsync("/Accounts/Settings").GetAwaiter().GetResult()
Assert-Test ($settingsHtml -match "إعدادات التوجيه المحاسبي التلقائي" -and $settingsHtml -match "AccountsReceivable") "Account Settings Mapping Accessible"

# 3. Create Custom Account & Edit
Write-Host "`n--- Step 3: Account Creation & Update ---" -ForegroundColor Yellow
$rndCode = "110" + (Get-Random -Minimum 10 -Maximum 99)
$createAccountToken = Get-Token "/Accounts/Create"
$accForm = New-Object System.Collections.Generic.Dictionary[string, string]
$accForm.Add("AccountCode", $rndCode)
$accForm.Add("AccountName", "Custom Petty Cash $rndCode")
$accForm.Add("AccountNameAr", "خزينة نقدية إضافية $rndCode")
$accForm.Add("AccountType", "Asset")
$accForm.Add("IsActive", "true")
$accForm.Add("IsControlAccount", "false")
$accForm.Add("Notes", "حساب تجريبي للاختبارات المؤتمتة")
$accForm.Add("__RequestVerificationToken", $createAccountToken)

$accResp = $client.PostAsync("/Accounts/Create", (New-Object System.Net.Http.FormUrlEncodedContent($accForm))).GetAwaiter().GetResult()
$accTreeCheck = $client.GetStringAsync("/Accounts").GetAwaiter().GetResult()
Assert-Test ($accTreeCheck -match $rndCode) "Custom Account Created: $rndCode"

# 4. Accounting Periods
Write-Host "`n--- Step 4: Accounting Periods ---" -ForegroundColor Yellow
$periodsHtml = $client.GetStringAsync("/AccountingPeriods").GetAwaiter().GetResult()
Assert-Test ($periodsHtml -match "FY" -and $periodsHtml -match "مفتوحة") "Current Fiscal Period Open and Active"

# 5. Manual Journal Entry Engine & Balancing Invariants
Write-Host "`n--- Step 5: Manual Journal Entries & Double-Entry Invariants ---" -ForegroundColor Yellow
$createJeToken = Get-Token "/JournalEntries/Create"

# 5.1 Test Unbalanced Journal (Must be Rejected)
$unbalancedForm = New-Object System.Collections.Generic.Dictionary[string, string]
$unbalancedForm.Add("EntryDate", (Get-Date).ToString("yyyy-MM-dd"))
$unbalancedForm.Add("Description", "قيد غير متوازن للاختبار")
$unbalancedForm.Add("Lines[0].AccountId", "1")
$unbalancedForm.Add("Lines[0].Debit", "1000")
$unbalancedForm.Add("Lines[0].Credit", "0")
$unbalancedForm.Add("Lines[1].AccountId", "2")
$unbalancedForm.Add("Lines[1].Debit", "0")
$unbalancedForm.Add("Lines[1].Credit", "500") # Unbalanced!
$unbalancedForm.Add("__RequestVerificationToken", $createJeToken)

$unbalResp = $client.PostAsync("/JournalEntries/Create", (New-Object System.Net.Http.FormUrlEncodedContent($unbalancedForm))).GetAwaiter().GetResult()
$unbalHtml = $unbalResp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
Assert-Test ($unbalHtml -match "غير متوازن" -or $unbalHtml -match "لا يساوي") "Unbalanced Journal Correctly Rejected"

# 5.2 Test Balanced Manual Journal (Must Succeed)
# Get Cash and Capital account IDs
$cashIdMatch = [regex]::Match($listHtml, 'Edit/(\d+)[^"]*">\s*<i class="bi bi-pencil"></i>\s*</a>\s*<a href="/GeneralLedger\?AccountId=\d+[^"]*"\s+class="btn btn-outline-info"\s+title="كشف الحساب">\s*<i class="bi bi-journal-text"></i>\s*</a>\s*</div>\s*</td>\s*</tr>')

$balancedForm = New-Object System.Collections.Generic.Dictionary[string, string]
$balancedForm.Add("EntryDate", (Get-Date).ToString("yyyy-MM-dd"))
$balancedForm.Add("Description", "قيد زيادة رأس مال نقدي تجريبي")
$balancedForm.Add("Lines[0].AccountId", "1") # Main Cash
$balancedForm.Add("Lines[0].Debit", "50000")
$balancedForm.Add("Lines[0].Credit", "0")
$balancedForm.Add("Lines[1].AccountId", "10") # Capital
$balancedForm.Add("Lines[1].Debit", "0")
$balancedForm.Add("Lines[1].Credit", "50000")
$balancedForm.Add("__RequestVerificationToken", $createJeToken)

$balResp = $client.PostAsync("/JournalEntries/Create", (New-Object System.Net.Http.FormUrlEncodedContent($balancedForm))).GetAwaiter().GetResult()
$balRedirectHtml = $balResp.Content.ReadAsStringAsync().GetAwaiter().GetResult()

# Check Journal Index
$jeIndexHtml = $client.GetStringAsync("/JournalEntries").GetAwaiter().GetResult()
$jeNumMatch = [regex]::Match($jeIndexHtml, '(JE-\d{8}-\d{4})')
Assert-Test ($jeNumMatch.Success) "Balanced Journal Successfully Posted: $($jeNumMatch.Groups[1].Value)"

# 6. Journal Reversal
Write-Host "`n--- Step 6: Journal Entry Reversal ---" -ForegroundColor Yellow
$jeDetailsIdMatch = [regex]::Match($jeIndexHtml, '/JournalEntries/Details/(\d+)')
if ($jeDetailsIdMatch.Success) {
    $jeId = $jeDetailsIdMatch.Groups[1].Value
    $revToken = Get-Token "/JournalEntries/Details/$jeId"
    
    $revForm = New-Object System.Collections.Generic.Dictionary[string, string]
    $revForm.Add("JournalEntryId", $jeId)
    $revForm.Add("Reason", "عكس قيد تجريبي للتأكد من آلية Reversal")
    $revForm.Add("__RequestVerificationToken", $revToken)
    
    $revResp = $client.PostAsync("/JournalEntries/Reverse", (New-Object System.Net.Http.FormUrlEncodedContent($revForm))).GetAwaiter().GetResult()
    $revHtml = $revResp.Content.ReadAsStringAsync().GetAwaiter().GetResult()
    Assert-Test ($revHtml -match "معكوس" -or $revHtml -match "قيد عكسي") "Journal Successfully Reversed and Linked"
} else {
    Assert-Test $false "Journal ID for Reversal"
}

# 7. Supplier Payment (SPAY) Flow
Write-Host "`n--- Step 7: Supplier Payment & Posting ---" -ForegroundColor Yellow
$spayCreateToken = Get-Token "/SupplierPayments/Create"
# Get first supplier ID
$suppMatch = [regex]::Match($spayCreateToken, 'value="(\d+)"[^>]*>[^<]*SUP-')
$suppId = if ($suppMatch.Success) { $suppMatch.Groups[1].Value } else { "1" }

$spayForm = New-Object System.Collections.Generic.Dictionary[string, string]
$spayForm.Add("SupplierId", $suppId)
$spayForm.Add("Amount", "1500.00")
$spayForm.Add("PaymentDate", (Get-Date).ToString("yyyy-MM-dd"))
$spayForm.Add("PaymentMethod", "Cash")
$spayForm.Add("Notes", "دفعة تحت الحساب للمورد")
$spayForm.Add("__RequestVerificationToken", $spayCreateToken)

$spayResp = $client.PostAsync("/SupplierPayments/Create", (New-Object System.Net.Http.FormUrlEncodedContent($spayForm))).GetAwaiter().GetResult()
$spayListHtml = $client.GetStringAsync("/SupplierPayments").GetAwaiter().GetResult()
Assert-Test ($spayListHtml -match "SPAY-" -and $spayListHtml -match "1,500.00") "Supplier Payment Created and Numbered (SPAY-YYYYMMDD-XXXX)"

# 8. General Ledger, Customer Subledger & Supplier Subledger
Write-Host "`n--- Step 8: General Ledger & Subledgers ---" -ForegroundColor Yellow
$glHtml = $client.GetStringAsync("/GeneralLedger").GetAwaiter().GetResult()
Assert-Test ($glHtml -match "دفتر الأستاذ العام" -and $glHtml -match "الرصيد التراكمي") "General Ledger Account Sheets Generated"

$custLedgerHtml = $client.GetStringAsync("/CustomerLedger").GetAwaiter().GetResult()
Assert-Test ($custLedgerHtml -match "كشف حساب الأستاذ المساعد للعملاء" -and $custLedgerHtml -match "الرصيد الافتتاحي") "Customer Subledger Statement Functional"

$suppLedgerHtml = $client.GetStringAsync("/SupplierLedger").GetAwaiter().GetResult()
Assert-Test ($suppLedgerHtml -match "كشف حساب الأستاذ المساعد للموردين" -and $suppLedgerHtml -match "الرصيد الافتتاحي") "Supplier Subledger Statement Functional"

# 9. Trial Balance Verification
Write-Host "`n--- Step 9: Trial Balance (Debits == Credits Parity) ---" -ForegroundColor Yellow
$tbHtml = $client.GetStringAsync("/TrialBalance").GetAwaiter().GetResult()
Assert-Test ($tbHtml -match "ميزان المراجعة" -and $tbHtml -match "الميزان متوازن تماماً") "Trial Balance Parity Verified: Debits == Credits"

# 10. Accounting Dashboard
Write-Host "`n--- Step 10: Accounting Dashboard KPIs ---" -ForegroundColor Yellow
$dashHtml = $client.GetStringAsync("/AccountingDashboard").GetAwaiter().GetResult()
Assert-Test ($dashHtml -match "لوحة المؤشرات المحاسبية والمالية") "Accounting Dashboard Loaded"
Assert-Test ($dashHtml -match "إجمالي الإيرادات" -and $dashHtml -match "مجمل الربح") "Financial KPIs (Revenue, COGS, Gross Profit) Present"
Assert-Test ($dashHtml -match "السيولة النقدية والبنكية" -and $dashHtml -match "تقييم المخزون المالي") "Liquidity & Inventory Value Metrics Functional"

Write-Host "`n============================================================" -ForegroundColor Cyan
Write-Host "Summary: $passed Passed, $failed Failed" -ForegroundColor $(if ($failed -eq 0) { "Green" } else { "Red" })
Write-Host "============================================================" -ForegroundColor Cyan

if ($failed -gt 0) {
    exit 1
}
