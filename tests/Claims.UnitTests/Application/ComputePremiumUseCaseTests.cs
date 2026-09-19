using Claims.Application.UseCases.Covers.ComputePremium;
using Claims.Domain.Cover;
using Claims.Domain.Core.Primitives;
using Moq;
using NUnit.Framework;

namespace Claims.UnitTests.Application;

public sealed class ComputePremiumUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_WhenRequestIsInvalid_DoesNotCallCalculator()
    {
        var calculator = new Mock<IPremiumCalculator>();

        var useCase = new ComputePremiumUseCase(
            new ComputePremiumRequestValidator(),
            calculator.Object);

        var result = await useCase.ExecuteAsync(
            new ComputePremiumRequest(
                new DateTime(2026, 1, 2),
                new DateTime(2026, 1, 1),
                (CoverType)999),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        calculator.Verify(
            item => item.Calculate(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                It.IsAny<CoverType>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenValid_ReturnsCalculatorResult()
    {
        var calculator = new Mock<IPremiumCalculator>();

        calculator
            .Setup(item => item.Calculate(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                CoverType.Yacht))
            .Returns(Result.Success(123m));

        var useCase = new ComputePremiumUseCase(
            new ComputePremiumRequestValidator(),
            calculator.Object);

        var result = await useCase.ExecuteAsync(
            new ComputePremiumRequest(
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 31),
                CoverType.Yacht),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo(123m));
    }

    [Test]
    public async Task ExecuteAsync_WhenCalculatorFails_ReturnsFailure()
    {
        var calculator = new Mock<IPremiumCalculator>();

        calculator
            .Setup(item => item.Calculate(
                It.IsAny<DateTime>(),
                It.IsAny<DateTime>(),
                CoverType.Yacht))
            .Returns(Result.Failure<decimal>("Premium could not be calculated."));

        var useCase = new ComputePremiumUseCase(
            new ComputePremiumRequestValidator(),
            calculator.Object);

        var result = await useCase.ExecuteAsync(
            new ComputePremiumRequest(
                new DateTime(2026, 1, 1),
                new DateTime(2026, 1, 31),
                CoverType.Yacht),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Premium could not be calculated."));
    }
}
