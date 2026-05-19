using Farmelo.API.Auditing;
using System.Diagnostics;
using System.Security.Claims;

namespace Farmelo.API.Middleware;

public sealed class ApiLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public ApiLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IApiLogQueue apiLogQueue)
    {
        var stopwatch = Stopwatch.StartNew();
        var statusCode = StatusCodes.Status500InternalServerError;

        try
        {
            await _next(context);
            statusCode = context.Response.StatusCode;
        }
        catch
        {
            statusCode = StatusCodes.Status500InternalServerError;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
            {
                apiLogQueue.TryEnqueue(new ApiLogEvent
                {
                    Method = Limit(context.Request.Method, 12),
                    Endpoint = Limit(context.Request.Path.Value ?? string.Empty, 500),
                    StatusCode = statusCode,
                    ResponseTimeMs = stopwatch.ElapsedMilliseconds,
                    ActorId = GetActorId(context),
                    Timestamp = DateTime.UtcNow
                });
            }
        }
    }

    private static int? GetActorId(HttpContext context)
    {
        var value = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var actorId) ? actorId : null;
    }

    private static string Limit(string value, int maxLength)
        => value.Length <= maxLength ? value : value[..maxLength];
}
