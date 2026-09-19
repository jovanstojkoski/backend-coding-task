using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Claims.Get;
using Claims.Application.UseCases.Claims.GetById;
using Claims.Domain.Cover;
using Claims.Domain.Claim;
using Claims.Infrastructure.Data;
using Claims.Infrastructure.Data.Extensions;
using Claims.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Claims.Infrastructure.Data.Repositories
{
    internal class ClaimRepository : BaseRepository<ClaimsContext, Claim>, IClaimRepository
    {
        private readonly IMongoCollection<BsonDocument> _claims;

        public ClaimRepository(
            ClaimsContext dbContext,
            IMongoClient mongoClient,
            IOptions<MongoDbOptions> options)
            : base(dbContext)
        {
            _claims = mongoClient
                .GetDatabase(options.Value.DatabaseName)
                .GetCollection<BsonDocument>("claims");
        }

        public async Task<GetClaimResponse?> GetByIdAsync(
            string id,
            CancellationToken cancellationToken)
        {
            return await _dbSet
                .AsNoTracking()
                .Where(claim => claim.Id == id)
                .Select(claim => new GetClaimResponse(
                    claim.Id,
                    claim.CoverId,
                    claim.Created,
                    claim.Name,
                    claim.Type,
                    claim.DamageCost))
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<PagedResponse<GetClaimsResponse>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            var query = _dbSet
                .AsNoTracking()
                .OrderByDescending(claim => claim.Created)
                .Select(claim => new GetClaimsResponse(
                    claim.Id,
                    claim.CoverId,
                    claim.Created,
                    claim.Name,
                    claim.Type,
                    claim.DamageCost));

            return await query.ToPagedResultAsync(pageNumber, pageSize, cancellationToken);
        }

        public async Task<bool> RemoveByIdAsync(
            string id,
            CancellationToken cancellationToken)
        {
            var result = await _claims.DeleteOneAsync(
                new BsonDocument("_id", id),
                cancellationToken);

            return result.DeletedCount == 1;
        }
    }
}
