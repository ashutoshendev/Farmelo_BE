namespace Farmelo.API.Auditing;

public sealed class AuditEvent
{
    public int? ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string ActorEmail { get; set; } = string.Empty;
    public string ActorRole { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public int? TargetId { get; set; }
    public string TargetLabel { get; set; } = string.Empty;
    public string OldValue { get; set; } = "{}";
    public string NewValue { get; set; } = "{}";
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
