using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Covers.Create;

public interface ICreateCoverUseCase
{
    Task<Result<CreateCoverResponse>> ExecuteAsync(
        CreateCoverRequest request,
        CancellationToken cancellationToken);
}
