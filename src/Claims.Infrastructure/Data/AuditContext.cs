using Claims.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Claims.Infrastructure.Data;

public class AuditContext : DbContext
{
    public AuditContext(DbContextOptions<AuditContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ClaimAuditConfiguration());
        modelBuilder.ApplyConfiguration(new CoverAuditConfiguration());
    }
}
