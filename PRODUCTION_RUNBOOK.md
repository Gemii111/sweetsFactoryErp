# FactoryX Mawlid Sweets ERP — Production Operations Runbook

This runbook outlines standard operating procedures (SOPs) for the IT and systems administration team in response to operational disruptions.

---

## Playbook 1: Application Outage (Web Server Down / 502 / 503)
- **Symptoms:** Users report inability to load the ERP interface; browser shows 502 Bad Gateway or 503 Service Unavailable.
- **Immediate Actions:**
  1. Check if application pool is running:
     ```powershell
     Get-WebAppPoolState -Name "FactoryXAppPool"
     ```
  2. If stopped, inspect Event Viewer (`Application` log, Source: `AspNetCoreModuleV2` or `.NET Runtime`).
  3. Start the application pool:
     ```powershell
     Start-WebAppPool -Name "FactoryXAppPool"
     ```
  4. Query `/health/live` to confirm process responsiveness.

---

## Playbook 2: Database Outage / Connectivity Loss
- **Symptoms:** `/health/ready` returns 503 Unhealthy; pages display "تعذر الاتصال بقاعدة بيانات SQL Server".
- **Immediate Actions:**
  1. Ping the SQL Server IP (`192.168.1.90`) to verify network routing.
  2. Verify SQL Server Windows Service status:
     ```powershell
     Get-Service -Name "MSSQLSERVER"
     ```
  3. Verify SQL Server port `1433` is open and not blocked by local Windows Firewall.
  4. Once database service restarts, test connection via:
     ```powershell
     Test-NetConnection -ComputerName 192.168.1.90 -Port 1433
     ```
  5. Check `/health/ready` until status returns 200 OK.

---

## Playbook 3: Server Hardware Failure
- **Symptoms:** Physical host machine is unresponsive or damaged.
- **Immediate Actions:**
  1. Switch to standby server hardware.
  2. Install/configure IIS and .NET 9 Hosting Bundle.
  3. Copy last deployed application directory from deployment backup repository.
  4. Restore the latest verified database backup as detailed in [BACKUP_RECOVERY.md](file:///d:/kh%20proj/FactoryX-main/BACKUP_RECOVERY.md).
  5. Update factory DNS or static IP mapping to point to the new host.

---

## Playbook 4: Disk Space Exhaustion (Disk &lt; 10%)
- **Symptoms:** `/SystemHealth` displays critical disk warning (red badge); SQL Server stops transactions.
- **Immediate Actions:**
  1. Do NOT delete database `.mdf` or `.ldf` files!
  2. Clean up IIS web logs in `C:\inetpub\logs\LogFiles\` older than 60 days.
  3. Clean up Windows Temp directory (`C:\Windows\Temp\`).
  4. In the backup directory, verify that verified older backups beyond the 30-day retention window can be moved to external cold storage.

---

## Playbook 5: Database Corruption
- **Symptoms:** SQL Server raises error 823/824; table queries fail with I/O or page corruption errors.
- **Immediate Actions:**
  1. Run DBCC CHECKDB:
     ```sql
     DBCC CHECKDB ('MawlidSweetsErpDb') WITH NO_INFOMSGS;
     ```
  2. If non-repairable allocation errors exist, set the database offline:
     ```sql
     ALTER DATABASE [MawlidSweetsErpDb] SET OFFLINE WITH ROLLBACK IMMEDIATE;
     ```
  3. Execute full restore from the latest verified backup file:
     ```powershell
     powershell -ExecutionPolicy Bypass -File scripts\backup\restore_database.ps1 -ProductionDbName MawlidSweetsErpDb
     ```

---

## Playbook 6: Failed Deployment Rollback
- **Symptoms:** Errors occur immediately after deploying a new release; `/health/ready` or core views fail.
- **Immediate Actions:**
  1. Stop application pool:
     ```powershell
     Stop-WebAppPool -Name "FactoryXAppPool"
     ```
  2. Restore the previous known-good published application files from the backup archive:
     ```powershell
     Copy-Item -Path ".\backups\pre_deploy_archive\*" -Destination "C:\inetpub\wwwroot\FactoryX\" -Recurse -Force
     ```
  3. Restart application pool:
     ```powershell
     Start-WebAppPool -Name "FactoryXAppPool"
     ```
  4. Verify `/health/live` and `/health/ready`.

---

## Playbook 7: Failed Database Migration
- **Symptoms:** Database schema update fails halfway; EF Core reports migration mismatch.
- **Immediate Actions:**
  1. Do NOT manually drop production tables!
  2. Restore the pre-migration full database backup created automatically prior to migration execution.
  3. Check the migration script log in `scratch/` or EF output to isolate the offending constraint or column.

---

## Playbook 8: Backup Restoration Verification
- **Symptoms:** Routine disaster recovery drill or audit requirement.
- **Immediate Actions:**
  1. Run the safe sandbox restore validation script:
     ```powershell
     powershell -ExecutionPolicy Bypass -File scripts\backup\restore_database.ps1 -TestRestoreDbName MawlidSweetsErpDb_RestoreTest
     ```
  2. Confirm 100% pass on all critical tables, trial balance, and audit logs.

---

## Playbook 9: Post-Recovery Validation Checklist
Following any recovery or major incident:
1. [ ] Log in as Super Admin (`testadmin`).
2. [ ] Check `/SystemHealth` dashboard (all checks PASS).
3. [ ] Verify Trial Balance in Accounting (`/TrialBalance` - debits match credits).
4. [ ] Verify Inventory Stock Balance (`/Inventory/Stock` - non-empty, quantities intact).
5. [ ] Verify MES Batches (`/ProductionBatches` - active batches visible).
6. [ ] Verify Audit Trail (`/Audit` - past entries immutable and visible).
7. [ ] Confirm operations resumption to plant management.
