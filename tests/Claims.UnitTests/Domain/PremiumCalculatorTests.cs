using Claims.Domain.Cover;
using NUnit.Framework;

namespace Claims.UnitTests.Domain;

public sealed class PremiumCalculatorTests
{
    private readonly IPremiumCalculator _calculator = new PremiumCalculator();
    private static readonly DateTime StartDate = new(2026, 1, 1);

    [TestCase(31, 42_556.25d)]
    [TestCase(180, 237_187.50d)]
    [TestCase(181, 238_452.50d)]
    public void Yacht_UsesTheCorrectProgressiveTiers(
        int days,
        double expectedPremium)
    {
        var result = _calculator.Calculate(
            StartDate,
            StartDate.AddDays(days),
            CoverType.Yacht);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo((decimal)expectedPremium));
    }

    [TestCase(31, 46_470d)]
    [TestCase(180, 265_500d)]
    [TestCase(181, 266_955d)]
    public void NonYacht_UsesTheCorrectProgressiveTiers(
        int days,
        double expectedPremium)
    {
        var result = _calculator.Calculate(
            StartDate,
            StartDate.AddDays(days),
            CoverType.PassengerShip);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo((decimal)expectedPremium));
    }

    [TestCase(CoverType.Yacht, 41_250d)]
    [TestCase(CoverType.PassengerShip, 45_000d)]
    [TestCase(CoverType.ContainerShip, 48_750d)]
    [TestCase(CoverType.BulkCarrier, 48_750d)]
    [TestCase(CoverType.Tanker, 56_250d)]
    public void Calculate_UsesTheCorrectTypeMultiplier(
        CoverType type,
        double expectedPremium)
    {
        var result = _calculator.Calculate(
            StartDate,
            StartDate.AddDays(30),
            type);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo((decimal)expectedPremium));
    }

    [Test]
    public void Calculate_AppliesTheProgressiveDiscountsOnce()
    {
        var result = _calculator.Calculate(
            StartDate,
            StartDate.AddDays(365),
            CoverType.Yacht);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(471_212.50m));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Calculate_RejectsNonPositivePeriods(int days)
    {
        var result = _calculator.Calculate(
            StartDate,
            StartDate.AddDays(days),
            CoverType.Yacht);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.EqualTo("End date must be after the start date."));
    }
}
