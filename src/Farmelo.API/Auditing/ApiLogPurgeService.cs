using Farmelo.Data.Write.EFContext;
using Microsoft.EntityFrameworkCore;

namespace Farmelo.API.Auditing;

public sealed class ApiLogPurgeService : BackgroundService
{
    private static readonly TimeSpan InitialDelay = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan RunInterval = TimeSpan.FromHours(24);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ApiLogPurgeService> _logger;

    public ApiLogPurgeService(IServiceScopeFactory scopeFactory, ILogger<ApiLogPurgeService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DelayAsync(InitialDelay, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            await PurgeAsync(stoppingToken);
            await DelayAsync(RunInterval, stoppingToken);
        }
    }

    private async Task PurgeAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<FarmeloDbContext>();
            await dbContext.Database.ExecuteSqlRawAsync(
                "DELETE FROM ApiLogs WHERE [Timestamp] < DATEADD(day, -30, SYSUTCDATETIME())",
                cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to purge old API logs.");
        }
    }

    private static async Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }
}
