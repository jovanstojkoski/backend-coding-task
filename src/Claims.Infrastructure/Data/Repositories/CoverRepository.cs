using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Covers.Get;
using Claims.Application.UseCases.Covers.GetById;
using Claims.Domain.Cover;
using Claims.Infrastructure.Data;
using Claims.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Data.Repositories
{
    internal class CoverRepository : BaseRepository<ClaimsContext, Cover>, ICoverRepository
    {
        public CoverRepository(ClaimsContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<GetCoverResponse?> GetByIdAsync(
            string id,
            CancellationToken cancellationToken)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(cover => cover.Id == id)
                .Select(cover => new GetCoverResponse(
                    cover.Id,
                    cover.StartDate,
                    cover.EndDate,
                    cover.Type,
                    cover.Premium))
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<PagedResponse<GetCoversResponse>> GetPagedAsync(
            Pagination pagination,
            CancellationToken cancellationToken)
        {
            var query = _dbSet
                .AsNoTracking()
                .OrderByDescending(cover => cover.StartDate)
                .ThenBy(cover => cover.Id)
                .Select(cover => new GetCoversResponse(
                    cover.Id,
                    cover.StartDate,
                    cover.EndDate,
                    cover.Type,
                    cover.Premium));

            return await query.ToPagedResultAsync(pagination, cancellationToken);
        }
    }
}
