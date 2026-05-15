using Farmelo.Shared.CommonHelper;
using System.Security.Claims;

namespace Farmelo.API.Auditing;

public sealed class AuditLogger : IAuditLogger
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuditEventQueue _queue;

    public AuditLogger(IHttpContextAccessor httpContextAccessor, IAuditEventQueue queue)
    {
        _httpContextAccessor = httpContextAccessor;
        _queue = queue;
    }

    public void Log(AuditEvent auditEvent)
    {
        if (!IsAdminOrOwner())
        {
            return;
        }

        auditEvent.ActorId ??= GetActorId();
        auditEvent.ActorName = Limit(FirstNotBlank(auditEvent.ActorName, GetClaim(ClaimTypes.Name)), 150);
        auditEvent.ActorEmail = Limit(FirstNotBlank(auditEvent.ActorEmail, GetClaim(ClaimTypes.Email)), 256);
        auditEvent.ActorRole = Limit(FirstNotBlank(auditEvent.ActorRole, GetClaim(ClaimTypes.Role)), 30);
        auditEvent.ActionType = Limit(auditEvent.ActionType, 40);
        auditEvent.Module = Limit(auditEvent.Module, 120);
        auditEvent.TargetLabel = Limit(auditEvent.TargetLabel, 200);
        auditEvent.IpAddress = Limit(GetIpAddress(), 80);
        auditEvent.Timestamp = DateTime.UtcNow;

        _queue.TryEnqueue(auditEvent);
    }

    public void LogAuthenticationEvent(string actionType, int targetId, string targetLabel)
        => Log(new AuditEvent
        {
            ActorId = targetId,
            ActionType = actionType,
            Module = "Authentication",
            TargetId = targetId,
            TargetLabel = targetLabel,
            OldValue = "{}",
            NewValue = "{}"
        });

    private bool IsAdminOrOwner()
    {
        var role = GetClaim(ClaimTypes.Role);
        return string.Equals(role, AppConstants.Roles.Admin, StringComparison.Ordinal)
               || string.Equals(role, AppConstants.Roles.Owner, StringComparison.Ordinal);
    }

    private int? GetActorId()
    {
        var value = GetClaim(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var actorId) ? actorId : null;
    }

    private string GetIpAddress()
    {
        var context = _httpContextAccessor.HttpContext;
        return context?.Request.Headers["X-Forwarded-For"].FirstOrDefault()
               ?? context?.Connection.RemoteIpAddress?.ToString()
               ?? string.Empty;
    }

    private string GetClaim(string claimType)
        => _httpContextAccessor.HttpContext?.User.FindFirstValue(claimType) ?? string.Empty;

    private static string FirstNotBlank(string first, string second)
        => string.IsNullOrWhiteSpace(first) ? second : first;

    private static string Limit(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
