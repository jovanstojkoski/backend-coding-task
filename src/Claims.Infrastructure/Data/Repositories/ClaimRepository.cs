using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Claims.Get;
using Claims.Application.UseCases.Claims.GetById;
using Claims.Domain.Cover;
using Claims.Domain.Claim;
using Claims.Infrastructure.Data;
using Claims.Infrastructure.Data.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Data.Repositories
{
    internal class ClaimRepository : BaseRepository<ClaimsContext, Claim>, IClaimRepository
    {
        public ClaimRepository(ClaimsContext dbContext)
            : base(dbContext)
        {
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

        public async Task<Claim?> GetByIdForUpdateAsync(
            string id,
            CancellationToken cancellationToken)
        {
            return await _dbSet
                .SingleOrDefaultAsync(
                    claim => claim.Id == id,
                    cancellationToken);
        }
    }
}
