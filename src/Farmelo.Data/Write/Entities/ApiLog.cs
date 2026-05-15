namespace Farmelo.Data.Write.Entities;

public sealed class ApiLog
{
    public long Id { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public int? ActorId { get; set; }
    public DateTime Timestamp { get; set; }
}
