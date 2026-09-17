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
        var pageNumber = request.PageNumber ?? 1;
        var pageSize = request.PageSize ?? 10;

        return await _claimRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            cancellationToken);
    }
}
