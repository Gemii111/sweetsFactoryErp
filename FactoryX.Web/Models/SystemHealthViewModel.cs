using System;
using System.Collections.Generic;

namespace FactoryX.Web.Models;

public class SystemHealthViewModel
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string EnvironmentName { get; set; } = string.Empty;
    public string Edition { get; set; } = string.Empty;
    public DateTime ServerTime { get; set; }
    public DateTime UtcTime { get; set; }
    public string ServerTimeZone { get; set; } = string.Empty;
    public TimeSpan Uptime { get; set; }
    public double MemoryUsageMb { get; set; }

    // Database Status
    public bool DatabaseConnected { get; set; }
    public long DatabaseLatencyMs { get; set; }
    public string DatabaseEngine { get; set; } = "Microsoft SQL Server";
    public int AppliedMigrationsCount { get; set; }
    public int PendingMigrationsCount { get; set; }
    public string LatestMigration { get; set; } = string.Empty;

    // Disk Space
    public List<DiskDriveHealth> Disks { get; set; } = new();

    // Backup Status
    public bool BackupDirectoryConfigured { get; set; }
    public bool BackupDirectoryExists { get; set; }
    public string BackupDirectoryPath { get; set; } = string.Empty;
    public string? LatestBackupFileName { get; set; }
    public DateTime? LatestBackupTime { get; set; }
    public double? LatestBackupSizeMb { get; set; }
    public double? LatestBackupAgeHours { get; set; }
    public string BackupStatusBadge { get; set; } = "WARNING"; // PASS, WARNING, FAIL

    // Readiness Checks Table
    public List<ReadinessCheckItem> ReadinessChecks { get; set; } = new();

    public bool IsProductionReady => ReadinessChecks.TrueForAll(c => c.Status != "FAIL");
}

public class DiskDriveHealth
{
    public string DriveName { get; set; } = string.Empty;
    public string DriveLabel { get; set; } = string.Empty;
    public double TotalSizeGb { get; set; }
    public double FreeSpaceGb { get; set; }
    public double FreeSpacePercent { get; set; }
    public string Status { get; set; } = "PASS"; // PASS, WARNING, FAIL
    public string Role { get; set; } = "Application & OS";
}

public class ReadinessCheckItem
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = "PASS"; // PASS, WARNING, FAIL
    public string Details { get; set; } = string.Empty;
}
