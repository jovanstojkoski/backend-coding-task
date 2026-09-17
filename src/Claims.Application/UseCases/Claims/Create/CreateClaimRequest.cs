using Claims.Domain.Claim;
using FluentValidation;

namespace Claims.Application.UseCases.Claims.Create;

public sealed record CreateClaimRequest(
    string CoverId,
    DateTime Created,
    string Name,
    ClaimType Type,
    decimal DamageCost);

internal sealed class CreateClaimRequestValidator : AbstractValidator<CreateClaimRequest>
{
    public CreateClaimRequestValidator()
    {
        RuleFor(request => request.CoverId)
            .NotEmpty()
            .WithMessage("Cover ID is required.");

        RuleFor(request => request.Created)
            .NotEqual(default(DateTime))
            .WithMessage("Created date is required.");

        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required.");

        RuleFor(request => request.Type)
            .IsInEnum()
            .WithMessage("Claim type is invalid.");

        RuleFor(request => request.DamageCost)
            .InclusiveBetween(0m, 100_000m)
            .WithMessage("Damage cost must be between 0 and 100000.");
    }
}
