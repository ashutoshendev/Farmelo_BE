namespace Farmelo.API.Auditing;

public interface IAuditEventQueue
{
    bool TryEnqueue(AuditEvent auditEvent);
    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken);
    bool TryDequeue(out AuditEvent auditEvent);
}
