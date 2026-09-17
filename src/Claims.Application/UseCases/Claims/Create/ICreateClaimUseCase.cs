using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Claims.Create;

public interface ICreateClaimUseCase
{
    Task<Result<CreateClaimResponse>> ExecuteAsync(
        CreateClaimRequest request,
        CancellationToken cancellationToken);
}
