using Claims.Application.Abstractions.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Data.Extensions;

public static class PaginationExtensions
{
    public static async Task<PagedResponse<T>> ToPagedResultAsync<T>(
        this IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var count = await source.CountAsync(cancellationToken);

        if (count == 0)
        {
            return new PagedResponse<T>([], 0, pageNumber, pageSize);
        }

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<T>(items, count, pageNumber, pageSize);
    }
}
