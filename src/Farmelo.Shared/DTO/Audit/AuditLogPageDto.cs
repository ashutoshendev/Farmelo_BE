namespace Farmelo.Shared.DTO.Audit;

public sealed class AuditLogPageDto
{
    public IReadOnlyList<AuditLogDto> Items { get; set; } = Array.Empty<AuditLogDto>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
