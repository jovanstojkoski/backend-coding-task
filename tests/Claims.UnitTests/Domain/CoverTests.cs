using Claims.Domain.Cover;
using NUnit.Framework;

namespace Claims.UnitTests.Domain;

public sealed class CoverTests
{
    private readonly IPremiumCalculator _premiumCalculator = new PremiumCalculator();
    private static readonly DateTime CurrentDate = new(2026, 1, 1);

    [Test]
    public void Create_RejectsAStartDateInThePast()
    {
        var result = Cover.Create(
            CurrentDate.AddDays(-1),
            CurrentDate.AddDays(10),
            CoverType.Yacht,
            CurrentDate,
            _premiumCalculator);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Start date cannot be in the past."));
    }

    [Test]
    public void Create_RejectsPeriodsLongerThanOneYear()
    {
        var startDate = CurrentDate.AddDays(1);

        var result = Cover.Create(
            startDate,
            startDate.AddYears(1).AddDays(1),
            CoverType.Yacht,
            CurrentDate,
            _premiumCalculator);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.EqualTo("The insurance period cannot exceed one year."));
    }
}
