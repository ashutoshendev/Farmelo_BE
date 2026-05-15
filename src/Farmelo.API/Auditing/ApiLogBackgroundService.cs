using Farmelo.Data.Write.EFContext;
using Farmelo.Data.Write.Entities;
using Microsoft.EntityFrameworkCore;

namespace Farmelo.API.Auditing;

public sealed class ApiLogBackgroundService : BackgroundService
{
    private const int BatchSize = 200;

    private readonly IApiLogQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ApiLogBackgroundService> _logger;

    public ApiLogBackgroundService(
        IApiLogQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ApiLogBackgroundService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var batch = new List<ApiLogEvent>(BatchSize);

        try
        {
            while (await _queue.WaitToReadAsync(stoppingToken))
            {
                while (batch.Count < BatchSize && _queue.TryDequeue(out var apiLogEvent))
                {
                    batch.Add(apiLogEvent);
                }

                await FlushAsync(batch, stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host shutdown; drain remaining API log events below.
        }
        finally
        {
            while (_queue.TryDequeue(out var apiLogEvent))
            {
                batch.Add(apiLogEvent);

                if (batch.Count >= BatchSize)
                {
                    await FlushAsync(batch, CancellationToken.None);
                }
            }

            await FlushAsync(batch, CancellationToken.None);
        }
    }

    private async Task FlushAsync(List<ApiLogEvent> batch, CancellationToken cancellationToken)
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
            await dbContext.ApiLogs.AddRangeAsync(entities, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Failed to persist API log batch.");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create API log persistence scope.");
        }
    }

    private static ApiLog ToEntity(ApiLogEvent apiLogEvent)
        => new()
        {
            Method = apiLogEvent.Method,
            Endpoint = apiLogEvent.Endpoint,
            StatusCode = apiLogEvent.StatusCode,
            ResponseTimeMs = apiLogEvent.ResponseTimeMs,
            ActorId = apiLogEvent.ActorId,
            Timestamp = apiLogEvent.Timestamp
        };
}
