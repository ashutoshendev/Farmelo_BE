using Farmelo.Data.Write.Entities.Abstractions;

namespace Farmelo.Data.Write.Entities;

public sealed class AuditLog : BaseEntity
{
    public long Id { get; set; }
    public int? UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public int? StatusCode { get; set; }
    public long? DurationMs { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public int? TargetId { get; set; }
    public string? TargetLabel { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public DateTime OccurredOn { get; set; }
}
