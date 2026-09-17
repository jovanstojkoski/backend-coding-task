using Claims.Domain.Core.Abstractions;

namespace Claims.Application.Abstractions;

public interface IAuditQueue
{
    void Enqueue(IAuditRecord auditRecord);

    IAsyncEnumerable<IAuditRecord> ReadAllAsync(
        CancellationToken cancellationToken);
}
