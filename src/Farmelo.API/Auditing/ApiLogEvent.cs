namespace Farmelo.API.Auditing;

public sealed class ApiLogEvent
{
    public string Method { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public int? ActorId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
