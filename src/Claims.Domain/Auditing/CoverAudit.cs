using Claims.Domain.Core.Abstractions;
using Claims.Domain.Core.Primitives;

namespace Claims.Domain.Auditing;

public class CoverAudit : IAuditRecord
{
    public int Id { get; private set; }

    public string CoverId { get; private set; } = null!;

    public DateTime Created { get; private set; }

    public string HttpRequestType { get; private set; } = null!;

    private CoverAudit()
    {
    }

    public static Result<CoverAudit> Create(
        string coverId,
        string httpRequestType,
        DateTime created)
    {
        if (string.IsNullOrWhiteSpace(coverId))
        {
            return Result.Failure<CoverAudit>("Cover ID is required.");
        }

        if (string.IsNullOrWhiteSpace(httpRequestType))
        {
            return Result.Failure<CoverAudit>("HTTP request type is required.");
        }

        return new CoverAudit
        {
            CoverId = coverId,
            Created = created,
            HttpRequestType = httpRequestType
        };
    }
}
