using FactoryX.Application.DTOs;

namespace FactoryX.Application.Services.Abstracts;

public interface IAuditService
{
    Task LogActivityAsync(int? userId, string username, string action, string module, string entityType, string entityId, string entityNumber, string description, string? oldValues = null, string? newValues = null, string? ipAddress = null, string? userAgent = null, string? correlationId = null);
    Task LogSecurityEventAsync(string eventType, int? userId, string username, string details, string? ipAddress = null, string severity = "Info");
    Task<List<AuditLogItemDto>> GetAuditLogsAsync(AuditLogFilterDto filter);
    Task<AuditLogItemDto?> GetAuditLogDetailsAsync(int id);
    Task<List<SecurityEventItemDto>> GetSecurityEventsAsync(SecurityEventFilterDto filter);
    Task<SecurityDashboardDto> GetSecurityDashboardAsync();
}
