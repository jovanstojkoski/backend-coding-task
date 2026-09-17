using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Claims.GetById;

public interface IGetClaimUseCase
{
    Task<Result<GetClaimResponse>> ExecuteAsync(
        GetClaimRequest request,
        CancellationToken cancellationToken);
}
