using Claims.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Infrastructure.Data.Configurations;

public sealed class ClaimAuditConfiguration : IEntityTypeConfiguration<ClaimAudit>
{
    public void Configure(EntityTypeBuilder<ClaimAudit> builder)
    {
        builder.ToTable("ClaimAudits");
        builder.HasKey(audit => audit.Id);

        builder.Property(audit => audit.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(audit => audit.ClaimId)
            .IsRequired();

        builder.Property(audit => audit.Created)
            .IsRequired();

        builder.Property(audit => audit.HttpRequestType)
            .IsRequired();
    }
}
