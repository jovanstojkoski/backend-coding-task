using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Covers.Delete;

public interface IDeleteCoverUseCase
{
    Task<Result<DeleteCoverResponse>> ExecuteAsync(
        DeleteCoverRequest request,
        CancellationToken cancellationToken);
}
