using Claims.Domain.Auditing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Claims.Infrastructure.Data.Configurations;

public sealed class CoverAuditConfiguration : IEntityTypeConfiguration<CoverAudit>
{
    public void Configure(EntityTypeBuilder<CoverAudit> builder)
    {
        builder.ToTable("CoverAudits");
        builder.HasKey(audit => audit.Id);

        builder.Property(audit => audit.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(audit => audit.CoverId)
            .IsRequired();

        builder.Property(audit => audit.Created)
            .IsRequired();

        builder.Property(audit => audit.HttpRequestType)
            .IsRequired();
    }
}
