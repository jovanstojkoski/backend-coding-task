using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common.Models;
using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Covers.Get;

internal sealed class GetCoversUseCase(
    ICoverRepository coverRepository) : IGetCoversUseCase
{
    private readonly ICoverRepository _coverRepository = coverRepository;

    public async Task<Result<PagedResponse<GetCoversResponse>>> ExecuteAsync(
        GetCoversRequest request,
        CancellationToken cancellationToken)
    {
        var paginationResult = Pagination.Create(
            request.PageNumber,
            request.PageSize);

        if (paginationResult.IsFailure)
        {
            return Result.Failure<PagedResponse<GetCoversResponse>>(
                paginationResult.Error);
        }

        return await _coverRepository.GetPagedAsync(
            paginationResult.Value,
            cancellationToken);
    }
}
