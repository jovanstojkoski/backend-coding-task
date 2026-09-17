using Claims.Domain.Core.Primitives;

namespace Claims.Application.UseCases.Covers.ComputePremium;

public interface IComputePremiumUseCase
{
    Task<Result<decimal>> ExecuteAsync(
        ComputePremiumRequest request,
        CancellationToken cancellationToken);
}
