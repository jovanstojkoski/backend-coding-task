using Claims.Application.Abstractions.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Data.Extensions;

public static class PaginationExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        Pagination pagination,
        CancellationToken cancellationToken)
    {
        var count = await source.CountAsync(cancellationToken);

        if (count == 0)
        {
            return new PagedResponse<T>(
                [],
                0,
                pagination.PageNumber,
                pagination.PageSize);
        }

        var items = await source
            .Skip(pagination.Skip)
            .Take(pagination.Take)
            .ToListAsync(cancellationToken);

        return new PagedResponse<T>(
            items,
            count,
            pagination.PageNumber,
            pagination.PageSize);
    }
}
