using System;

namespace FactoryX.Application.Common;

/// <summary>
/// Single source of truth for FactoryX Mawlid Sweets ERP application versioning.
/// </summary>
public static class SystemVersionInfo
{
    public const string Version = "v1.0.0";
    public const string ReleaseName = "FactoryX Mawlid Sweets ERP";
    public const string Edition = "Production Edition (Factory LAN)";
    public const string Codename = "Mawlid Sweets Operational Excellence";
    public const string ReleaseDate = "2026-09-03";
    public const string TargetPlatform = "Windows Server / IIS / Microsoft SQL Server";

    public static string FullVersionString => $"{ReleaseName} {Version} ({Edition})";
}
