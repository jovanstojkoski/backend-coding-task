using Claims.Domain.Cover;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Covers.Get;
using Claims.Application.UseCases.Covers.GetById;

namespace Claims.Application.Abstractions;

public interface ICoverRepository
{
    Task<GetCoverResponse?> GetByIdAsync(string id, CancellationToken cancellationToken);

    Task<PagedResponse<GetCoversResponse>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken);

    Task<Cover?> GetByIdForUpdateAsync(
        string id,
        CancellationToken cancellationToken);

    void AddItem(Cover cover);

    void Remove(Cover cover);
}
