namespace Claims.Infrastructure.Options;

public sealed class ConnectionStringsOptions
{
    public const string SectionName = "ConnectionStrings";

    public string AuditDatabase { get; set; } = string.Empty;
}
