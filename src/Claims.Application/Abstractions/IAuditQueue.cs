using Claims.Domain.Core.Abstractions;

namespace Claims.Application.Abstractions;

public interface IAuditQueue
{
    ValueTask EnqueueAsync(
        IAuditRecord auditRecord,
        CancellationToken cancellationToken);

    IAsyncEnumerable<IAuditRecord> ReadAllAsync(
        CancellationToken cancellationToken);
}
