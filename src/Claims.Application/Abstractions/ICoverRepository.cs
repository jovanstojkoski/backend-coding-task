using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Claims.GetById;
using Claims.Application.UseCases.Covers.Get;
using Claims.Application.UseCases.Covers.GetById;
using Claims.Domain.Cover;

namespace Claims.Application.Abstractions;

public interface ICoverRepository
{
    Task<GetCoverResponse?> GetByIdAsync(
        string id,
        CancellationToken cancellationToken);

    Task<Cover?> GetByIdForUpdateAsync(
        string id,
        CancellationToken cancellationToken);

    Task<PagedResponse<GetCoversResponse>> GetPagedAsync(
        Pagination pagination,
        CancellationToken cancellationToken);

    void AddItem(Cover cover);

    void Remove(Cover cover);
}
