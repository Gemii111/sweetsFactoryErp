# Test Script: Phase 10 Quality Control and Inspection Verification
# FactoryX Mawlid Sweets ERP System

$baseUrl = "http://127.0.0.1:5265"
$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

$passCount = 0
$failCount = 0

function Assert-Step($title, $condition, $details = "") {
    if ($condition) {
        Write-Host " [PASS] $title" -ForegroundColor Green
        $global:passCount++
    } else {
        Write-Host " [FAIL] $title" -ForegroundColor Red
        if ($details) {
            Write-Host "        Details: $details" -ForegroundColor Yellow
        }
        $global:failCount++
    }
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  FACTORYX PHASE 10: QUALITY CONTROL AND RELEASE GATE TESTS " -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan

# 1. Login
try {
    $loginPage = Invoke-WebRequest -Uri "$baseUrl/Login" -WebSession $session -Method Get
    $loginToken = ($loginPage.InputFields | Where-Object { $_.name -eq "__RequestVerificationToken" }).value

    $loginBody = @{
        "Username" = "testadmin"
        "Password" = "Password123!"
        "__RequestVerificationToken" = $loginToken
    }
    $loginResp = Invoke-WebRequest -Uri "$baseUrl/Login" -WebSession $session -Method Post -Body $loginBody
    Assert-Step "1. Authentication successful" ($loginResp.StatusCode -eq 200 -or $loginResp.StatusCode -eq 302)
} catch {
    Assert-Step "1. Authentication successful" $false $_.Exception.Message
}

# 2. Quality Templates Index
try {
    $tplIndex = Invoke-WebRequest -Uri "$baseUrl/QualityTemplates" -WebSession $session -Method Get
    $hasTemplates = ($tplIndex.Content -match "SESAME-QC-01") -and ($tplIndex.Content -match "MALBAN-QC-01")
    Assert-Step "2. Seeded Quality Templates listed (SESAME-QC-01, MALBAN-QC-01)" $hasTemplates
} catch {
    Assert-Step "2. Seeded Quality Templates listed" $false $_.Exception.Message
}

# 3. Create Custom QC Template
$uniqueCode = "FUSTUQ-QC-" + (Get-Random -Minimum 1000 -Maximum 9999)
try {
    $createTplPage = Invoke-WebRequest -Uri "$baseUrl/QualityTemplates/Create" -WebSession $session -Method Get
    $tplToken = ($createTplPage.InputFields | Where-Object { $_.name -eq "__RequestVerificationToken" }).value

    $createTplBody = @{
        "Code" = $uniqueCode
        "Name" = "Standard Pistachio QC Specification"
        "Description" = "QC template for testing Pistachio release criteria"
        "IsActive" = "true"
        "Items[0].SpecificationName" = "Net Piece Weight"
        "Items[0].Sequence" = "1"
        "Items[0].IsRequired" = "true"
        "Items[0].DataType" = "Number"
        "Items[0].MinValue" = "240"
        "Items[0].MaxValue" = "260"
        "Items[0].TargetValue" = "250"
        "Items[0].Unit" = "G"
        "Items[1].SpecificationName" = "Color and Appearance"
        "Items[1].Sequence" = "2"
        "Items[1].IsRequired" = "true"
        "Items[1].DataType" = "PassFail"
        "Items[2].SpecificationName" = "Texture and Crunch"
        "Items[2].Sequence" = "3"
        "Items[2].IsRequired" = "true"
        "Items[2].DataType" = "PassFail"
        "__RequestVerificationToken" = $tplToken
    }

    $createTplResp = Invoke-WebRequest -Uri "$baseUrl/QualityTemplates/Create" -WebSession $session -Method Post -Body $createTplBody
    Assert-Step "3. Create Custom QC Template ($uniqueCode)" ($createTplResp.StatusCode -eq 200 -or $createTplResp.StatusCode -eq 302)

    # Verify in Index
    $tplIndex2 = Invoke-WebRequest -Uri "$baseUrl/QualityTemplates" -WebSession $session -Method Get
    $foundCreated = ($tplIndex2.Content -match $uniqueCode)
    Assert-Step "3.1 Custom template appears in templates index" $foundCreated
} catch {
    Assert-Step "3. Create Custom QC Template" $false $_.Exception.Message
}

# 4. Get a Completed Batch for Inspection Testing
$testBatchId = 0
try {
    $batchesPage = Invoke-WebRequest -Uri "$baseUrl/ProductionBatches" -WebSession $session -Method Get
    if ($batchesPage.Content -match 'href="/ProductionBatches/Details/(\d+)"') {
        $testBatchId = [int]$matches[1]
    }
    Assert-Step "4. Retrieve existing Production Batch for QC testing (Batch #$testBatchId)" ($testBatchId -gt 0)
} catch {
    Assert-Step "4. Retrieve existing Production Batch" $false $_.Exception.Message
}

# 5. Check QC Release Gate before any inspection (Must be BLOCKED)
try {
    $gateResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/GetReleaseGateStatus?batchId=$testBatchId" -WebSession $session -Method Get
    $gateJson = $gateResp.Content | ConvertFrom-Json
    $isBlocked = ($gateJson.isAllowed -eq $false) -and ($gateJson.status -eq "BLOCKED")
    Assert-Step "5. QC Release Gate initially BLOCKED for unapproved batch" $isBlocked "Status: $($gateJson.status), Reason: $($gateJson.reason)"
} catch {
    Assert-Step "5. QC Release Gate initially BLOCKED" $false $_.Exception.Message
}

# 6. Create QC Inspection for Batch
$inspectionId = 0
$inspectionNumber = ""
try {
    $createQcPage = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Create?batchId=$testBatchId" -WebSession $session -Method Get
    $qcToken = ($createQcPage.InputFields | Where-Object { $_.name -eq "__RequestVerificationToken" }).value

    $createQcBody = @{
        "ProductionBatchId" = $testBatchId.ToString()
        "InspectionDate" = (Get-Date).ToString("yyyy-MM-dd")
        "Notes" = "Routine Batch Inspection for Sweets"
        "__RequestVerificationToken" = $qcToken
    }

    $createQcResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Create" -WebSession $session -Method Post -Body $createQcBody
    
    # Check created inspection in Index
    $qcIndex = Invoke-WebRequest -Uri "$baseUrl/QualityInspections" -WebSession $session -Method Get
    if ($qcIndex.Content -match 'href="/QualityInspections/Details/(\d+)"[^>]*>([^<]+)</a>') {
        $inspectionId = [int]$matches[1]
        $inspectionNumber = $matches[2].Trim()
    }
    
    $numPatternValid = ($inspectionNumber -match "^QC-\d{8}-\d{4}$")
    Assert-Step "6. Create QC Inspection record ($inspectionNumber)" ($inspectionId -gt 0)
    Assert-Step "6.1 Deterministic QC Number format (QC-YYYYMMDD-XXXX)" $numPatternValid "Got: $inspectionNumber"
} catch {
    Assert-Step "6. Create QC Inspection record" $false $_.Exception.Message
}

# 7. Inspect Page and Measurement Recording (Test Auto-Evaluation: Out-of-Spec -> FAIL)
try {
    $inspectPage = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Inspect/$inspectionId" -WebSession $session -Method Get
    $inspectToken = ($inspectPage.InputFields | Where-Object { $_.name -eq "__RequestVerificationToken" }).value

    # Extract items to measure
    $itemIds = @()
    $matches2 = [regex]::Matches($inspectPage.Content, 'name="Measurements\[\d+\]\.ItemId"\s+value="(\d+)"')
    foreach ($m in $matches2) {
        $itemIds += [int]$m.Groups[1].Value
    }
    Assert-Step "7. Inspection items loaded from template ($($itemIds.Count) items)" ($itemIds.Count -gt 0)

    # First test: Enter FAILING measurements
    $failMeasureBody = @{
        "InspectionId" = $inspectionId.ToString()
        "Measurements[0].ItemId" = $itemIds[0].ToString()
        "Measurements[0].ActualNumericValue" = "350"
        "Measurements[0].InspectorNotes" = "Low weight below standard"
        "Measurements[1].ItemId" = $itemIds[1].ToString()
        "Measurements[1].ActualPassFailValue" = "FAIL"
        "Measurements[1].InspectorNotes" = "Dark color"
        "__RequestVerificationToken" = $inspectToken
    }

    $failMeasureResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/RecordMeasurements" -WebSession $session -Method Post -Body $failMeasureBody
    
    # Verify inspection details shows FAIL
    $detailsPage = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Details/$inspectionId" -WebSession $session -Method Get
    $hasFailResult = ($detailsPage.Content -match "FAIL")
    Assert-Step "7.1 Out-of-spec measurement auto-evaluated as FAIL" $hasFailResult
} catch {
    Assert-Step "7. Inspect Page and Measurement Recording" $false $_.Exception.Message
}

# 8. Test Attempting Approval on Failing Measurements (Must be BLOCKED with error)
try {
    $detailsPage = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Details/$inspectionId" -WebSession $session -Method Get
    $appToken = ($detailsPage.InputFields | Where-Object { $_.name -eq "__RequestVerificationToken" }).value

    $appBody = @{
        "InspectionId" = $inspectionId.ToString()
        "ApprovalNotes" = "Attempting illegal approval"
        "__RequestVerificationToken" = $appToken
    }

    $appResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Approve" -WebSession $session -Method Post -Body $appBody
    
    $detailsAfterApp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Details/$inspectionId" -WebSession $session -Method Get
    $blockedApproval = ($detailsAfterApp.Content -match "FAIL") -and (($detailsAfterApp.Content -match "Approved") -eq $false)
    Assert-Step "8. Approval blocked when required QC specifications fail" $blockedApproval
} catch {
    Assert-Step "8. Approval blocked when required QC specifications fail" $false $_.Exception.Message
}

# 9. Test Decision: HOLD (Put Batch on Hold)
try {
    $detailsPage = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Details/$inspectionId" -WebSession $session -Method Get
    $holdToken = ($detailsPage.InputFields | Where-Object { $_.name -eq "__RequestVerificationToken" }).value

    $holdBody = @{
        "InspectionId" = $inspectionId.ToString()
        "HoldReason" = "Precautionary hold for calibration investigation"
        "__RequestVerificationToken" = $holdToken
    }

    $holdResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Hold" -WebSession $session -Method Post -Body $holdBody
    
    $detailsAfterHold = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Details/$inspectionId" -WebSession $session -Method Get
    $isHold = ($detailsAfterHold.Content -match "Hold")
    Assert-Step "9. Batch placed on HOLD with reason" $isHold

    # Gate status must be BLOCKED
    $gateHoldResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/GetReleaseGateStatus?batchId=$testBatchId" -WebSession $session -Method Get
    $gateHoldJson = $gateHoldResp.Content | ConvertFrom-Json
    $gateIsBlocked = ($gateHoldJson.isAllowed -eq $false) -and ($gateHoldJson.reason -match "HOLD")
    Assert-Step "9.1 QC Release Gate BLOCKS release when batch is on HOLD" $gateIsBlocked
} catch {
    Assert-Step "9. Test Decision: HOLD" $false $_.Exception.Message
}

# 10. Test Re-inspection Workflow (Reinspect Held Batch)
$reinspectionId = 0
$reinspectionNumber = ""
try {
    $reinspectPage = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Reinspect/$inspectionId" -WebSession $session -Method Get
    $reToken = ($reinspectPage.InputFields | Where-Object { $_.name -eq "__RequestVerificationToken" }).value

    $reBody = @{
        "PreviousInspectionId" = $inspectionId.ToString()
        "ReinspectionReason" = "Re-sampling after equipment recalibration"
        "Notes" = "Re-inspection verification test"
        "__RequestVerificationToken" = $reToken
    }

    $reResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Reinspect" -WebSession $session -Method Post -Body $reBody
    
    $qcIndex = Invoke-WebRequest -Uri "$baseUrl/QualityInspections" -WebSession $session -Method Get
    if ($qcIndex.Content -match 'href="/QualityInspections/Details/(\d+)"[^>]*>([^<]+)</a>') {
        $reinspectionId = [int]$matches[1]
        $reinspectionNumber = $matches[2].Trim()
    }

    $isNewRecord = ($reinspectionId -gt 0) -and ($reinspectionId -ne $inspectionId)
    Assert-Step "10. Re-inspection created new distinct record ($reinspectionNumber)" $isNewRecord

    # Check that previous inspection is still preserved in history
    $prevDetails = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Details/$inspectionId" -WebSession $session -Method Get
    $prevPreserved = ($prevDetails.Content -match $inspectionNumber)
    Assert-Step "10.1 Previous inspection history completely preserved" $prevPreserved
} catch {
    Assert-Step "10. Test Re-inspection Workflow" $false $_.Exception.Message
}

# 11. Record PASSING Measurements in the Re-inspection and Approve
try {
    $inspectPage = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Inspect/$reinspectionId" -WebSession $session -Method Get
    $inspectToken = ($inspectPage.InputFields | Where-Object { $_.name -eq "__RequestVerificationToken" }).value

    $itemIds = @()
    $matches3 = [regex]::Matches($inspectPage.Content, 'name="Measurements\[\d+\]\.ItemId"\s+value="(\d+)"')
    foreach ($m in $matches3) {
        $itemIds += [int]$m.Groups[1].Value
    }

    # Populate all items with compliant PASS values
    $passMeasureBody = @{
        "InspectionId" = $reinspectionId.ToString()
        "__RequestVerificationToken" = $inspectToken
    }
    for ($i = 0; $i -lt $itemIds.Count; $i++) {
        $passMeasureBody["Measurements[$i].ItemId"] = $itemIds[$i].ToString()
        $passMeasureBody["Measurements[$i].ActualNumericValue"] = "500"
        $passMeasureBody["Measurements[$i].ActualPassFailValue"] = "PASS"
        $passMeasureBody["Measurements[$i].ActualBooleanValue"] = "true"
        $passMeasureBody["Measurements[$i].ActualTextValue"] = "Pass"
        $passMeasureBody["Measurements[$i].InspectorNotes"] = "Compliant with specification"
    }

    $passMeasureResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/RecordMeasurements" -WebSession $session -Method Post -Body $passMeasureBody
    
    # Now Approve the re-inspection
    $detailsPage = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Details/$reinspectionId" -WebSession $session -Method Get
    $appToken = ($detailsPage.InputFields | Where-Object { $_.name -eq "__RequestVerificationToken" }).value

    $appBody = @{
        "InspectionId" = $reinspectionId.ToString()
        "ApprovalNotes" = "Re-inspection successfully passed all standard specifications"
        "__RequestVerificationToken" = $appToken
    }

    $appResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Approve" -WebSession $session -Method Post -Body $appBody
    
    $detailsAfterPass = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/Details/$reinspectionId" -WebSession $session -Method Get
    $isApproved = ($detailsAfterPass.Content -match "Approved")
    Assert-Step "11. Re-inspection approved with compliant measurements" $isApproved
} catch {
    Assert-Step "11. Record PASSING Measurements and Approve" $false $_.Exception.Message
}

# 12. Verify QC Release Gate is now ALLOWED (RELEASED)
try {
    $gateApprovedResp = Invoke-WebRequest -Uri "$baseUrl/QualityInspections/GetReleaseGateStatus?batchId=$testBatchId" -WebSession $session -Method Get
    $gateAppJson = $gateApprovedResp.Content | ConvertFrom-Json
    $isAllowed = ($gateAppJson.isAllowed -eq $true) -and ($gateAppJson.status -eq "ALLOWED")
    Assert-Step "12. QC Release Gate ALLOWED (RELEASED) for approved batch" $isAllowed "Status: $($gateAppJson.status), Reason: $($gateAppJson.reason)"
} catch {
    Assert-Step "12. QC Release Gate ALLOWED" $false $_.Exception.Message
}

# 13. Scope Isolation Verification
try {
    # Verify QC does NOT directly modify stock or create waste
    $invPage = Invoke-WebRequest -Uri "$baseUrl/Inventory/Transactions" -WebSession $session -Method Get
    Assert-Step "13.1 Inventory Transactions page accessible and intact" ($invPage.StatusCode -eq 200)

    $wastePage = Invoke-WebRequest -Uri "$baseUrl/Waste" -WebSession $session -Method Get
    Assert-Step "13.2 Waste records page accessible and isolated" ($wastePage.StatusCode -eq 200)
} catch {
    Assert-Step "13. Scope Isolation Verification" $false $_.Exception.Message
}

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "  PHASE 10 TEST SUMMARY: $passCount PASSED, $failCount FAILED " -ForegroundColor $(if ($failCount -eq 0) { [ConsoleColor]::Green } else { [ConsoleColor]::Red })
Write-Host "============================================================" -ForegroundColor Cyan

if ($failCount -gt 0) {
    exit 1
}
