namespace Farmelo.Shared.DTO.Audit;

public sealed class TrackAuditEventRequestDto
{
    public string Module { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Details { get; set; }
}
