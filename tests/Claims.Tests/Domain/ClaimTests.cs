using Claims.Domain.Claim;
using Claims.Domain.Cover;
using Xunit;

namespace Claims.Tests.Domain;

public sealed class ClaimTests
{
    [Fact]
    public void Create_AcceptsAClaimInsideTheCoverPeriod()
    {
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var coverResult = Cover.Create(
            startDate,
            startDate.AddDays(30),
            CoverType.Yacht,
            DateTime.UtcNow.Date);
        var claimResult = Claim.Create(
            "cover-id",
            startDate.AddDays(10),
            "Collision damage",
            ClaimType.Collision,
            10_000m,
            coverResult.Value.StartDate,
            coverResult.Value.EndDate);

        Assert.True(coverResult.IsSuccess);
        Assert.True(claimResult.IsSuccess);
    }

    [Fact]
    public void Create_RejectsAClaimOutsideTheCoverPeriod()
    {
        var startDate = DateTime.UtcNow.Date.AddDays(1);
        var coverResult = Cover.Create(
            startDate,
            startDate.AddDays(30),
            CoverType.Yacht,
            DateTime.UtcNow.Date);
        var claimResult = Claim.Create(
            "cover-id",
            startDate.AddDays(31),
            "Collision damage",
            ClaimType.Collision,
            10_000m,
            coverResult.Value.StartDate,
            coverResult.Value.EndDate);

        Assert.True(coverResult.IsSuccess);
        Assert.True(claimResult.IsFailure);
        Assert.Equal("Claim date must be within the cover period.", claimResult.Error);
    }

    [Fact]
    public void Create_RejectsDamageCostAboveTheBusinessLimit()
    {
        var result = Claim.Create(
            "cover-id",
            DateTime.UtcNow.Date,
            "Collision damage",
            ClaimType.Collision,
            100_000.01m,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(30));

        Assert.True(result.IsFailure);
        Assert.Equal("Damage cost must be between 0 and 100000.", result.Error);
    }
}
