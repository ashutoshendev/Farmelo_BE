using Farmelo.Data.Write.EFContext;
using Farmelo.Data.Write.Entities;
using Microsoft.EntityFrameworkCore;

namespace Farmelo.API.Auditing;

public sealed class AuditLogBackgroundService : BackgroundService
{
    private const int BatchSize = 100;

    private readonly IAuditEventQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AuditLogBackgroundService> _logger;

    public AuditLogBackgroundService(
        IAuditEventQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<AuditLogBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<AuditEvent>(BatchSize);

        try
        {
            while (await _queue.WaitToReadAsync(stoppingToken))
            {
                while (batch.Count < BatchSize && _queue.TryDequeue(out var auditEvent))
                {
                    batch.Add(auditEvent);
                }

                await FlushAsync(batch, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutdown; drain remaining audit events below.
        }
        finally
        {
            while (_queue.TryDequeue(out var auditEvent))
            {
                batch.Add(auditEvent);

                if (batch.Count >= BatchSize)
                {
                    await FlushAsync(batch, CancellationToken.None);
                }
            }

            await FlushAsync(batch, CancellationToken.None);
        }
    }

    private async Task FlushAsync(List<AuditEvent> batch, CancellationToken cancellationToken)
    {
        if (batch.Count == 0)
        {
            return;
        }

        var entities = batch.Select(ToEntity).ToArray();
        batch.Clear();

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FarmeloDbContext>();
            await dbContext.AuditLogs.AddRangeAsync(entities, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to persist audit log batch.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create audit log persistence scope.");
        }
    }

    private static AuditLog ToEntity(AuditEvent auditEvent)
        => new()
        {
            UserId = auditEvent.ActorId,
            UserFullName = auditEvent.ActorName,
            UserEmail = auditEvent.ActorEmail,
            UserRole = auditEvent.ActorRole,
            EventType = auditEvent.ActionType,
            Module = auditEvent.Module,
            Action = auditEvent.ActionType,
            HttpMethod = string.Empty,
            Path = string.Empty,
            StatusCode = null,
            DurationMs = null,
            IpAddress = auditEvent.IpAddress,
            UserAgent = string.Empty,
            Details = string.Empty,
            TargetId = auditEvent.TargetId,
            TargetLabel = auditEvent.TargetLabel,
            OldValue = auditEvent.OldValue,
            NewValue = auditEvent.NewValue,
            OccurredOn = auditEvent.Timestamp,
            CreatedBy = auditEvent.ActorEmail,
            CreatedOn = auditEvent.Timestamp
        };
}
