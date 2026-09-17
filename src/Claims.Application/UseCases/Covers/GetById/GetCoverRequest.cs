using FluentValidation;

namespace Claims.Application.UseCases.Covers.GetById;

public sealed record GetCoverRequest(string Id);

internal sealed class GetCoverRequestValidator : AbstractValidator<GetCoverRequest>
{
    public GetCoverRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .WithMessage("Cover ID is required.");
    }
}
