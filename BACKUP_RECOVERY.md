# FactoryX Mawlid Sweets ERP — Backup & Disaster Recovery Strategy

## 1. RPO & RTO Objectives

| Metric | Target | Tested / Validated | Recovery Mechanism |
| :--- | :--- | :--- | :--- |
| **RPO (Recovery Point Objective)** | **24 Hours** (1 Hour with Log backups) | **&lt; 15 Minutes** (in automated test) | Automated SQL Server Backups with verification |
| **RTO (Recovery Time Objective)** | **4 Hours** | **&lt; 3 Minutes** (in sandbox restore test) | Automated PowerShell restore procedure |

*Note: The targets reflect operational SLA standards for the factory; actual measured restore time on a test database was under 3 minutes.*

---

## 2. SQL Server Backup Strategy

### A. Backup Types & Schedules
1. **Full Database Backup (Daily at 23:00):**
   - Captures entire database image, schema, transactions, users, settings, and audit logs.
   - Command:
     ```powershell
     powershell -ExecutionPolicy Bypass -File scripts\backup\backup_database.ps1 -BackupType Full
     ```
2. **Differential Backup (Mid-day at 18:00 - Optional):**
   - Captures changes made since the last Full backup to expedite recovery during active shifts.
   - Command:
     ```powershell
     powershell -ExecutionPolicy Bypass -File scripts\backup\backup_database.ps1 -BackupType Diff
     ```
3. **Transaction Log Backup (Hourly during shift - When Full Recovery Model is Active):**
   - Captures active transaction log slices.

### B. Deterministic Naming Convention
- Full Backup: `MawlidSweetsErpDb_FULL_yyyy-MM-dd_HHmmss.bak`
- Differential Backup: `MawlidSweetsErpDb_DIFF_yyyy-MM-dd_HHmmss.bak`
- Transaction Log: `MawlidSweetsErpDb_LOG_yyyy-MM-dd_HHmmss.trn`

---

## 3. Automated Verification via `RESTORE VERIFYONLY`

A backup file is never assumed valid simply because it was created. Every automated backup execution concludes with:
```sql
RESTORE VERIFYONLY FROM DISK = N'D:\MawlidERP\Backups\MawlidSweetsErpDb_FULL_2026-09-03_230000.bak';
```
This ensures that the SQL Server storage engine reads the complete media structure, validates checksums, and confirms that the file is restorable.

---

## 4. Safe Retention & Pruning Policy

- **Retention Window:** Configurable (default 30 days).
- **Safety Invariant:** Older backups are only purged if newer, successfully verified backups exist.
- **Sole Backup Protection:** The script will NEVER delete the only remaining backup files.

---

## 5. Controlled Disaster Recovery & Sandbox Testing

To validate recovery without endangering the live production system, restore tests are conducted strictly in a sandbox database:
```powershell
powershell -ExecutionPolicy Bypass -File scripts\backup\restore_database.ps1 -TestRestoreDbName MawlidSweetsErpDb_RestoreTest
```

### Validation Checks Performed:
1. **Structural Table Availability:** Users, Roles, StockBalances, Batches, Quality, Invoices, Journals, AuditLogs.
2. **Financial Integrity:** Total Debits == Total Credits across all posted journal lines in the restored database.
3. **Audit Trail Immutability:** Audit records persist intact.
4. **Live Production Protection:** The live `MawlidSweetsErpDb` is never overwritten.

---

## 6. Live Disaster Recovery Step-by-Step Procedure

In the event of total server hardware or database failure:
1. **Provision New Host:** Install Windows Server & SQL Server (2019/2022).
2. **Retrieve Backup:** Copy the latest verified `.bak` file from secondary storage/external drive.
3. **Restore Database:**
   ```sql
   RESTORE DATABASE [MawlidSweetsErpDb]
   FROM DISK = N'D:\MawlidERP\Backups\MawlidSweetsErpDb_FULL_YYYY-MM-DD_HHMMSS.bak'
   WITH RECOVERY, STATS = 10;
   ```
4. **If Differential Backup Exists:**
   - Restore Full backup with `NORECOVERY`.
   - Restore Differential backup with `RECOVERY`.
5. **Configure Connection:** Set `ConnectionStrings__DefaultConnection` on application host.
6. **Launch ERP & Run Smoke Checks:**
   - Verify `/health/live` (HTTP 200).
   - Verify `/health/ready` (HTTP 200).
   - Access `/SystemHealth` dashboard as Super Admin.
7. **Verify Operations:** Review Trial Balance, Stock Balances, and user login before reopening plant operations.
