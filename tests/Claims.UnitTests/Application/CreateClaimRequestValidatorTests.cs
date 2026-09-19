using Claims.Application.UseCases.Claims.Create;
using Claims.Domain.Claim;
using NUnit.Framework;

namespace Claims.UnitTests.Application;

public sealed class CreateClaimRequestValidatorTests
{
    [Test]
    public async Task ValidateAsync_RejectsZeroDamageCost()
    {
        var validator = new CreateClaimRequestValidator();

        var result = await validator.ValidateAsync(
            new CreateClaimRequest(
                "cover-id",
                new DateTime(2026, 1, 10),
                "Collision damage",
                ClaimType.Collision,
                0m),
            CancellationToken.None);

        Assert.That(result.IsValid, Is.False);
        Assert.That(
            result.Errors.Any(
                error => error.ErrorMessage == "Damage cost must be greater than 0."),
            Is.True);
    }

    [Test]
    public async Task ValidateAsync_AllowsMaximumDamageCost()
    {
        var validator = new CreateClaimRequestValidator();
        var result = await validator.ValidateAsync(
            new CreateClaimRequest(
                "cover-id",
                new DateTime(2026, 1, 10),
                "Collision damage",
                ClaimType.Collision,
                100_000m),
            CancellationToken.None);

        Assert.That(result.IsValid, Is.True);
    }
}
