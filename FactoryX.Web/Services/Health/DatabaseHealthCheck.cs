using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FactoryX.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FactoryX.Web.Services.Health;

/// <summary>
/// Verifies SQL Server database connectivity without exposing sensitive credentials or connection strings.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly AppDbContext _context;

    public DatabaseHealthCheck(AppDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            // Lightweight connectivity ping
            bool canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            stopwatch.Stop();

            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy("تعذر الاتصال بقاعدة بيانات SQL Server المركزية. الرجاء التحقق من تشغيل خدمة قاعدة البيانات والشبكة.");
            }

            var data = new System.Collections.Generic.Dictionary<string, object>
            {
                { "LatencyMs", stopwatch.ElapsedMilliseconds },
                { "DatabaseEngine", "Microsoft SQL Server" },
                { "Status", "Connected" }
            };

            if (stopwatch.ElapsedMilliseconds > 3000)
            {
                return HealthCheckResult.Degraded($"اتصال قاعدة البيانات بطيء استغرق ({stopwatch.ElapsedMilliseconds}ms)", data: data);
            }

            return HealthCheckResult.Healthy($"اتصال قاعدة البيانات نشط وسريع ({stopwatch.ElapsedMilliseconds}ms)", data: data);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            // Sanitize message: never reveal internal passwords or connection details
            return HealthCheckResult.Unhealthy($"فشل اختبار اتصال قاعدة البيانات: {ex.GetType().Name}", ex);
        }
    }
}
