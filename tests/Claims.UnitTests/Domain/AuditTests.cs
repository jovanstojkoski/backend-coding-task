using Claims.Domain.Auditing;
using NUnit.Framework;

namespace Claims.UnitTests.Domain;

public sealed class AuditTests
{
    [Test]
    public void ClaimAuditCreate_ReturnsFailureForMissingValues()
    {
        var result = ClaimAudit.Create(
            string.Empty,
            string.Empty,
            new DateTime(2026, 1, 1));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Claim ID is required."));
    }

    [Test]
    public void CoverAuditCreate_ReturnsFailureForMissingValues()
    {
        var result = CoverAudit.Create(
            string.Empty,
            string.Empty,
            new DateTime(2026, 1, 1));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Cover ID is required."));
    }
}
