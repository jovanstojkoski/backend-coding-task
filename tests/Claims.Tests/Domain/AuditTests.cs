using Claims.Domain.Auditing;
using Xunit;

namespace Claims.Tests.Domain;

public sealed class AuditTests
{
    [Fact]
    public void ClaimAuditCreate_ReturnsFailureForMissingValues()
    {
        var result = ClaimAudit.Create(string.Empty, string.Empty, DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("Claim ID is required.", result.Error);
    }

    [Fact]
    public void CoverAuditCreate_ReturnsFailureForMissingValues()
    {
        var result = CoverAudit.Create(string.Empty, string.Empty, DateTime.UtcNow);

        Assert.True(result.IsFailure);
        Assert.Equal("Cover ID is required.", result.Error);
    }
}
