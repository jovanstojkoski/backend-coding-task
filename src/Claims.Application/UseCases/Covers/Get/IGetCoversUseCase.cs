using Claims.Application.Abstractions.Common.Models;
using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Covers.Get;

public interface IGetCoversUseCase
{
    Task<Result<PagedResponse<GetCoversResponse>>> ExecuteAsync(
        GetCoversRequest request,
        CancellationToken cancellationToken);
}
