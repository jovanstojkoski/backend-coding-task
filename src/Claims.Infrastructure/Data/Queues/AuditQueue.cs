using System.Threading.Channels;
using Claims.Application.Abstractions;
using Claims.Domain.Core.Abstractions;
using Microsoft.Extensions.Options;

namespace Claims.Infrastructure.Data.Queues;

public sealed class AuditQueue : IAuditQueue
{
    private readonly Channel<IAuditRecord> _queue;

    public AuditQueue(IOptions<AuditQueueOptions> options)
    {
        var capacity = options.Value.Capacity;

        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(AuditQueueOptions.Capacity),
                "Audit queue capacity must be greater than zero.");
        }

        _queue = Channel.CreateBounded<IAuditRecord>(
            new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            });
    }

    public ValueTask EnqueueAsync(
        IAuditRecord auditRecord,
        CancellationToken cancellationToken)
    {
        return _queue.Writer.WriteAsync(auditRecord, cancellationToken);
    }

    public IAsyncEnumerable<IAuditRecord> ReadAllAsync(CancellationToken cancellationToken)
        => _queue.Reader.ReadAllAsync(cancellationToken);
}
