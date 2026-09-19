namespace Claims.Infrastructure.Data.Queues;

public sealed class AuditQueueOptions
{
    public const string SectionName = "AuditQueue";

    public int Capacity { get; init; } = 1_000;
}
