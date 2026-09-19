using Claims.Domain.Claim;
using NUnit.Framework;

namespace Claims.UnitTests.Domain;

public sealed class ClaimTests
{
    private static readonly DateTime CoverStartDate = new(2026, 1, 10);
    private static readonly DateTime CoverEndDate = CoverStartDate.AddDays(30);

    [TestCase(0)]
    [TestCase(30)]
    public void Create_AcceptsClaimOnCoverBoundaries(int daysFromStart)
    {
        var result = CreateClaim(created: CoverStartDate.AddDays(daysFromStart));

        Assert.That(result.IsSuccess, Is.True);
    }

    [TestCase(-1)]
    [TestCase(31)]
    public void Create_RejectsClaimOutsideTheCoverPeriod(int daysFromStart)
    {
        var result = CreateClaim(created: CoverStartDate.AddDays(daysFromStart));

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.EqualTo("Claim date must be within the cover period."));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_RejectsMissingCoverId(string? coverId)
    {
        var result = CreateClaim(coverId: coverId!);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Cover ID is required."));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(" ")]
    public void Create_RejectsMissingName(string? name)
    {
        var result = CreateClaim(name: name!);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Name is required."));
    }

    [Test]
    public void Create_RejectsMissingCreatedDate()
    {
        var result = CreateClaim(created: DateTime.MinValue);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Created date is required."));
    }

    [Test]
    public void Create_RejectsInvalidClaimType()
    {
        var result = CreateClaim(type: (ClaimType)999);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Claim type is invalid."));
    }

    [Test]
    public void Create_RejectsInvalidCoverPeriod()
    {
        var result = Claim.Create(
            "cover-id",
            CoverStartDate,
            "Collision damage",
            ClaimType.Collision,
            10_000m,
            CoverEndDate,
            CoverStartDate);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Cover period is invalid."));
    }

    [TestCase(0d)]
    [TestCase(100_000.01d)]
    public void Create_RejectsDamageCostOutsideTheBusinessLimit(
        double damageCost)
    {
        var result = CreateClaim(damageCost: (decimal)damageCost);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.EqualTo(
                "Damage cost must be greater than 0 and less than or equal to 100000."));
    }

    [Test]
    public void Create_AcceptsMaximumDamageCost()
    {
        var result = CreateClaim(damageCost: 100_000m);

        Assert.That(result.IsSuccess, Is.True);
    }

    private static Claims.Domain.Core.Primitives.Result<Claim> CreateClaim(
        string coverId = "cover-id",
        DateTime? created = null,
        string name = "Collision damage",
        ClaimType type = ClaimType.Collision,
        decimal damageCost = 10_000m)
    {
        return Claim.Create(
            coverId,
            created ?? CoverStartDate.AddDays(10),
            name,
            type,
            damageCost,
            CoverStartDate,
            CoverEndDate);
    }
}
