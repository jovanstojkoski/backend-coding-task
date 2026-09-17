using Claims.Application.Abstractions.Common.Models;
using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Claims.Get;

public interface IGetClaimsUseCase
{
    Task<Result<PagedResponse<GetClaimsResponse>>> ExecuteAsync(
        GetClaimsRequest request,
        CancellationToken cancellationToken);
}
