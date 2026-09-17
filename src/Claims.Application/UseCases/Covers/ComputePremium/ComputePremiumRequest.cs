using Claims.Domain.Cover;
using FluentValidation;

namespace Claims.Application.UseCases.Covers.ComputePremium;

public sealed record ComputePremiumRequest(
    DateTime StartDate,
    DateTime EndDate,
    CoverType Type);

internal sealed class ComputePremiumRequestValidator : AbstractValidator<ComputePremiumRequest>
{
    public ComputePremiumRequestValidator()
    {
        RuleFor(request => request.EndDate)
            .GreaterThan(request => request.StartDate)
            .WithMessage("End date must be after the start date.");

        RuleFor(request => request.Type)
            .IsInEnum()
            .WithMessage("Cover type is invalid.");
    }
}
