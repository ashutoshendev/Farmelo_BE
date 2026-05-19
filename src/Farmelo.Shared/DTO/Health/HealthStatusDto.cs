namespace Farmelo.Shared.DTO.Health;

public sealed record HealthStatusDto(
    string Status,
    string Service,
    DateTimeOffset CheckedAtUtc);
