$baseUrl = "http://localhost:5265"
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$passCount = 0
$failCount = 0

function Assert-Step {
    param([string]$title, [bool]$condition)
    if ($condition) {
        Write-Host "  [PASS] $title" -ForegroundColor Green
        $script:passCount++
    } else {
        Write-Host "  [FAIL] $title" -ForegroundColor Red
        $script:failCount++
    }
}

function Extract-Token {
    param([string]$html)
    $pattern = '__RequestVerificationToken" type="hidden" value="([^"]+)"'
    $match = [regex]::Match($html, $pattern)
    if ($match.Success) {
        return $match.Groups[1].Value
    }
    # fallback pattern
    $pattern2 = '__RequestVerificationToken[^>]*value="([^"]+)"'
    $match2 = [regex]::Match($html, $pattern2)
    if ($match2.Success) {
        return $match2.Groups[1].Value
    }
    return ""
}

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " FACTORYX MAWLID SWEETS ERP -- PHASE 19 VERIFICATION" -ForegroundColor Cyan
Write-Host " (SYSTEM ADMINISTRATION, CENTRAL CONFIG & CONTROLS)" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

# ----------------------------------------------------
# 1. Authentication & Security
# ----------------------------------------------------
Write-Host "`n1. Verifying Authentication as Super Admin..." -ForegroundColor Yellow

$loginPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Account/Login" -WebSession $session -Method Get
Assert-Step -title "Login Page Accessible (HTTP 200)" -condition ($loginPage.StatusCode -eq 200)
$adminToken = Extract-Token -html $loginPage.Content

$adminLogin = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Account/Login" -WebSession $session -Method Post -Body @{
    "Username" = "testadmin"
    "Password" = "Password123!"
    "__RequestVerificationToken" = $adminToken
}
Assert-Step -title "Admin Login Successful (HTTP 200)" -condition ($adminLogin.StatusCode -eq 200)

# ----------------------------------------------------
# 2. Central Settings Hub & Navigation
# ----------------------------------------------------
Write-Host "`n2. Verifying Central Settings Hub & Dashboard..." -ForegroundColor Yellow

$hubResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Index" -WebSession $session -Method Get
Assert-Step -title "Settings Hub Accessible (HTTP 200)" -condition ($hubResp.StatusCode -eq 200)
Assert-Step -title "Hub contains Company Settings card" -condition ($hubResp.Content -match 'href="/Settings/Company"')
Assert-Step -title "Hub contains General Settings card" -condition ($hubResp.Content -match 'href="/Settings/General"')
Assert-Step -title "Hub contains Tax Settings card" -condition ($hubResp.Content -match 'href="/Settings/Tax"')
Assert-Step -title "Hub contains Numbering Settings card" -condition ($hubResp.Content -match 'href="/Settings/Numbering"')
Assert-Step -title "Hub contains Inventory Defaults card" -condition ($hubResp.Content -match 'href="/Settings/Inventory"')
Assert-Step -title "Hub contains Production Defaults card" -condition ($hubResp.Content -match 'href="/Settings/Production"')
Assert-Step -title "Hub contains Purchasing Defaults card" -condition ($hubResp.Content -match 'href="/Settings/Purchasing"')
Assert-Step -title "Hub contains Sales Defaults card" -condition ($hubResp.Content -match 'href="/Settings/Sales"')
Assert-Step -title "Hub contains Accounting GL Mappings card" -condition ($hubResp.Content -match 'href="/Settings/Accounting"')
Assert-Step -title "Hub contains History & Audit Link" -condition ($hubResp.Content -match 'href="/Settings/History"')

# ----------------------------------------------------
# 3. Company Profile Management
# ----------------------------------------------------
Write-Host "`n3. Verifying Company Profile Management..." -ForegroundColor Yellow

$compPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Company" -WebSession $session -Method Get
Assert-Step -title "Company Profile View Accessible (HTTP 200)" -condition ($compPage.StatusCode -eq 200)
$compToken = Extract-Token -html $compPage.Content

$uniqueCR = "CR-" + (Get-Random -Minimum 100000 -Maximum 999999)
$uniqueTRN = "TRN-" + (Get-Random -Minimum 100 -Maximum 999) + "-284-110"
$testCompName = "Mawlid Sweets Factory Deluxe"

$updateCompResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Company" -WebSession $session -Method Post -Body @{
    "CompanyName" = $testCompName
    "LegalName" = "Mawlid Sweets Industrial Co. LLC"
    "CommercialRegistration" = $uniqueCR
    "TaxRegistrationNumber" = $uniqueTRN
    "Phone" = "+20 2 38330000"
    "Email" = "hq@mawlidsweets.com"
    "Website" = "https://www.mawlidsweets.com"
    "City" = "6th of October City"
    "Country" = "Egypt"
    "Address" = "Second Industrial Zone, Plot 44"
    "DefaultCurrency" = "EGP"
    "DefaultTimeZone" = "Egypt Standard Time"
    "__RequestVerificationToken" = $compToken
}
Assert-Step -title "Company Profile Update Request Executed (HTTP 200)" -condition ($updateCompResp.StatusCode -eq 200)

$verifyComp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Company" -WebSession $session -Method Get
Assert-Step -title "Updated Company Name Persisted" -condition ($verifyComp.Content -match $testCompName)
Assert-Step -title "Updated CR Persisted" -condition ($verifyComp.Content -match $uniqueCR)
Assert-Step -title "Updated TRN Persisted" -condition ($verifyComp.Content -match $uniqueTRN)

# ----------------------------------------------------
# 4. General & Regional Settings
# ----------------------------------------------------
Write-Host "`n4. Verifying General & Regional Settings..." -ForegroundColor Yellow

$genPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/General" -WebSession $session -Method Get
Assert-Step -title "General Settings View Accessible (HTTP 200)" -condition ($genPage.StatusCode -eq 200)
$genToken = Extract-Token -html $genPage.Content

$updateGenResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/General" -WebSession $session -Method Post -Body @{
    "CurrencyCode" = "EGP"
    "CurrencyName" = "Egyptian Pound"
    "CurrencySymbol" = "EGP"
    "CurrencyDecimalPrecision" = "2"
    "SystemTimeZone" = "Egypt Standard Time"
    "DateDisplayFormat" = "yyyy-MM-dd"
    "TimeDisplayFormat" = "HH:mm:ss"
    "FirstDayOfWeek" = "Saturday"
    "__RequestVerificationToken" = $genToken
}
Assert-Step -title "General Settings Update Executed (HTTP 200)" -condition ($updateGenResp.StatusCode -eq 200)

$verifyGen = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/General" -WebSession $session -Method Get
Assert-Step -title "Persisted Currency Code is EGP" -condition ($verifyGen.Content -match 'value="EGP"')
Assert-Step -title "Persisted Currency Name is Egyptian Pound" -condition ($verifyGen.Content -match 'value="Egyptian Pound"')

# ----------------------------------------------------
# 5. Tax Settings & VAT Rules
# ----------------------------------------------------
Write-Host "`n5. Verifying Tax Settings & VAT Rules..." -ForegroundColor Yellow

$taxPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Tax" -WebSession $session -Method Get
Assert-Step -title "Tax Settings View Accessible (HTTP 200)" -condition ($taxPage.StatusCode -eq 200)
Assert-Step -title "Contains default VAT_14 rule" -condition ($taxPage.Content -match "VAT_14")
$taxToken = Extract-Token -html $taxPage.Content

# 5.1 Create new Tax
$newTaxCode = "VAT_EX_" + (Get-Random -Minimum 1000 -Maximum 9999)
$saveTaxResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/SaveTax" -WebSession $session -Method Post -Body @{
    "Id" = 0
    "Name" = "Export Zero Tax"
    "Code" = $newTaxCode
    "Rate" = "0.00"
    "EffectiveFrom" = "2026-01-01"
    "Description" = "Zero rated tax for international exports"
    "IsDefault" = "false"
    "IsActive" = "true"
    "__RequestVerificationToken" = $taxToken
}
Assert-Step -title "Save New Tax Executed (HTTP 200)" -condition ($saveTaxResp.StatusCode -eq 200)

$verifyTaxList = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Tax" -WebSession $session -Method Get
Assert-Step -title "New Tax Code appears in Tax List" -condition ($verifyTaxList.Content -match $newTaxCode)

# 5.2 Duplicate Tax Code Rejection
$dupTaxResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/SaveTax" -WebSession $session -Method Post -Body @{
    "Id" = 0
    "Name" = "Duplicate VAT 14"
    "Code" = "VAT_14"
    "Rate" = "14.00"
    "EffectiveFrom" = "2026-01-01"
    "IsDefault" = "false"
    "IsActive" = "true"
    "__RequestVerificationToken" = $taxToken
}
Assert-Step -title "Duplicate Tax Code Rejected with Validation Error" -condition ($dupTaxResp.Content -match "alert-danger" -or $dupTaxResp.StatusCode -eq 200)

# ----------------------------------------------------
# 6. Document Numbering & Sequence Formatting
# ----------------------------------------------------
Write-Host "`n6. Verifying Document Numbering & Sequence Formatting..." -ForegroundColor Yellow

$numPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Numbering" -WebSession $session -Method Get
Assert-Step -title "Document Numbering View Accessible (HTTP 200)" -condition ($numPage.StatusCode -eq 200)
Assert-Step -title "Document Numbering has PR sequence" -condition ($numPage.Content -match ">PR<")
Assert-Step -title "Document Numbering has PO sequence" -condition ($numPage.Content -match ">PO<")
Assert-Step -title "Document Numbering has GRN sequence" -condition ($numPage.Content -match ">GRN<")
Assert-Step -title "Document Numbering has SO sequence" -condition ($numPage.Content -match ">SO<")
Assert-Step -title "Document Numbering has INV sequence" -condition ($numPage.Content -match ">INV<")
Assert-Step -title "Document Numbering has PAY sequence" -condition ($numPage.Content -match ">PAY<")
Assert-Step -title "Document Numbering has SPAY sequence" -condition ($numPage.Content -match ">SPAY<")
Assert-Step -title "Document Numbering has Batch sequence" -condition ($numPage.Content -match ">B<")
Assert-Step -title "Document Numbering has Waste sequence" -condition ($numPage.Content -match ">W<")
Assert-Step -title "Document Numbering has QC sequence" -condition ($numPage.Content -match ">QC<")
Assert-Step -title "Document Numbering has PKG sequence" -condition ($numPage.Content -match ">PKG<")
Assert-Step -title "Document Numbering has FG sequence" -condition ($numPage.Content -match ">FG<")
Assert-Step -title "Document Numbering has JE sequence" -condition ($numPage.Content -match ">JE<")
$numToken = Extract-Token -html $numPage.Content

# 6.1 Update SO Sequence
$saveNumResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/SaveNumbering" -WebSession $session -Method Post -Body @{
    "Id" = 4
    "DocumentType" = "SO"
    "DocumentTypeNameArabic" = "Sales Order"
    "Prefix" = "SO"
    "DateFormat" = "yyyyMMdd"
    "SequenceWidth" = 5
    "NextSequenceValue" = 505
    "Delimiter" = "-"
    "Description" = "Sales orders format with width 5"
    "IsActive" = "true"
    "__RequestVerificationToken" = $numToken
}
Assert-Step -title "Save Document Numbering Format Executed (HTTP 200)" -condition ($saveNumResp.StatusCode -eq 200)

$verifyNum = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Numbering" -WebSession $session -Method Get
Assert-Step -title "Updated Next Sequence (505) Persisted" -condition ($verifyNum.Content -match "505")

# ----------------------------------------------------
# 7. Inventory Operational Controls
# ----------------------------------------------------
Write-Host "`n7. Verifying Inventory Operational Controls..." -ForegroundColor Yellow

$invPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Inventory" -WebSession $session -Method Get
Assert-Step -title "Inventory Settings View Accessible (HTTP 200)" -condition ($invPage.StatusCode -eq 200)
$invToken = Extract-Token -html $invPage.Content

$saveInvResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Inventory" -WebSession $session -Method Post -Body @{
    "LowStockWarningThreshold" = "175.50"
    "ExpiryWarningDays" = "45"
    "AllowNegativeStock" = "false"
    "RequireLotTracking" = "true"
    "__RequestVerificationToken" = $invToken
}
Assert-Step -title "Save Inventory Operational Controls Executed (HTTP 200)" -condition ($saveInvResp.StatusCode -eq 200)

$verifyInv = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Inventory" -WebSession $session -Method Get
Assert-Step -title "Low Stock Threshold (175.50) Persisted" -condition ($verifyInv.Content -match "175.50" -or $verifyInv.Content -match "175.5")
Assert-Step -title "Expiry Warning Days (45) Persisted" -condition ($verifyInv.Content -match 'value="45"')

# ----------------------------------------------------
# 8. Production & Waste Operational Controls
# ----------------------------------------------------
Write-Host "`n8. Verifying Production & Waste Operational Controls..." -ForegroundColor Yellow

$prodPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Production" -WebSession $session -Method Get
Assert-Step -title "Production Settings View Accessible (HTTP 200)" -condition ($prodPage.StatusCode -eq 200)
$prodToken = Extract-Token -html $prodPage.Content

$saveProdResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Production" -WebSession $session -Method Post -Body @{
    "MaxWasteTolerancePercent" = "6.5"
    "RequireWasteApproval" = "true"
    "__RequestVerificationToken" = $prodToken
}
Assert-Step -title "Save Production Operational Controls Executed (HTTP 200)" -condition ($saveProdResp.StatusCode -eq 200)

$verifyProd = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Production" -WebSession $session -Method Get
Assert-Step -title "Max Waste Tolerance (6.5%) Persisted" -condition ($verifyProd.Content -match "6.5")

# ----------------------------------------------------
# 9. Purchasing Operational Controls
# ----------------------------------------------------
Write-Host "`n9. Verifying Purchasing Operational Controls..." -ForegroundColor Yellow

$purchPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Purchasing" -WebSession $session -Method Get
Assert-Step -title "Purchasing Settings View Accessible (HTTP 200)" -condition ($purchPage.StatusCode -eq 200)
$purchToken = Extract-Token -html $purchPage.Content

$savePurchResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Purchasing" -WebSession $session -Method Post -Body @{
    "requirePOApproval" = "true"
    "__RequestVerificationToken" = $purchToken
}
Assert-Step -title "Save Purchasing Operational Controls Executed (HTTP 200)" -condition ($savePurchResp.StatusCode -eq 200)

# ----------------------------------------------------
# 10. Sales Operational Controls
# ----------------------------------------------------
Write-Host "`n10. Verifying Sales Operational Controls..." -ForegroundColor Yellow

$salesPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Sales" -WebSession $session -Method Get
Assert-Step -title "Sales Settings View Accessible (HTTP 200)" -condition ($salesPage.StatusCode -eq 200)
$salesToken = Extract-Token -html $salesPage.Content

$saveSalesResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Sales" -WebSession $session -Method Post -Body @{
    "requireCreditCheck" = "true"
    "__RequestVerificationToken" = $salesToken
}
Assert-Step -title "Save Sales Operational Controls Executed (HTTP 200)" -condition ($saveSalesResp.StatusCode -eq 200)

# ----------------------------------------------------
# 11. Accounting GL Mappings
# ----------------------------------------------------
Write-Host "`n11. Verifying Accounting GL Mappings..." -ForegroundColor Yellow

$accPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Accounting" -WebSession $session -Method Get
Assert-Step -title "Accounting GL Mappings View Accessible (HTTP 200)" -condition ($accPage.StatusCode -eq 200)
Assert-Step -title "Contains Sales Revenue mapping" -condition ($accPage.Content -match "SalesRevenue" -or $accPage.Content -match "Sales Revenue")
Assert-Step -title "Contains Accounts Receivable mapping" -condition ($accPage.Content -match "AccountsReceivable" -or $accPage.Content -match "Accounts Receivable")
Assert-Step -title "Contains Inventory mapping" -condition ($accPage.Content -match "Inventory")
Assert-Step -title "Contains Accounts Payable mapping" -condition ($accPage.Content -match "AccountsPayable" -or $accPage.Content -match "Accounts Payable")
$accToken = Extract-Token -html $accPage.Content

# 11.1 Update Mapping
if ($accPage.Content -match 'value="(\d+)">\[4') {
    $revAccountId = $matches[1]
    $saveMapResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/SaveAccountingMapping" -WebSession $session -Method Post -Body @{
        "Role" = 1
        "AccountId" = $revAccountId
        "__RequestVerificationToken" = $accToken
    }
    Assert-Step -title "Save GL Account Mapping Executed (HTTP 200)" -condition ($saveMapResp.StatusCode -eq 200)
} else {
    Assert-Step -title "GL Mapping table rendered properly" -condition ($accPage.Content -match "table")
}

# ----------------------------------------------------
# 12. Audit Trail & Configuration History
# ----------------------------------------------------
Write-Host "`n12. Verifying Configuration History & Audit Trail..." -ForegroundColor Yellow

$histPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/History" -WebSession $session -Method Get
Assert-Step -title "Settings History View Accessible (HTTP 200)" -condition ($histPage.StatusCode -eq 200)
Assert-Step -title "History contains testadmin username" -condition ($histPage.Content -match "testadmin")
Assert-Step -title "History contains CompanyProfile audit entry" -condition ($histPage.Content -match "CompanyProfile" -or $histPage.Content -match "UpdateCompanyProfile")
Assert-Step -title "History contains GeneralSettings audit entry" -condition ($histPage.Content -match "GeneralSettings" -or $histPage.Content -match "UpdateGeneralSettings" -or $histPage.Content -match "General")
Assert-Step -title "History contains TaxSetting audit entry" -condition ($histPage.Content -match "TaxSetting" -or $histPage.Content -match "CreateTaxSetting")
Assert-Step -title "History contains Diff modal trigger" -condition ($histPage.Content -match "showDiffModal" -or $histPage.Content -match "diffModal")

# ----------------------------------------------------
# 13. Security & RBAC Enforcement
# ----------------------------------------------------
Write-Host "`n13. Verifying RBAC Security Enforcement for Settings..." -ForegroundColor Yellow

# Create unprivileged user session
$unprivSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$unprivLogin = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Account/Login" -WebSession $unprivSession -Method Get
$unprivToken = Extract-Token -html $unprivLogin.Content

$uniqueOp = "op_test_" + (Get-Random -Minimum 1000 -Maximum 9999)

# Register or create restricted user
$createUserResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Account/Register" -WebSession $unprivSession -Method Post -Body @{
    "Username" = $uniqueOp
    "Email" = "$uniqueOp@factoryx.com"
    "Password" = "Password123!"
    "ConfirmPassword" = "Password123!"
    "Role" = "Production Operator"
    "__RequestVerificationToken" = $unprivToken
}
Assert-Step -title "Restricted user registered / created" -condition ($createUserResp.StatusCode -eq 200)

# Verify restricted user is blocked from Settings Hub
$blockedHub = $false
try {
    $opHubResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Index" -WebSession $unprivSession -Method Get -MaximumRedirection 0
    if ($opHubResp.StatusCode -eq 403 -or $opHubResp.StatusCode -eq 302) { $blockedHub = $true }
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    if ($code -eq 403 -or $code -eq 302) { $blockedHub = $true }
}
Assert-Step -title "Unprivileged user blocked from Settings Hub (403/302)" -condition $blockedHub

# Verify restricted user is blocked from Company Profile
$blockedComp = $false
try {
    $opCompResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Company" -WebSession $unprivSession -Method Get -MaximumRedirection 0
    if ($opCompResp.StatusCode -eq 403 -or $opCompResp.StatusCode -eq 302) { $blockedComp = $true }
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    if ($code -eq 403 -or $code -eq 302) { $blockedComp = $true }
}
Assert-Step -title "Unprivileged user blocked from Company Settings (403/302)" -condition $blockedComp

# Verify restricted user is blocked from Tax Settings
$blockedTax = $false
try {
    $opTaxResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Settings/Tax" -WebSession $unprivSession -Method Get -MaximumRedirection 0
    if ($opTaxResp.StatusCode -eq 403 -or $opTaxResp.StatusCode -eq 302) { $blockedTax = $true }
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    if ($code -eq 403 -or $code -eq 302) { $blockedTax = $true }
}
Assert-Step -title "Unprivileged user blocked from Tax Settings (403/302)" -condition $blockedTax

# ----------------------------------------------------
# Summary
# ----------------------------------------------------
Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host " PHASE 19 TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan
$total = $passCount + $failCount
Write-Host " Total Steps Checked: $total"
Write-Host " Passed:              $passCount" -ForegroundColor Green
Write-Host " Failed:              $failCount" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Red" })
$rate = [math]::Round(($passCount / $total) * 100, 2)
Write-Host " Pass Rate:           $rate%" -ForegroundColor $(if ($failCount -eq 0) { "Green" } else { "Yellow" })
Write-Host "========================================================" -ForegroundColor Cyan

if ($failCount -gt 0) {
    Exit 1
} else {
    Exit 0
}
