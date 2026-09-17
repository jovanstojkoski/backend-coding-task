using Claims.Application.Abstractions.Common;
using Claims.Domain.Cover;
using FluentValidation;

namespace Claims.Application.UseCases.Covers.Create;

public sealed record CreateCoverRequest(
    DateTime StartDate,
    DateTime EndDate,
    CoverType Type);

internal sealed class CreateCoverRequestValidator : AbstractValidator<CreateCoverRequest>
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateCoverRequestValidator(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;

        RuleFor(request => request.StartDate)
            .Must(startDate => startDate.Date >= _dateTimeProvider.UtcNow.Date)
            .WithMessage("Start date cannot be in the past.");

        RuleFor(request => request.EndDate)
            .GreaterThan(request => request.StartDate)
            .WithMessage("End date must be after the start date.")
            .LessThanOrEqualTo(request => request.StartDate.AddYears(1))
            .WithMessage("The insurance period cannot exceed one year.");

        RuleFor(request => request.Type)
            .IsInEnum()
            .WithMessage("Cover type is invalid.");
    }
}
