namespace Farmelo.API.Auditing;

public interface IApiLogQueue
{
    bool TryEnqueue(ApiLogEvent apiLogEvent);
    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken);
    bool TryDequeue(out ApiLogEvent apiLogEvent);
}
