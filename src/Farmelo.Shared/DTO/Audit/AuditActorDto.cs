namespace Farmelo.Shared.DTO.Audit;

public sealed class AuditActorDto
{
    public int ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string ActorRole { get; set; } = string.Empty;
}
