using Claims.Application.Abstractions;
using Claims.Domain.Core.Primitives;
using FluentValidation;

namespace Claims.Application.UseCases.Covers.GetById;

internal sealed class GetCoverUseCase(
    IValidator<GetCoverRequest> validator,
    ICoverRepository coverRepository) : IGetCoverUseCase
{
    private readonly IValidator<GetCoverRequest> _validator = validator;
    private readonly ICoverRepository _coverRepository = coverRepository;

    public async Task<Result<GetCoverResponse>> ExecuteAsync(
        GetCoverRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<GetCoverResponse>(
                string.Join(Environment.NewLine, validationResult.Errors.Select(error => error.ErrorMessage)));
        }

        var response = await _coverRepository.GetByIdAsync(request.Id, cancellationToken);
        if (response is null)
        {
            return Result.Failure<GetCoverResponse>("Cover not found.");
        }

        return response;
    }
}
