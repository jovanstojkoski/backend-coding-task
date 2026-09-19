using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Audit;
using Claims.Domain.Auditing;
using Claims.Domain.Core.Abstractions;
using Claims.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Claims.IntegrationTests.Infrastructure;

public sealed class AuditBackgroundServiceTests
{
    [Test]
    public async Task ExecuteAsync_PersistsClaimAndCoverAudits()
    {
        var claimAudit = ClaimAudit.Create(
            "claim-id",
            AuditHttpRequestTypes.Post,
            new DateTime(2026, 1, 1)).Value;

        var coverAudit = CoverAudit.Create(
            "cover-id",
            AuditHttpRequestTypes.Delete,
            new DateTime(2026, 1, 1)).Value;

        var queue = new TestAuditQueue(claimAudit, coverAudit);

        var databaseName = Guid.NewGuid().ToString();

        using var provider = new ServiceCollection()
            .AddDbContext<AuditContext>(options =>
                options.UseInMemoryDatabase(databaseName))
            .BuildServiceProvider();

        var service = new AuditBackgroundService(
            queue,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<AuditBackgroundService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await queue.Completed;
        await service.StopAsync(CancellationToken.None);

        await using var scope = provider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<AuditContext>();

        Assert.That(
            await context.Set<ClaimAudit>().ToListAsync(CancellationToken.None),
            Has.Count.EqualTo(1));
        Assert.That(
            await context.Set<CoverAudit>().ToListAsync(CancellationToken.None),
            Has.Count.EqualTo(1));
    }

    [Test]
    public async Task ExecuteAsync_ContinuesAfterUnsupportedRecord()
    {
        var coverAudit = CoverAudit.Create(
            "cover-id",
            AuditHttpRequestTypes.Post,
            new DateTime(2026, 1, 1)).Value;

        var queue = new TestAuditQueue(new UnknownAuditRecord(), coverAudit);

        var databaseName = Guid.NewGuid().ToString();

        using var provider = new ServiceCollection()
            .AddDbContext<AuditContext>(options =>
                options.UseInMemoryDatabase(databaseName))
            .BuildServiceProvider();

        var service = new AuditBackgroundService(
            queue,
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<AuditBackgroundService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await queue.Completed;
        await service.StopAsync(CancellationToken.None);

        await using var scope = provider.CreateAsyncScope();

        var context = scope.ServiceProvider.GetRequiredService<AuditContext>();

        Assert.That(
            await context.Set<CoverAudit>().ToListAsync(CancellationToken.None),
            Has.Count.EqualTo(1));
    }

    private sealed class UnknownAuditRecord : IAuditRecord;

    private sealed class TestAuditQueue(params IAuditRecord[] records) : IAuditQueue
    {
        private readonly TaskCompletionSource _completed =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task Completed => _completed.Task;

        public ValueTask EnqueueAsync(
            IAuditRecord auditRecord,
            CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public async IAsyncEnumerable<IAuditRecord> ReadAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            try
            {
                foreach (var record in records)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return record;
                }
            }
            finally
            {
                _completed.TrySetResult();
            }
        }
    }
}
