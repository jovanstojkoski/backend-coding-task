using Claims.Application.Abstractions;
using Claims.Domain.Auditing;
using Claims.Domain.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Claims.Infrastructure.Data;

public sealed class AuditBackgroundService(
    IAuditQueue auditQueue,
    IServiceScopeFactory scopeFactory,
    ILogger<AuditBackgroundService> logger) : BackgroundService
{
    private readonly IAuditQueue _auditQueue = auditQueue;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;
    private readonly ILogger<AuditBackgroundService> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await foreach (var auditRecord in _auditQueue.ReadAllAsync(stoppingToken))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var auditContext = scope.ServiceProvider.GetRequiredService<AuditContext>();

                    AddAuditRecord(auditContext, auditRecord);
                    await auditContext.SaveChangesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to persist an audit record.");
                }
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private static void AddAuditRecord(AuditContext context, IAuditRecord auditRecord)
    {
        switch (auditRecord)
        {
            case ClaimAudit claimAudit:
                context.Set<ClaimAudit>().Add(claimAudit);
                break;
            case CoverAudit coverAudit:
                context.Set<CoverAudit>().Add(coverAudit);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(auditRecord));
        }
    }
}
