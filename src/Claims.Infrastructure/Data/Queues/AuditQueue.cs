using System.Threading.Channels;
using Claims.Application.Abstractions;
using Claims.Domain.Core.Abstractions;

namespace Claims.Infrastructure.Data.Queues;

public sealed class AuditQueue : IAuditQueue
{
    private readonly Channel<IAuditRecord> _queue =
        Channel.CreateUnbounded<IAuditRecord>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

    public void Enqueue(IAuditRecord auditRecord)
    {
        ArgumentNullException.ThrowIfNull(auditRecord);

        if (!_queue.Writer.TryWrite(auditRecord))
        {
            throw new InvalidOperationException("The audit queue is not accepting new records.");
        }
    }

    public IAsyncEnumerable<IAuditRecord> ReadAllAsync(CancellationToken cancellationToken)
        => _queue.Reader.ReadAllAsync(cancellationToken);
}
