using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Covers.Get;
using Claims.Application.UseCases.Covers.GetById;
using Claims.Domain.Cover;
using Claims.Infrastructure.Data;
using Claims.Infrastructure.Data.Extensions;
using Claims.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Claims.Infrastructure.Data.Repositories
{
    internal class CoverRepository : BaseRepository<ClaimsContext, Cover>, ICoverRepository
    {
        public CoverRepository(
            ClaimsContext dbContext,
            IMongoClient mongoClient,
            IOptions<MongoDbOptions> options)
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
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = _dbSet
                .AsNoTracking()
                .OrderByDescending(cover => cover.StartDate)
                .Select(cover => new GetCoversResponse(
                    cover.Id,
                    cover.StartDate,
                    cover.EndDate,
                    cover.Type,
                    cover.Premium));

            return await query.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
        }
    }
}
