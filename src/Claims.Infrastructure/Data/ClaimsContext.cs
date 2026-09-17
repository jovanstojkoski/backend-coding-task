using Claims.Application.Abstractions;
using Claims.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Data;

public class ClaimsContext : DbContext, IClaimsUnitOfWork
{
    public ClaimsContext(DbContextOptions<ClaimsContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ClaimConfiguration());
        modelBuilder.ApplyConfiguration(new CoverConfiguration());
    }
}
