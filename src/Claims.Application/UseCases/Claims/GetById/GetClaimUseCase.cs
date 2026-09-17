using Claims.Application.Abstractions;
using Claims.Domain.Core.Primitives;
using FluentValidation;

namespace Claims.Application.UseCases.Claims.GetById;

internal sealed class GetClaimUseCase(
    IValidator<GetClaimRequest> validator,
    IClaimRepository claimRepository) : IGetClaimUseCase
{
    private readonly IValidator<GetClaimRequest> _validator = validator;
    private readonly IClaimRepository _claimRepository = claimRepository;

    public async Task<Result<GetClaimResponse>> ExecuteAsync(
        GetClaimRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<GetClaimResponse>(
                string.Join(Environment.NewLine, validationResult.Errors.Select(error => error.ErrorMessage)));
        }

        var response = await _claimRepository.GetByIdAsync(request.Id, cancellationToken);
        if (response is null)
        {
            return Result.Failure<GetClaimResponse>("Claim not found.");
        }

        return response;
    }
}
