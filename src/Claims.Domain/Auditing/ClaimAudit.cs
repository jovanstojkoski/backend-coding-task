using Claims.Domain.Core.Abstractions;
using Claims.Domain.Core.Primitives;

namespace Claims.Domain.Auditing;

public class ClaimAudit : IAuditRecord
{
    public int Id { get; private set; }

    public string ClaimId { get; private set; } = null!;

    public DateTime Created { get; private set; }

    public string HttpRequestType { get; private set; } = null!;

    private ClaimAudit()
    {
    }

    public static Result<ClaimAudit> Create(
        string claimId,
        string httpRequestType,
        DateTime created)
    {
        if (string.IsNullOrWhiteSpace(claimId))
        {
            return Result.Failure<ClaimAudit>("Claim ID is required.");
        }

        if (string.IsNullOrWhiteSpace(httpRequestType))
        {
            return Result.Failure<ClaimAudit>("HTTP request type is required.");
        }

        return new ClaimAudit
        {
            ClaimId = claimId,
            Created = created,
            HttpRequestType = httpRequestType
        };
    }
}
