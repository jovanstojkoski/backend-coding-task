using Claims.Domain.Claim;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Claims.Get;
using Claims.Application.UseCases.Claims.GetById;

namespace Claims.Application.Abstractions;

public interface IClaimRepository
{
    Task<GetClaimResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<PagedResponse<GetClaimsResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    void AddItem(Claim claim);

    void Remove(Claim claim);

    Task<Claim?> GetByIdForUpdateAsync(
        string id,
        CancellationToken cancellationToken);
}
