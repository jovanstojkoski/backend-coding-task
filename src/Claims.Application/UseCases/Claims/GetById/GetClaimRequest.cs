using FluentValidation;

namespace Claims.Application.UseCases.Claims.GetById;

public sealed record GetClaimRequest(string Id);

internal sealed class GetClaimRequestValidator : AbstractValidator<GetClaimRequest>
{
    public GetClaimRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .WithMessage("Claim ID is required.");
    }
}
