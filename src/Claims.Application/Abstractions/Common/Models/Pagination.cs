using Claims.Domain.Core.Primitives;

namespace Claims.Application.Abstractions.Common.Models;

public sealed record Pagination
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    private Pagination(int pageNumber, int pageSize)
    {
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public int PageNumber { get; }

    public int PageSize { get; }

    public int Skip => (PageNumber - 1) * PageSize;

    public int Take => PageSize;

    public static Result<Pagination> Create(
        int? pageNumber = null,
        int? pageSize = null)
    {
        var finalPage = pageNumber ?? DefaultPage;
        var requestedPageSize = pageSize ?? DefaultPageSize;
        var finalPageSize = Math.Min(requestedPageSize, MaxPageSize);

        if (finalPage < 1)
        {
            return Result.Failure<Pagination>("Page must be at least 1.");
        }

        if (requestedPageSize < 1)
        {
            return Result.Failure<Pagination>("PageSize must be at least 1.");
        }

        return new Pagination(finalPage, finalPageSize);
    }
}
