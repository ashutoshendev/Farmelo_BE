using System.Threading.Channels;

namespace Farmelo.API.Auditing;

public sealed class ApiLogQueue : IApiLogQueue
{
    private readonly Channel<ApiLogEvent> _channel = Channel.CreateBounded<ApiLogEvent>(
        new BoundedChannelOptions(20_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

    public bool TryEnqueue(ApiLogEvent apiLogEvent)
        => _channel.Writer.TryWrite(apiLogEvent);

    public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken)
        => _channel.Reader.WaitToReadAsync(cancellationToken);

    public bool TryDequeue(out ApiLogEvent apiLogEvent)
    {
        var read = _channel.Reader.TryRead(out var item);
        apiLogEvent = item!;
        return read;
    }
}
