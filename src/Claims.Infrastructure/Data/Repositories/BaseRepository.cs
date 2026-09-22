using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Data.Repositories
{
    public abstract class BaseRepository<TContext, TEntity>
        where TContext : DbContext
        where TEntity : class
    {
        protected readonly TContext _dbContext;
        protected readonly DbSet<TEntity> _dbSet;
        public BaseRepository(TContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<TEntity>();
        }

        public void AddItem(TEntity entity)
        {
            _dbSet.Add(entity);
        }

        public void Remove(TEntity entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<TEntity?> GetByIdForUpdateAsync(
            string id,
            CancellationToken cancellationToken)
        {
            return await _dbSet.FindAsync([id], cancellationToken);
        }
    }
}
