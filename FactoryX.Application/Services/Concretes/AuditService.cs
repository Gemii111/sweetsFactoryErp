using FactoryX.Application.DTOs;
using FactoryX.Application.Services.Abstracts;
using FactoryX.Domain.Entities;
using FactoryX.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace FactoryX.Application.Services.Concretes;

public class AuditService : IAuditService
{
    private readonly AppDbContext _context;

    public AuditService(AppDbContext context)
    {
        _context = context;
    }

    public async Task LogActivityAsync(
        int? userId,
        string username,
        string action,
        string module,
        string entityType,
        string entityId,
        string entityNumber,
        string description,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null)
    {
        var log = new AuditLog
        {
            Timestamp = DateTime.UtcNow,
            UserId = userId,
            Username = string.IsNullOrWhiteSpace(username) ? "System" : username,
            Action = action,
            Module = module,
            EntityType = entityType,
            EntityId = entityId,
            EntityNumber = entityNumber,
            Description = description,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString("N") : correlationId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogSecurityEventAsync(
        string eventType,
        int? userId,
        string username,
        string details,
        string? ipAddress = null,
        string severity = "Info")
    {
        var secEvent = new SecurityEvent
        {
            Timestamp = DateTime.UtcNow,
            EventType = eventType,
            UserId = userId,
            Username = string.IsNullOrWhiteSpace(username) ? "Anonymous" : username,
            Details = details,
            IpAddress = ipAddress,
            Severity = severity,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.SecurityEvents.Add(secEvent);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditLogItemDto>> GetAuditLogsAsync(AuditLogFilterDto filter)
    {
        var query = _context.AuditLogs.AsNoTracking().AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(a => a.Timestamp >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue)
            query = query.Where(a => a.Timestamp <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));
        if (filter.UserId.HasValue)
            query = query.Where(a => a.UserId == filter.UserId.Value);
        if (!string.IsNullOrWhiteSpace(filter.Module))
            query = query.Where(a => a.Module == filter.Module);
        if (!string.IsNullOrWhiteSpace(filter.Action))
            query = query.Where(a => a.Action == filter.Action);
        if (!string.IsNullOrWhiteSpace(filter.EntityType))
            query = query.Where(a => a.EntityType == filter.EntityType);
        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            query = query.Where(a => a.Description.Contains(filter.SearchTerm) ||
                                     a.EntityNumber.Contains(filter.SearchTerm) ||
                                     a.Username.Contains(filter.SearchTerm));
        }

        var items = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(a => new AuditLogItemDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                UserId = a.UserId,
                Username = a.Username,
                Action = a.Action,
                Module = a.Module,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                EntityNumber = a.EntityNumber,
                Description = a.Description,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                CorrelationId = a.CorrelationId
            })
            .ToListAsync();

        return items;
    }

    public async Task<AuditLogItemDto?> GetAuditLogDetailsAsync(int id)
    {
        var a = await _context.AuditLogs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (a == null) return null;

        return new AuditLogItemDto
        {
            Id = a.Id,
            Timestamp = a.Timestamp,
            UserId = a.UserId,
            Username = a.Username,
            Action = a.Action,
            Module = a.Module,
            EntityType = a.EntityType,
            EntityId = a.EntityId,
            EntityNumber = a.EntityNumber,
            Description = a.Description,
            OldValues = a.OldValues,
            NewValues = a.NewValues,
            IpAddress = a.IpAddress,
            CorrelationId = a.CorrelationId
        };
    }

    public async Task<List<SecurityEventItemDto>> GetSecurityEventsAsync(SecurityEventFilterDto filter)
    {
        var query = _context.SecurityEvents.AsNoTracking().AsQueryable();

        if (filter.FromDate.HasValue)
            query = query.Where(s => s.Timestamp >= filter.FromDate.Value.Date);
        if (filter.ToDate.HasValue)
            query = query.Where(s => s.Timestamp <= filter.ToDate.Value.Date.AddDays(1).AddTicks(-1));
        if (!string.IsNullOrWhiteSpace(filter.EventType))
            query = query.Where(s => s.EventType == filter.EventType);
        if (!string.IsNullOrWhiteSpace(filter.Severity))
            query = query.Where(s => s.Severity == filter.Severity);
        if (!string.IsNullOrWhiteSpace(filter.Username))
            query = query.Where(s => s.Username.Contains(filter.Username));

        var items = await query
            .OrderByDescending(s => s.Timestamp)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(s => new SecurityEventItemDto
            {
                Id = s.Id,
                Timestamp = s.Timestamp,
                EventType = s.EventType,
                UserId = s.UserId,
                Username = s.Username,
                IpAddress = s.IpAddress,
                Details = s.Details,
                Severity = s.Severity
            })
            .ToListAsync();

        return items;
    }

    public async Task<SecurityDashboardDto> GetSecurityDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;
        var users = await _context.Users.AsNoTracking().ToListAsync();

        var dto = new SecurityDashboardDto
        {
            TotalUsersCount = users.Count,
            ActiveUsersCount = users.Count(u => u.IsActive && (!u.LockedUntil.HasValue || u.LockedUntil.Value <= DateTime.UtcNow)),
            DisabledUsersCount = users.Count(u => !u.IsActive),
            LockedUsersCount = users.Count(u => u.LockedUntil.HasValue && u.LockedUntil.Value > DateTime.UtcNow),
            TotalRolesCount = await _context.Roles.CountAsync(r => r.IsActive),
            TotalPermissionsCount = await _context.Permissions.CountAsync(),

            TodaySuccessfulLoginsCount = await _context.SecurityEvents
                .CountAsync(s => s.Timestamp >= today && s.EventType == "LoginSuccess"),

            TodayFailedLoginsCount = await _context.SecurityEvents
                .CountAsync(s => s.Timestamp >= today && s.EventType == "LoginFailure"),

            TodayHighRiskActionsCount = await _context.AuditLogs
                .CountAsync(a => a.Timestamp >= today && (a.Action == "Reverse" || a.Action == "Delete" || a.Action == "Approve" || a.Action == "Release" || a.Action == "Adjust"))
        };

        dto.RecentSecurityEvents = await _context.SecurityEvents.AsNoTracking()
            .OrderByDescending(s => s.Timestamp)
            .Take(10)
            .Select(s => new SecurityEventItemDto
            {
                Id = s.Id,
                Timestamp = s.Timestamp,
                EventType = s.EventType,
                UserId = s.UserId,
                Username = s.Username,
                IpAddress = s.IpAddress,
                Details = s.Details,
                Severity = s.Severity
            })
            .ToListAsync();

        dto.RecentHighRiskAudits = await _context.AuditLogs.AsNoTracking()
            .Where(a => a.Action == "Reverse" || a.Action == "Delete" || a.Action == "Approve" || a.Action == "Release" || a.Action == "Adjust")
            .OrderByDescending(a => a.Timestamp)
            .Take(10)
            .Select(a => new AuditLogItemDto
            {
                Id = a.Id,
                Timestamp = a.Timestamp,
                Username = a.Username,
                Action = a.Action,
                Module = a.Module,
                EntityType = a.EntityType,
                EntityNumber = a.EntityNumber,
                Description = a.Description,
                IpAddress = a.IpAddress
            })
            .ToListAsync();

        return dto;
    }
}
