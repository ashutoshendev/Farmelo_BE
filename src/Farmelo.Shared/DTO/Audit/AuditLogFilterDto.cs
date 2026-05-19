namespace Farmelo.Shared.DTO.Audit;

public sealed class AuditLogFilterDto
{
    public string? Search { get; set; }
    public string? Module { get; set; }
    public string? ActionType { get; set; }
    public int? ActorId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
