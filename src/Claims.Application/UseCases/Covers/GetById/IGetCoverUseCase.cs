using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Covers.GetById;

public interface IGetCoverUseCase
{
    Task<Result<GetCoverResponse>> ExecuteAsync(
        GetCoverRequest request,
        CancellationToken cancellationToken);
}
