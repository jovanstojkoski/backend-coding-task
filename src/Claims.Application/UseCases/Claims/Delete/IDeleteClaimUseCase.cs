using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Claims.Delete;

public interface IDeleteClaimUseCase
{
    Task<Result<DeleteClaimResponse>> ExecuteAsync(
        DeleteClaimRequest request,
        CancellationToken cancellationToken);
}
