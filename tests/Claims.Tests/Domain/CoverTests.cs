using Claims.Domain.Cover;
using Xunit;

namespace Claims.Tests.Domain;

public sealed class CoverTests
{
    [Theory]
    [InlineData(CoverType.Yacht, 41_250d)]
    [InlineData(CoverType.PassengerShip, 45_000d)]
    [InlineData(CoverType.ContainerShip, 48_750d)]
    [InlineData(CoverType.BulkCarrier, 48_750d)]
    [InlineData(CoverType.Tanker, 56_250d)]
    public void ComputePremium_UsesTheCorrectTypeMultiplier(
        CoverType type,
        double expectedPremium)
    {
        var result = Cover.ComputePremium(
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(30),
            type);

        Assert.True(result.IsSuccess);
        Assert.Equal((decimal)expectedPremium, result.Value);
    }

    [Fact]
    public void ComputePremium_AppliesTheProgressiveDiscountsOnce()
    {
        var result = Cover.ComputePremium(
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(365),
            CoverType.Yacht);

        Assert.True(result.IsSuccess);
        Assert.Equal(471_212.50m, result.Value);
    }

    [Fact]
    public void Create_RejectsAStartDateInThePast()
    {
        var result = Cover.Create(
            DateTime.UtcNow.Date.AddDays(-1),
            DateTime.UtcNow.Date.AddDays(10),
            CoverType.Yacht,
            DateTime.UtcNow.Date);

        Assert.True(result.IsFailure);
        Assert.Equal("Start date cannot be in the past.", result.Error);
    }

    [Fact]
    public void Create_RejectsPeriodsLongerThanOneYear()
    {
        var startDate = DateTime.UtcNow.Date.AddDays(1);

        var result = Cover.Create(
            startDate,
            startDate.AddYears(1).AddDays(1),
            CoverType.Yacht,
            DateTime.UtcNow.Date);

        Assert.True(result.IsFailure);
        Assert.Equal("The insurance period cannot exceed one year.", result.Error);
    }
}
