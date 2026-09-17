using FluentValidation;

namespace Claims.Application.UseCases.Claims.Delete;

public sealed record DeleteClaimRequest(string Id);

internal sealed class DeleteClaimRequestValidator : AbstractValidator<DeleteClaimRequest>
{
    public DeleteClaimRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .WithMessage("Claim ID is required.");
    }
}
