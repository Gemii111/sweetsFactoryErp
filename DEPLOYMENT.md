# FactoryX Mawlid Sweets ERP — Production Deployment Guide

## 1. Overview & Architecture
The **FactoryX Mawlid Sweets ERP** is an on-premises, enterprise food-manufacturing ERP system designed for factory LAN environments. It operates in a Windows Server environment powered by **Internet Information Services (IIS)** and **Microsoft SQL Server**.

### Technology Baseline:
- **Operating System:** Windows Server 2019 / 2022 / Windows 11 Enterprise (Factory Host)
- **Web Server:** Internet Information Services (IIS) 10.0+
- **Application Runtime:** ASP.NET Core 9.0 (Hosting Bundle installed)
- **Database Engine:** Microsoft SQL Server 2019 / 2022 (Standard or Enterprise Edition)
- **Architecture Model:** Reverse Proxy / In-Process ASP.NET Core Module (`AspNetCoreModuleV2`)

---

## 2. Server Prerequisites & Installation

### A. Windows Features & Roles
Install IIS with required modules via PowerShell (Run as Administrator):
```powershell
Enable-WindowsOptionalFeature -Online -FeatureName `
    IIS-WebServerRole, `
    IIS-WebServer, `
    IIS-CommonHttpFeatures, `
    IIS-HttpErrors, `
    IIS-HttpRedirect, `
    IIS-StaticContent, `
    IIS-DefaultDocument, `
    IIS-DirectoryBrowsing, `
    IIS-ApplicationDevelopment, `
    IIS-HealthAndDiagnostics, `
    IIS-HttpLogging, `
    IIS-Security, `
    IIS-RequestFiltering, `
    IIS-Performance, `
    IIS-HttpCompressionStatic
```

### B. .NET 9.0 Hosting Bundle
1. Download and install the **.NET 9.0 Hosting Bundle** (includes .NET 9.0 Runtime and IIS ASP.NET Core Module).
2. Verify installation:
   ```powershell
   dotnet --info
   ```
3. Restart IIS:
   ```powershell
   net stop was /y
   net start w3svc
   ```

---

## 3. IIS Application Pool & Website Configuration

### A. Dedicated Application Pool
- **Name:** `FactoryXAppPool`
- **.NET CLR Version:** `No Managed Code` (since ASP.NET Core runs out-of-process or via in-process ANCM)
- **Managed Pipeline Mode:** `Integrated`
- **Identity:** `ApplicationPoolIdentity` (or dedicated factory service account `SVC_FACTORYX`)
- **Start Mode:** `AlwaysRunning`
- **Idle Time-out (minutes):** `0` (prevents app unloading during shifts)
- **Recycling:** Scheduled during daily maintenance window (e.g. `03:00 AM`)

### B. IIS Website Setup
1. Point Physical Path to the deployed directory: `C:\inetpub\wwwroot\FactoryX\`.
2. Add bindings:
   - **HTTP:** Port `80` or `5265` bound to Factory LAN IP (e.g. `192.168.1.100`).
   - **HTTPS:** Port `443` with factory internal SSL certificate (where applicable).

---

## 4. File System Permissions

Assign the following minimal required permissions:
- **Application Root (`C:\inetpub\wwwroot\FactoryX\`):**
  - `IIS_IUSRS`: Read & Execute
  - `SVC_FACTORYX`: Read & Execute
- **Logs Directory (`C:\inetpub\wwwroot\FactoryX\logs\`):**
  - `IIS_IUSRS` / `ApplicationPoolIdentity`: Modify, Write
- **Backups Directory (`D:\MawlidERP\Backups\`):**
  - `SQLSERVER_SERVICE_ACCOUNT`: Full Control
  - `Administrators`: Full Control
  - Application identity has NO direct delete access to older backups.

---

## 5. Environment Separation & Configuration Safety

Configure the environment variable on the server:
- System Environment Variable: `ASPNETCORE_ENVIRONMENT = Production`
- Or inside IIS `web.config`:
```xml
<aspNetCore processPath="dotnet" arguments=".\FactoryX.Web.dll" stdoutLogEnabled="true" stdoutLogFile=".\logs\stdout" hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="ASPNETCORE_ENVIRONMENT" value="Production" />
  </environmentVariables>
</aspNetCore>
```

### Connection String Security:
In production, connection strings should NEVER be hardcoded in git. Configure either:
1. Windows Environment Variable: `ConnectionStrings__DefaultConnection`
2. Encrypted `appsettings.Production.json` managed exclusively by System Administrators.

---

## 6. Automated Deployment Helper

To deploy or update a build, run:
```powershell
powershell -ExecutionPolicy Bypass -File scripts\deploy\deploy.ps1 -Environment Production
```

The script automatically:
1. Validates .NET 9 prerequisites.
2. Performs a verified pre-deployment database backup.
3. Compiles and publishes release assets.
4. Ensures migrations are non-destructively verified.
5. Performs smoke checks against `/health/live` and `/health/ready`.
