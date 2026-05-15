using System.Threading.Channels;

namespace Farmelo.API.Auditing;

public sealed class AuditEventQueue : IAuditEventQueue
{
    private readonly Channel<AuditEvent> _channel = Channel.CreateBounded<AuditEvent>(
        new BoundedChannelOptions(10_000)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

    public bool TryEnqueue(AuditEvent auditEvent)
        => _channel.Writer.TryWrite(auditEvent);

    public ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken)
        => _channel.Reader.WaitToReadAsync(cancellationToken);

    public bool TryDequeue(out AuditEvent auditEvent)
    {
        var read = _channel.Reader.TryRead(out var item);
        auditEvent = item!;
        return read;
    }
}
