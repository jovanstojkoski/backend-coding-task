using Claims.Domain.Claim;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Claims.Get;
using Claims.Application.UseCases.Claims.GetById;

namespace Claims.Application.Abstractions;

public interface IClaimRepository
{
    Task<PagedResponse<GetClaimsResponse>> GetPagedAsync(
        Pagination pagination,
        CancellationToken cancellationToken);

    Task<bool> HasAnyForCoverAsync(
        string coverId,
        CancellationToken cancellationToken);

    Task<GetClaimResponse?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken);

    Task<Claim?> GetByIdForUpdateAsync(
        string id,
        CancellationToken cancellationToken);

    void AddItem(Claim claim);

    void Remove(Claim claim);
}
