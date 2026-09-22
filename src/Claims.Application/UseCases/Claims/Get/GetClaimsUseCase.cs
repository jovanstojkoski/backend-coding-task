using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common.Models;
using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Claims.Get;

internal sealed class GetClaimsUseCase(
    IClaimRepository claimRepository) : IGetClaimsUseCase
{
    private readonly IClaimRepository _claimRepository = claimRepository;

    public async Task<Result<PagedResponse<GetClaimsResponse>>> ExecuteAsync(
        GetClaimsRequest request,
        CancellationToken cancellationToken)
    {
        var paginationResult = Pagination.Create(
            request.PageNumber,
            request.PageSize);

        if (paginationResult.IsFailure)
        {
            return Result.Failure<PagedResponse<GetClaimsResponse>>(
                paginationResult.Error);
        }

        return await _claimRepository.GetPagedAsync(
            paginationResult.Value,
            cancellationToken);
    }
}
