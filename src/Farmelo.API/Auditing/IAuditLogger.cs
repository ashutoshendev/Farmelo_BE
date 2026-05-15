namespace Farmelo.API.Auditing;

public interface IAuditLogger
{
    void Log(AuditEvent auditEvent);
    void LogAuthenticationEvent(string actionType, int targetId, string targetLabel);
}
