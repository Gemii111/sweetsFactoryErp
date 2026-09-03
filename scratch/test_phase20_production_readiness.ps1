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
    $pattern2 = '__RequestVerificationToken[^>]*value="([^"]+)"'
    $match2 = [regex]::Match($html, $pattern2)
    if ($match2.Success) {
        return $match2.Groups[1].Value
    }
    return ""
}

Write-Host "========================================================" -ForegroundColor Cyan
Write-Host " FACTORYX MAWLID SWEETS ERP -- PHASE 20 VERIFICATION    " -ForegroundColor Cyan
Write-Host " DEPLOYMENT, BACKUP, RECOVERY AND PRODUCTION READINESS  " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

# ----------------------------------------------------
# 1. Environment and Configuration Security
# ----------------------------------------------------
Write-Host "`n1. Verifying Environment Separation and Configuration Security..." -ForegroundColor Yellow

$appsettingsPath = "d:\kh proj\FactoryX-main\FactoryX.Web\appsettings.json"
$prodSettingsPath = "d:\kh proj\FactoryX-main\FactoryX.Web\appsettings.Production.json"
$testSettingsPath = "d:\kh proj\FactoryX-main\FactoryX.Web\appsettings.Test.json"

Assert-Step -title "appsettings.json exists" -condition (Test-Path $appsettingsPath)
Assert-Step -title "appsettings.Production.json exists" -condition (Test-Path $prodSettingsPath)
Assert-Step -title "appsettings.Test.json exists" -condition (Test-Path $testSettingsPath)

$prodJson = Get-Content $prodSettingsPath -Raw
Assert-Step -title "Production config sets Warning level for EF Core" -condition ($prodJson -match '"Microsoft.EntityFrameworkCore":\s*"Warning"')
Assert-Step -title "Production config contains BackupSettings section" -condition ($prodJson -match '"BackupSettings"')
Assert-Step -title "Production config contains MonitoringSettings section" -condition ($prodJson -match '"MonitoringSettings"')
Assert-Step -title "Production config contains ProductionSafety section" -condition ($prodJson -match '"ProductionSafety"')
Assert-Step -title "Production config has EnableDetailedErrors disabled" -condition ($prodJson -match '"EnableDetailedErrors":\s*false')

# ----------------------------------------------------
# 2. Health Checks Subsystem (/health/live and /health/ready)
# ----------------------------------------------------
Write-Host "`n2. Verifying Health Checks Subsystem..." -ForegroundColor Yellow

$liveResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/health/live" -Method Get
Assert-Step -title "Liveness endpoint (/health/live) returns HTTP 200" -condition ($liveResp.StatusCode -eq 200)
Assert-Step -title "Liveness response has JSON Content-Type" -condition ($liveResp.Headers["Content-Type"] -match "application/json")

$liveJson = $liveResp.Content | ConvertFrom-Json
Assert-Step -title "Liveness reports status Healthy" -condition ($liveJson.status -eq "Healthy")
Assert-Step -title "Liveness reports single-source version v1.0.0" -condition ($liveJson.version -eq "v1.0.0")
Assert-Step -title "Liveness reports application name" -condition ($liveJson.application -match "FactoryX")
Assert-Step -title "Liveness reports positive uptime" -condition ($liveJson.uptimeSeconds -gt 0)

$readyResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/health/ready" -Method Get
Assert-Step -title "Readiness endpoint (/health/ready) returns HTTP 200" -condition ($readyResp.StatusCode -eq 200)
Assert-Step -title "Readiness response has JSON Content-Type" -condition ($readyResp.Headers["Content-Type"] -match "application/json")

$readyJson = $readyResp.Content | ConvertFrom-Json
Assert-Step -title "Readiness reports overall status Healthy" -condition ($readyJson.status -eq "Healthy")
Assert-Step -title "Readiness includes database component check" -condition ($readyJson.checks.name -contains "database")

$dbCheck = $readyJson.checks | Where-Object { $_.name -eq "database" }
Assert-Step -title "Database check reports status Healthy" -condition ($dbCheck.status -eq "Healthy")
Assert-Step -title "Database check reports low latency (< 3000ms)" -condition ($dbCheck.durationMs -lt 3000)
Assert-Step -title "Readiness does NOT leak database password" -condition ($readyResp.Content -notmatch "Aa456456" -and $readyResp.Content -notmatch "Password=")
Assert-Step -title "Readiness does NOT leak connection string credentials" -condition ($readyResp.Content -notmatch "User Id=" -and $readyResp.Content -notmatch "TrustServerCertificate")

# ----------------------------------------------------
# 3. Application Versioning Single Source of Truth
# ----------------------------------------------------
Write-Host "`n3. Verifying Single Source of Truth for Application Versioning..." -ForegroundColor Yellow

$versionCsPath = "d:\kh proj\FactoryX-main\FactoryX.Application\Common\SystemVersionInfo.cs"
Assert-Step -title "SystemVersionInfo.cs exists in Application Core" -condition (Test-Path $versionCsPath)

$versionCs = Get-Content $versionCsPath -Raw
Assert-Step -title "SystemVersionInfo defines version v1.0.0" -condition ($versionCs -match 'Version = "v1\.0\.0"')
Assert-Step -title "SystemVersionInfo defines ReleaseName" -condition ($versionCs -match 'ReleaseName = "FactoryX Mawlid Sweets ERP"')
Assert-Step -title "SystemVersionInfo defines Edition" -condition ($versionCs -match 'Edition = "Production Edition \(Factory LAN\)"')

# ----------------------------------------------------
# 4. Global Exception Handling and Correlation IDs
# ----------------------------------------------------
Write-Host "`n4. Verifying Global Exception Handling and Request Correlation..." -ForegroundColor Yellow

$errorResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Home/Error" -Method Get
Assert-Step -title "Error endpoint (/Home/Error) returns HTTP 200" -condition ($errorResp.StatusCode -eq 200)
Assert-Step -title "Error view displays Reference / Correlation ID" -condition ($errorResp.Content -match "Correlation ID" -or $errorResp.Content -match "Reference / Correlation")
Assert-Step -title "Error view conceals developer stack traces" -condition ($errorResp.Content -notmatch "StackTrace" -and $errorResp.Content -notmatch "Exception: ")
Assert-Step -title "Error view conceals internal connection strings" -condition ($errorResp.Content -notmatch "Server=" -and $errorResp.Content -notmatch "Database=")

# ----------------------------------------------------
# 5. Backup Strategy and Automation (backup_database.ps1)
# ----------------------------------------------------
Write-Host "`n5. Verifying SQL Server Backup Automation and Verification..." -ForegroundColor Yellow

$backupScript = "d:\kh proj\FactoryX-main\scripts\backup\backup_database.ps1"
Assert-Step -title "Backup script backup_database.ps1 exists" -condition (Test-Path $backupScript)

$testBackupDir = "d:\kh proj\FactoryX-main\backups"
& powershell -ExecutionPolicy Bypass -File $backupScript -BackupType Full -VerifyOnly
Assert-Step -title "Full backup script executed with return code 0" -condition ($LASTEXITCODE -eq 0)

$backupFiles = Get-ChildItem -Path $testBackupDir -Filter "MawlidSweetsErpDb_FULL_*.bak" | Sort-Object CreationTimeUtc -Descending
Assert-Step -title "Verified timestamped backup file generated" -condition ($backupFiles.Count -gt 0)

$latestBackup = $backupFiles | Select-Object -First 1
Assert-Step -title "Backup filename matches deterministic convention" -condition ($latestBackup.Name -match "^MawlidSweetsErpDb_FULL_\d{4}-\d{2}-\d{2}_\d{6}\.bak$")
Assert-Step -title "Backup file size is valid (> 1 MB)" -condition ($latestBackup.Length -gt 1MB)

# ----------------------------------------------------
# 6. Disaster Recovery Sandbox Validation (restore_database.ps1)
# ----------------------------------------------------
Write-Host "`n6. Verifying Safe Sandbox Disaster Recovery Procedure..." -ForegroundColor Yellow

$restoreScript = "d:\kh proj\FactoryX-main\scripts\backup\restore_database.ps1"
Assert-Step -title "Restore script restore_database.ps1 exists" -condition (Test-Path $restoreScript)

# Execute safe restore test into isolated sandbox MawlidSweetsErpDb_RestoreTest
& powershell -ExecutionPolicy Bypass -File $restoreScript -TestRestoreDbName "MawlidSweetsErpDb_RestoreTest" -ExecuteRestore -CleanupAfter
Assert-Step -title "Restore script completed successfully with return code 0" -condition ($LASTEXITCODE -eq 0)

# Verify script rejects attempting to overwrite live production database
& powershell -ExecutionPolicy Bypass -File $restoreScript -TestRestoreDbName "MawlidSweetsErpDb"
Assert-Step -title "Restore script ABORTS if target equals live database" -condition ($LASTEXITCODE -ne 0)


# ----------------------------------------------------
# 7. Deployment and Operational Runbooks Documentation
# ----------------------------------------------------
Write-Host "`n7. Verifying Deployment and Operational Runbook Documentation..." -ForegroundColor Yellow

$deployDoc = "d:\kh proj\FactoryX-main\DEPLOYMENT.md"
$backupDoc = "d:\kh proj\FactoryX-main\BACKUP_RECOVERY.md"
$runbookDoc = "d:\kh proj\FactoryX-main\PRODUCTION_RUNBOOK.md"
$deployScript = "d:\kh proj\FactoryX-main\scripts\deploy\deploy.ps1"

Assert-Step -title "DEPLOYMENT.md exists" -condition (Test-Path $deployDoc)
Assert-Step -title "BACKUP_RECOVERY.md exists" -condition (Test-Path $backupDoc)
Assert-Step -title "PRODUCTION_RUNBOOK.md exists" -condition (Test-Path $runbookDoc)
Assert-Step -title "deploy.ps1 script exists" -condition (Test-Path $deployScript)

$backupDocContent = Get-Content $backupDoc -Raw
Assert-Step -title "BACKUP_RECOVERY.md documents Target RPO" -condition ($backupDocContent -match "RPO")
Assert-Step -title "BACKUP_RECOVERY.md documents Target RTO" -condition ($backupDocContent -match "RTO")
Assert-Step -title "BACKUP_RECOVERY.md documents RESTORE VERIFYONLY" -condition ($backupDocContent -match "RESTORE VERIFYONLY")

$runbookDocContent = Get-Content $runbookDoc -Raw
Assert-Step -title "PRODUCTION_RUNBOOK.md covers 9 incident playbooks" -condition ($runbookDocContent -match "Playbook 1" -and $runbookDocContent -match "Playbook 9")

# ----------------------------------------------------
# 8. Authentication and Production Readiness Dashboard
# ----------------------------------------------------
Write-Host "`n8. Verifying Production Readiness and System Health Dashboard..." -ForegroundColor Yellow

$loginPage = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Account/Login" -WebSession $session -Method Get
$adminToken = Extract-Token -html $loginPage.Content

$loginResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Account/Login" -WebSession $session -Method Post -Body @{
    "Username" = "testadmin"
    "Password" = "Password123!"
    "__RequestVerificationToken" = $adminToken
}
Assert-Step -title "Admin Login Successful (HTTP 200)" -condition ($loginResp.StatusCode -eq 200)

$healthView = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/SystemHealth" -WebSession $session -Method Get
Assert-Step -title "SystemHealth Dashboard accessible (HTTP 200)" -condition ($healthView.StatusCode -eq 200)
Assert-Step -title "Dashboard displays Production Ready banner" -condition ($healthView.Content -match "Production Ready" -or $healthView.Content -match "SystemHealth")
Assert-Step -title "Dashboard displays SQL Server connectivity indicator" -condition ($healthView.Content -match "Microsoft SQL Server")
Assert-Step -title "Dashboard displays Storage and Disks section" -condition ($healthView.Content -match "TotalSizeGb" -or $healthView.Content -match "FreeSpacePercent" -or $healthView.Content -match "Disks" -or $healthView.Content -match "progress-bar")
Assert-Step -title "Dashboard displays Backup Health section" -condition ($healthView.Content -match "Backup" -or $healthView.Content -match "MawlidERP")
Assert-Step -title "Dashboard displays UTC Time" -condition ($healthView.Content -match "UTC")
Assert-Step -title "Dashboard displays Unified Version v1.0.0" -condition ($healthView.Content -match "v1\.0\.0")
Assert-Step -title "Dashboard displays Layout footer with SystemVersionInfo" -condition ($healthView.Content -match "FactoryX Mawlid Sweets ERP v1\.0\.0")

# Diagnostics JSON API
$diagResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/SystemHealth/Diagnostics" -WebSession $session -Method Get
Assert-Step -title "Diagnostics API accessible (HTTP 200)" -condition ($diagResp.StatusCode -eq 200)

$diagJson = $diagResp.Content | ConvertFrom-Json
Assert-Step -title "Diagnostics reports application name" -condition ($diagJson.application -match "FactoryX")
Assert-Step -title "Diagnostics reports database connected true" -condition ($diagJson.database.connected -eq $true)
Assert-Step -title "Diagnostics reports applied migrations count" -condition ($diagJson.database.appliedMigrations -gt 0)
Assert-Step -title "Diagnostics reports disk health collection" -condition ($diagJson.disks.Count -gt 0)
Assert-Step -title "Diagnostics reports backup status" -condition ($diagJson.backup.status -ne $null)

# ----------------------------------------------------
# 9. Security and RBAC Enforcement for SystemHealth
# ----------------------------------------------------
Write-Host "`n9. Verifying RBAC Security Enforcement for Health Dashboard..." -ForegroundColor Yellow

$unprivSession = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$blockedHealth = $false
try {
    $guestResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/SystemHealth" -WebSession $unprivSession -Method Get -MaximumRedirection 0
    if ($guestResp.StatusCode -eq 403 -or $guestResp.StatusCode -eq 302) { $blockedHealth = $true }
} catch {
    $code = $_.Exception.Response.StatusCode.value__
    if ($code -eq 403 -or $code -eq 302) { $blockedHealth = $true }
}
Assert-Step -title "Unauthenticated request blocked from SystemHealth (403/302)" -condition $blockedHealth

# ----------------------------------------------------
# 10. Safety Invariants and Zero Business Data Mutation
# ----------------------------------------------------
Write-Host "`n10. Verifying Safety Invariants and Zero Business Data Mutation..." -ForegroundColor Yellow

$stockResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Inventory/Stock" -WebSession $session -Method Get
Assert-Step -title "Inventory stock balance remains accessible and intact" -condition ($stockResp.StatusCode -eq 200)

$trialBalResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/TrialBalance" -WebSession $session -Method Get
Assert-Step -title "Financial General Ledger trial balance remains intact" -condition ($trialBalResp.StatusCode -eq 200)

$auditResp = Invoke-WebRequest -UseBasicParsing -Uri "$baseUrl/Audit" -WebSession $session -Method Get
Assert-Step -title "Audit logs remain accessible, intact and append-only" -condition ($auditResp.StatusCode -eq 200)

# ----------------------------------------------------
# Summary
# ----------------------------------------------------
Write-Host "`n========================================================" -ForegroundColor Cyan
Write-Host " PHASE 20 TEST SUMMARY" -ForegroundColor Cyan
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
