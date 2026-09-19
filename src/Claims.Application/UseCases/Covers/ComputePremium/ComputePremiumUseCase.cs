using Claims.Domain.Core.Primitives;
using Claims.Domain.Cover;
using FluentValidation;

namespace Claims.Application.UseCases.Covers.ComputePremium;

internal sealed class ComputePremiumUseCase(
    IValidator<ComputePremiumRequest> validator,
    IPremiumCalculator premiumCalculator) : IComputePremiumUseCase
{
    private readonly IValidator<ComputePremiumRequest> _validator = validator;
    private readonly IPremiumCalculator _premiumCalculator = premiumCalculator;

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

        return _premiumCalculator.Calculate(
            request.StartDate,
            request.EndDate,
            request.Type);
    }
}
