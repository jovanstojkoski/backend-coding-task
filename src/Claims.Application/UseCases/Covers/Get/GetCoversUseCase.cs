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
        var pageNumber = request.PageNumber ?? 1;
        var pageSize = request.PageSize ?? 10;

        return await _coverRepository.GetPagedAsync(
            pageNumber,
            pageSize,
            cancellationToken);
    }
}
