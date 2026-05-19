namespace Farmelo.Data.Write.Abstractions;

public interface IAuditContext
{
    string? Endpoint { get; }
    string? IpAddress { get; }
}
