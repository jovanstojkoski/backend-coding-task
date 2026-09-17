using Claims.Domain.Core.Primitives;
using Claims.Domain.Cover;
using FluentValidation;

namespace Claims.Application.UseCases.Covers.ComputePremium;

internal sealed class ComputePremiumUseCase(
    IValidator<ComputePremiumRequest> validator) : IComputePremiumUseCase
{
    private readonly IValidator<ComputePremiumRequest> _validator = validator;

    public async Task<Result<decimal>> ExecuteAsync(
        ComputePremiumRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<decimal>(
                string.Join(Environment.NewLine, validationResult.Errors.Select(error => error.ErrorMessage)));
        }

        return Cover.ComputePremium(request.StartDate, request.EndDate, request.Type);
    }
}
