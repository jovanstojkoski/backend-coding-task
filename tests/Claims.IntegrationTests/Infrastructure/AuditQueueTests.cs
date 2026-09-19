using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Audit;
using Claims.Domain.Auditing;
using Claims.Domain.Core.Abstractions;
using Claims.Infrastructure.Data.Queues;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Claims.IntegrationTests.Infrastructure;

public sealed class AuditQueueTests
{
    [Test]
    public async Task EnqueueAsync_MakesRecordAvailableToReader()
    {
        var queue = new AuditQueue(Options.Create(new AuditQueueOptions { Capacity = 1 }));

        var audit = ClaimAudit.Create(
            "claim-id",
            AuditHttpRequestTypes.Post,
            new DateTime(2026, 1, 1)).Value;

        var readTask = ReadOneAsync(queue);

        await queue.EnqueueAsync(audit, CancellationToken.None);

        var result = await readTask;

        Assert.That(result, Is.SameAs(audit));
    }

    [Test]
    public async Task EnqueueAsync_WaitsWhenQueueIsFullUntilCancelled()
    {
        var queue = new AuditQueue(Options.Create(new AuditQueueOptions { Capacity = 1 }));
        var firstAudit = CreateAudit("first");
        var secondAudit = CreateAudit("second");

        await queue.EnqueueAsync(firstAudit, CancellationToken.None);

        using var cancellationSource = new CancellationTokenSource();
        var secondWrite = queue.EnqueueAsync(
            secondAudit,
            cancellationSource.Token).AsTask();

        var completedTask = await Task.WhenAny(
            secondWrite,
            Task.Delay(TimeSpan.FromMilliseconds(100)));

        Assert.That(completedTask, Is.Not.SameAs(secondWrite));

        cancellationSource.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(
            async () => await secondWrite);
    }

    [Test]
    public void Constructor_RejectsNonPositiveCapacity()
    {
        Assert.That(
            () => new AuditQueue(
                Options.Create(new AuditQueueOptions { Capacity = 0 })),
            Throws.TypeOf<ArgumentOutOfRangeException>());
    }

    private static ClaimAudit CreateAudit(string claimId)
    {
        return ClaimAudit.Create(
            claimId,
            AuditHttpRequestTypes.Post,
            new DateTime(2026, 1, 1)).Value;
    }

    private static async Task<IAuditRecord> ReadOneAsync(
        IAuditQueue queue)
    {
        await foreach (var auditRecord in queue.ReadAllAsync(CancellationToken.None))
        {
            return auditRecord;
        }

        throw new InvalidOperationException("The queue ended before returning a record.");
    }
}
