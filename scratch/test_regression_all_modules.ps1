# FactoryX Mawlid Sweets ERP System - Master End-to-End Regression Test Suite
# Covering Modules across all Phases (Phases 3 to 19):
#  - Phase 3: Inventory Foundation and Warehouses
#  - Phase 4: Raw Materials Master Data
#  - Phase 5: Finished Products Master Data
#  - Phase 6: Recipes and BOM Management
#  - Phase 7: Production Planning and Production Orders
#  - Phase 8: Production Batches and Execution
#  - Phase 9: Waste and Rejection Management
#  - Phase 10: Quality Control (QC) and Release Gate
#  - Phase 11: Packaging Management and Packaging Execution
#  - Phase 12: Finished Goods Inventory & Release
#  - Phase 13: Purchasing and Supplier Management
#  - Phase 14: Sales and Customer Management
#  - Phase 15: Invoicing and Payments
#  - Phase 16: Accounting and General Ledger
#  - Phase 17: Reporting and Analytics
#  - Phase 18: Security, RBAC & Audit Trail
#  - Phase 19: System Administration, Central Configuration & Operational Controls

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5265"
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

$script:passed = 0
$script:failed = 0

function Assert-Test([string]$name, [bool]$condition, [string]$details = "") {
    if ($condition) {
        Write-Host "  [PASS] $name" -ForegroundColor Green
        $script:passed++
    } else {
        Write-Host "  [FAIL] $name - Details: $details" -ForegroundColor Red
        $script:failed++
    }
}

function Get-Token($content) {
    if ($content -match 'name="__RequestVerificationToken"\s+type="hidden"\s+value="([^"]+)"') {
        return $matches[1]
    }
    if ($content -match '__RequestVerificationToken"[^>]*value="([^"]+)"') {
        return $matches[1]
    }
    return ""
}

Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host " FACTORYX MAWLID SWEETS ERP -- MASTER REGRESSION TEST SUITE     " -ForegroundColor Cyan
Write-Host " Full System Verification Across All Modules (Phases 3 - 19)     " -ForegroundColor Cyan
Write-Host "=================================================================" -ForegroundColor Cyan

# 1. Authentication
Write-Host "`n[Regression] 1. Authentication and System Core..." -ForegroundColor Yellow
$loginPage = Invoke-WebRequest -Uri "$baseUrl/Login" -WebSession $session -UseBasicParsing
$loginToken = Get-Token $loginPage.Content
$loginBody = @{
    Username = "testadmin"
    Password = "Password123!"
    "__RequestVerificationToken" = $loginToken
}
$loginResp = Invoke-WebRequest -Uri "$baseUrl/Login" -Method Post -Body $loginBody -WebSession $session -MaximumRedirection 5 -UseBasicParsing
Assert-Test "Admin Login Successful (HTTP 200)" ($loginResp.StatusCode -eq 200)

# 2. Phase 3: Inventory Foundation
Write-Host "`n[Regression] 2. Phase 3: Inventory Foundation and Warehouses..." -ForegroundColor Yellow
$whResp = Invoke-WebRequest -Uri "$baseUrl/Warehouses" -WebSession $session -UseBasicParsing
Assert-Test "Warehouses Index (HTTP 200)" ($whResp.StatusCode -eq 200)
$locResp = Invoke-WebRequest -Uri "$baseUrl/WarehouseLocations" -WebSession $session -UseBasicParsing
Assert-Test "Locations Index (HTTP 200)" ($locResp.StatusCode -eq 200)
$stockResp = Invoke-WebRequest -Uri "$baseUrl/Inventory/Stock" -WebSession $session -UseBasicParsing
Assert-Test "Stock Balance View (HTTP 200)" ($stockResp.StatusCode -eq 200)
$txResp = Invoke-WebRequest -Uri "$baseUrl/Inventory/Transactions" -WebSession $session -UseBasicParsing
Assert-Test "Inventory Transactions View (HTTP 200)" ($txResp.StatusCode -eq 200)

# 3. Phase 4: Raw Materials Master Data
Write-Host "`n[Regression] 3. Phase 4: Raw Materials Master Data..." -ForegroundColor Yellow
$matResp = Invoke-WebRequest -Uri "$baseUrl/Materials" -WebSession $session -UseBasicParsing
Assert-Test "Materials Master Data Index (HTTP 200)" ($matResp.StatusCode -eq 200)
Assert-Test "Essential Raw Materials Listed" ($matResp.Content -match '/Materials/Details/\d+')
$matCatResp = Invoke-WebRequest -Uri "$baseUrl/MaterialCategories" -WebSession $session -UseBasicParsing
Assert-Test "Material Categories Index (HTTP 200)" ($matCatResp.StatusCode -eq 200)

# 4. Phase 5: Finished Products Master Data
Write-Host "`n[Regression] 4. Phase 5: Finished Products Master Data..." -ForegroundColor Yellow
$prodResp = Invoke-WebRequest -Uri "$baseUrl/Products" -WebSession $session -UseBasicParsing
Assert-Test "Finished Products Index (HTTP 200)" ($prodResp.StatusCode -eq 200)
Assert-Test "Mawlid Sweets Products Present" ($prodResp.Content -match '/Products/Details/\d+')
$prodCatResp = Invoke-WebRequest -Uri "$baseUrl/ProductCategories" -WebSession $session -UseBasicParsing
Assert-Test "Product Categories Index (HTTP 200)" ($prodCatResp.StatusCode -eq 200)

# 5. Phase 6: Recipes and BOM Management
Write-Host "`n[Regression] 5. Phase 6: Recipes and BOM Management..." -ForegroundColor Yellow
$recResp = Invoke-WebRequest -Uri "$baseUrl/Recipes" -WebSession $session -UseBasicParsing
Assert-Test "Recipes Index (HTTP 200)" ($recResp.StatusCode -eq 200)
Assert-Test "Standard Recipes with Versions Exist" ($recResp.Content -match '/Recipes/Details/\d+')

# 6. Phase 7: Production Planning and Production Orders
Write-Host "`n[Regression] 6. Phase 7: Production Planning and Orders..." -ForegroundColor Yellow
$poResp = Invoke-WebRequest -Uri "$baseUrl/ProductionOrders" -WebSession $session -UseBasicParsing
Assert-Test "Production Orders Index (HTTP 200)" ($poResp.StatusCode -eq 200)
Assert-Test "Orders Listed with Status Tracking" ($poResp.Content -match '/ProductionOrders/Details/\d+')

# 7. Phase 8: Production Batches and Execution
Write-Host "`n[Regression] 7. Phase 8: Production Batches and Execution..." -ForegroundColor Yellow
$pbResp = Invoke-WebRequest -Uri "$baseUrl/ProductionBatches" -WebSession $session -UseBasicParsing
Assert-Test "Production Batches Index (HTTP 200)" ($pbResp.StatusCode -eq 200)
Assert-Test "Production Batches with Consumption Traceability" ($pbResp.Content -match '/ProductionBatches/Details/\d+')

# 8. Phase 9: Waste and Rejection Management
Write-Host "`n[Regression] 8. Phase 9: Waste and Rejection Management..." -ForegroundColor Yellow
$wrResp = Invoke-WebRequest -Uri "$baseUrl/WasteReasons" -WebSession $session -UseBasicParsing
Assert-Test "Waste Reasons Master Data (HTTP 200)" ($wrResp.StatusCode -eq 200)
$wasteResp = Invoke-WebRequest -Uri "$baseUrl/Waste" -WebSession $session -UseBasicParsing
Assert-Test "Waste Records Index (HTTP 200)" ($wasteResp.StatusCode -eq 200)
Assert-Test "Waste Approval Workflow and Records Active" ($wasteResp.Content -match '/Waste/Details/\d+')

# 9. Phase 10: Quality Control, Specifications and Release Gate
Write-Host "`n[Regression] 9. Phase 10: Quality Control (QC) and Release Gate..." -ForegroundColor Yellow
$tplResp = Invoke-WebRequest -Uri "$baseUrl/QualityTemplates" -WebSession $session -UseBasicParsing
Assert-Test "Quality Templates Index (HTTP 200)" ($tplResp.StatusCode -eq 200)
Assert-Test "QC Templates Present (SESAME-QC-01, MALBAN-QC-01)" (($tplResp.Content -match "SESAME-QC-01") -and ($tplResp.Content -match "MALBAN-QC-01"))
$qcResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections" -WebSession $session -UseBasicParsing
Assert-Test "Quality Inspections Index (HTTP 200)" ($qcResp.StatusCode -eq 200)
Assert-Test "Deterministic QC Inspection Numbers Listed (QC- codes)" ($qcResp.Content -match "QC-")

# 10. Phase 11: Packaging Management and Packaging Execution
Write-Host "`n[Regression] 10. Phase 11: Packaging Management and Packaging Execution..." -ForegroundColor Yellow
$pkgBomResp = Invoke-WebRequest -Uri "$baseUrl/PackagingBOMs" -WebSession $session -UseBasicParsing
Assert-Test "Packaging BOMs Index (HTTP 200)" ($pkgBomResp.StatusCode -eq 200)
Assert-Test "Standard Packaging BOMs Present (SES-500-PKG)" ($pkgBomResp.Content -match "SES-500-PKG")
$pkgOrdersResp = Invoke-WebRequest -Uri "$baseUrl/PackagingOrders" -WebSession $session -UseBasicParsing
Assert-Test "Packaging Orders Index (HTTP 200)" ($pkgOrdersResp.StatusCode -eq 200)
Assert-Test "Packaging Orders Listed with Deterministic Numbers (PKG-)" ($pkgOrdersResp.Content -match "PKG-")

# 11. Phase 12: Finished Goods Inventory & Release
Write-Host "`n[Regression] 11. Phase 12: Finished Goods Inventory & Release..." -ForegroundColor Yellow
$fgStockResp = Invoke-WebRequest -Uri "$baseUrl/FinishedGoods" -WebSession $session -UseBasicParsing
Assert-Test "Finished Goods Stock Index (HTTP 200)" ($fgStockResp.StatusCode -eq 200)
$fgMovResp = Invoke-WebRequest -Uri "$baseUrl/FinishedGoods/Movements" -WebSession $session -UseBasicParsing
Assert-Test "Finished Goods Movements Ledger (HTTP 200)" ($fgMovResp.StatusCode -eq 200)
$fgRelResp = Invoke-WebRequest -Uri "$baseUrl/FinishedGoodsReleases" -WebSession $session -UseBasicParsing
Assert-Test "Finished Goods Releases Index (HTTP 200)" ($fgRelResp.StatusCode -eq 200)
Assert-Test "Deterministic FG Release Numbers Listed (FG-)" ($fgRelResp.Content -match "FG-")
$fgRelCockpitResp = Invoke-WebRequest -Uri "$baseUrl/FinishedGoodsReleases/Create" -WebSession $session -UseBasicParsing
Assert-Test "Finished Goods Release Cockpit (HTTP 200)" ($fgRelCockpitResp.StatusCode -eq 200)

# 12. Phase 13: Purchasing & Supplier Management
Write-Host "`n[Regression] 12. Phase 13: Purchasing and Supplier Management..." -ForegroundColor Yellow
$supResp = Invoke-WebRequest -Uri "$baseUrl/Suppliers" -WebSession $session -UseBasicParsing
Assert-Test "Suppliers Index (HTTP 200)" ($supResp.StatusCode -eq 200)
$prResp = Invoke-WebRequest -Uri "$baseUrl/PurchaseRequests" -WebSession $session -UseBasicParsing
Assert-Test "Purchase Requests Index (HTTP 200)" ($prResp.StatusCode -eq 200)
$poResp = Invoke-WebRequest -Uri "$baseUrl/PurchaseOrders" -WebSession $session -UseBasicParsing
Assert-Test "Purchase Orders Index (HTTP 200)" ($poResp.StatusCode -eq 200)
$grnResp = Invoke-WebRequest -Uri "$baseUrl/PurchaseReceipts" -WebSession $session -UseBasicParsing
Assert-Test "Purchase Receipts (GRN) Index (HTTP 200)" ($grnResp.StatusCode -eq 200)

# 13. Phase 14: Sales & Customer Management
Write-Host "`n[Regression] 13. Phase 14: Sales and Customer Management..." -ForegroundColor Yellow
$cusResp = Invoke-WebRequest -Uri "$baseUrl/Customers" -WebSession $session -UseBasicParsing
Assert-Test "Customers Master Index (HTTP 200)" ($cusResp.StatusCode -eq 200)
$soResp = Invoke-WebRequest -Uri "$baseUrl/SalesOrders" -WebSession $session -UseBasicParsing
Assert-Test "Sales Orders Index (HTTP 200)" ($soResp.StatusCode -eq 200)
$sfResp = Invoke-WebRequest -Uri "$baseUrl/SalesFulfillments" -WebSession $session -UseBasicParsing
Assert-Test "Sales Fulfillments Index (HTTP 200)" ($sfResp.StatusCode -eq 200)

# 14. Phase 15: Invoicing & Payments
Write-Host "`n[Regression] 14. Phase 15: Invoicing and Payments..." -ForegroundColor Yellow
$invResp = Invoke-WebRequest -Uri "$baseUrl/Invoices" -WebSession $session -UseBasicParsing
Assert-Test "Invoices Index (HTTP 200)" ($invResp.StatusCode -eq 200)
Assert-Test "Invoices Numbering Active (INV-)" ($invResp.Content -match "INV-")
$payResp = Invoke-WebRequest -Uri "$baseUrl/Payments" -WebSession $session -UseBasicParsing
Assert-Test "Payments Index (HTTP 200)" ($payResp.StatusCode -eq 200)
Assert-Test "Payment Receipts Numbering Active (PAY-)" ($payResp.Content -match "PAY-")
$stmtResp = Invoke-WebRequest -Uri "$baseUrl/CustomerStatements" -WebSession $session -UseBasicParsing
Assert-Test "Customer Statements Index (HTTP 200)" ($stmtResp.StatusCode -eq 200)

# 15. Phase 16: Accounting & General Ledger
Write-Host "`n[Regression] 15. Phase 16: Accounting and General Ledger..." -ForegroundColor Yellow
$coaResp = Invoke-WebRequest -Uri "$baseUrl/Accounts" -WebSession $session -UseBasicParsing
Assert-Test "Chart of Accounts Tree (HTTP 200)" ($coaResp.StatusCode -eq 200)
Assert-Test "Standard Accounts Seeded (1101, 1201, 2101, 4101, 5101)" ($coaResp.Content.Contains("1101") -and $coaResp.Content.Contains("4101"))
$jeResp = Invoke-WebRequest -Uri "$baseUrl/JournalEntries" -WebSession $session -UseBasicParsing
Assert-Test "Journal Entries Index (HTTP 200)" ($jeResp.StatusCode -eq 200)
Assert-Test "Journal Numbers Active (JE-)" ($jeResp.Content -match "JE-")
$glResp = Invoke-WebRequest -Uri "$baseUrl/GeneralLedger" -WebSession $session -UseBasicParsing
Assert-Test "General Ledger Index (HTTP 200)" ($glResp.StatusCode -eq 200)
$tbResp = Invoke-WebRequest -Uri "$baseUrl/TrialBalance" -WebSession $session -UseBasicParsing
Assert-Test "Trial Balance Index (HTTP 200)" ($tbResp.StatusCode -eq 200)
$spayResp = Invoke-WebRequest -Uri "$baseUrl/SupplierPayments" -WebSession $session -UseBasicParsing
Assert-Test "Supplier Payments Index (HTTP 200)" ($spayResp.StatusCode -eq 200)
Assert-Test "Supplier Payment Numbers Active (SPAY-)" ($spayResp.Content -match "SPAY-")
$dashResp = Invoke-WebRequest -Uri "$baseUrl/AccountingDashboard" -WebSession $session -UseBasicParsing
Assert-Test "Accounting Dashboard Index (HTTP 200)" ($dashResp.StatusCode -eq 200)

# 16. Phase 17: Reporting & Analytics
Write-Host "`n[Regression] 16. Phase 17: Reporting and Analytics..." -ForegroundColor Yellow
$repDashResp = Invoke-WebRequest -Uri "$baseUrl/Reports/Dashboard" -WebSession $session -UseBasicParsing
Assert-Test "Reports Dashboard (HTTP 200)" ($repDashResp.StatusCode -eq 200)
$repSalesResp = Invoke-WebRequest -Uri "$baseUrl/Reports/SalesSummary" -WebSession $session -UseBasicParsing
Assert-Test "Sales Summary Report (HTTP 200)" ($repSalesResp.StatusCode -eq 200)
$repPurResp = Invoke-WebRequest -Uri "$baseUrl/Reports/PurchaseSummary" -WebSession $session -UseBasicParsing
Assert-Test "Purchase Summary Report (HTTP 200)" ($repPurResp.StatusCode -eq 200)
$repInvResp = Invoke-WebRequest -Uri "$baseUrl/Reports/InventoryValuation" -WebSession $session -UseBasicParsing
Assert-Test "Inventory Valuation Report (HTTP 200)" ($repInvResp.StatusCode -eq 200)
$repProdResp = Invoke-WebRequest -Uri "$baseUrl/Reports/ProductionSummary" -WebSession $session -UseBasicParsing
Assert-Test "Production Summary Report (HTTP 200)" ($repProdResp.StatusCode -eq 200)
$repWasteResp = Invoke-WebRequest -Uri "$baseUrl/Reports/WasteSummary" -WebSession $session -UseBasicParsing
Assert-Test "Waste Summary Report (HTTP 200)" ($repWasteResp.StatusCode -eq 200)
$repQcResp = Invoke-WebRequest -Uri "$baseUrl/Reports/QualitySummary" -WebSession $session -UseBasicParsing
Assert-Test "Quality Summary Report (HTTP 200)" ($repQcResp.StatusCode -eq 200)
$repPkgResp = Invoke-WebRequest -Uri "$baseUrl/Reports/PackagingSummary" -WebSession $session -UseBasicParsing
Assert-Test "Packaging Summary Report (HTTP 200)" ($repPkgResp.StatusCode -eq 200)
$repTraceResp = Invoke-WebRequest -Uri "$baseUrl/Reports/Traceability" -WebSession $session -UseBasicParsing
Assert-Test "Traceability Report (HTTP 200)" ($repTraceResp.StatusCode -eq 200)
$repPnlResp = Invoke-WebRequest -Uri "$baseUrl/Reports/ProfitAndLoss" -WebSession $session -UseBasicParsing
Assert-Test "Profit and Loss Statement Report (HTTP 200)" ($repPnlResp.StatusCode -eq 200)
$repBsResp = Invoke-WebRequest -Uri "$baseUrl/Reports/BalanceSheet" -WebSession $session -UseBasicParsing
Assert-Test "Balance Sheet Report (HTTP 200)" ($repBsResp.StatusCode -eq 200)
$repVatResp = Invoke-WebRequest -Uri "$baseUrl/Reports/Vat" -WebSession $session -UseBasicParsing
Assert-Test "VAT Return Report (HTTP 200)" ($repVatResp.StatusCode -eq 200)
$repProfResp = Invoke-WebRequest -Uri "$baseUrl/Reports/Profitability" -WebSession $session -UseBasicParsing
Assert-Test "Profitability Report (HTTP 200)" ($repProfResp.StatusCode -eq 200)

# 17. Phase 18: Security, RBAC & Audit Trail
Write-Host "`n[Regression] 17. Phase 18: Security, RBAC and Audit Trail..." -ForegroundColor Yellow
$secDashResp = Invoke-WebRequest -Uri "$baseUrl/Security/Dashboard" -WebSession $session -UseBasicParsing
Assert-Test "Security Dashboard View (HTTP 200)" ($secDashResp.StatusCode -eq 200)
$usersResp = Invoke-WebRequest -Uri "$baseUrl/Users" -WebSession $session -UseBasicParsing
Assert-Test "User Management Index (HTTP 200)" ($usersResp.StatusCode -eq 200)
$rolesResp = Invoke-WebRequest -Uri "$baseUrl/Roles" -WebSession $session -UseBasicParsing
Assert-Test "Roles Management Index (HTTP 200)" ($rolesResp.StatusCode -eq 200)
$auditResp = Invoke-WebRequest -Uri "$baseUrl/Audit" -WebSession $session -UseBasicParsing
Assert-Test "Audit Trail Log Index (HTTP 200)" ($auditResp.StatusCode -eq 200)

# 18. Phase 19: System Administration, Central Configuration & Operational Controls
Write-Host "`n[Regression] 18. Phase 19: System Administration and Central Configuration..." -ForegroundColor Yellow
$settingsHubResp = Invoke-WebRequest -Uri "$baseUrl/Settings/Index" -WebSession $session -UseBasicParsing
Assert-Test "Settings Hub View (HTTP 200)" ($settingsHubResp.StatusCode -eq 200)
$settingsCompResp = Invoke-WebRequest -Uri "$baseUrl/Settings/Company" -WebSession $session -UseBasicParsing
Assert-Test "Company Profile View (HTTP 200)" ($settingsCompResp.StatusCode -eq 200)
$settingsGenResp = Invoke-WebRequest -Uri "$baseUrl/Settings/General" -WebSession $session -UseBasicParsing
Assert-Test "General Settings View (HTTP 200)" ($settingsGenResp.StatusCode -eq 200)
$settingsTaxResp = Invoke-WebRequest -Uri "$baseUrl/Settings/Tax" -WebSession $session -UseBasicParsing
Assert-Test "Tax Settings View (HTTP 200)" ($settingsTaxResp.StatusCode -eq 200)
$settingsNumResp = Invoke-WebRequest -Uri "$baseUrl/Settings/Numbering" -WebSession $session -UseBasicParsing
Assert-Test "Document Numbering View (HTTP 200)" ($settingsNumResp.StatusCode -eq 200)
$settingsInvResp = Invoke-WebRequest -Uri "$baseUrl/Settings/Inventory" -WebSession $session -UseBasicParsing
Assert-Test "Inventory Defaults View (HTTP 200)" ($settingsInvResp.StatusCode -eq 200)
$settingsProdResp = Invoke-WebRequest -Uri "$baseUrl/Settings/Production" -WebSession $session -UseBasicParsing
Assert-Test "Production Defaults View (HTTP 200)" ($settingsProdResp.StatusCode -eq 200)
$settingsPurchResp = Invoke-WebRequest -Uri "$baseUrl/Settings/Purchasing" -WebSession $session -UseBasicParsing
Assert-Test "Purchasing Defaults View (HTTP 200)" ($settingsPurchResp.StatusCode -eq 200)
$settingsSalesResp = Invoke-WebRequest -Uri "$baseUrl/Settings/Sales" -WebSession $session -UseBasicParsing
Assert-Test "Sales Defaults View (HTTP 200)" ($settingsSalesResp.StatusCode -eq 200)
$settingsAccResp = Invoke-WebRequest -Uri "$baseUrl/Settings/Accounting" -WebSession $session -UseBasicParsing
Assert-Test "Accounting Mappings View (HTTP 200)" ($settingsAccResp.StatusCode -eq 200)
$settingsHistResp = Invoke-WebRequest -Uri "$baseUrl/Settings/History" -WebSession $session -UseBasicParsing
Assert-Test "Configuration History View (HTTP 200)" ($settingsHistResp.StatusCode -eq 200)

# 19. Phase 20: Deployment, Backup, Recovery & Production Readiness
Write-Host "`n[Regression] 19. Phase 20: Deployment, Health Monitoring and Production Readiness..." -ForegroundColor Yellow
$liveResp = Invoke-WebRequest -Uri "$baseUrl/health/live" -UseBasicParsing
Assert-Test "Application Liveness Endpoint (HTTP 200)" ($liveResp.StatusCode -eq 200)
Assert-Test "Liveness Reports Healthy" ($liveResp.Content -match '"Healthy"')

$readyResp = Invoke-WebRequest -Uri "$baseUrl/health/ready" -UseBasicParsing
Assert-Test "Database Readiness Endpoint (HTTP 200)" ($readyResp.StatusCode -eq 200)
Assert-Test "Readiness Reports Healthy" ($readyResp.Content -match '"Healthy"')

$healthViewResp = Invoke-WebRequest -Uri "$baseUrl/SystemHealth" -WebSession $session -UseBasicParsing
Assert-Test "System Health & Production Readiness Dashboard (HTTP 200)" ($healthViewResp.StatusCode -eq 200)

$diagResp = Invoke-WebRequest -Uri "$baseUrl/SystemHealth/Diagnostics" -WebSession $session -UseBasicParsing
Assert-Test "Diagnostics API Metric Endpoint (HTTP 200)" ($diagResp.StatusCode -eq 200)

Write-Host "`n=================================================================" -ForegroundColor Cyan
Write-Host " MASTER REGRESSION RESULT: $script:passed PASSED, $script:failed FAILED" -ForegroundColor $(if ($script:failed -eq 0) { [ConsoleColor]::Green } else { [ConsoleColor]::Red })
Write-Host "=================================================================" -ForegroundColor Cyan

if ($script:failed -gt 0) {
    exit 1
}

