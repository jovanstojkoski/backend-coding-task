using FluentValidation;

namespace Claims.Application.UseCases.Covers.Delete;

public sealed record DeleteCoverRequest(string Id);

internal sealed class DeleteCoverRequestValidator : AbstractValidator<DeleteCoverRequest>
{
    public DeleteCoverRequestValidator()
    {
        RuleFor(request => request.Id)
            .NotEmpty()
            .WithMessage("Cover ID is required.");
    }
}
