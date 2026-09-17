using Claims.Domain.Cover;
using Claims.Domain.Claim;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;
using Claims.Infrastructure.Data.ValueGenerators;

namespace Claims.Infrastructure.Data.Configurations;

public sealed class ClaimConfiguration : IEntityTypeConfiguration<Claim>
{
    public void Configure(EntityTypeBuilder<Claim> builder)
    {
        builder.ToCollection("claims");
        builder.HasKey(claim => claim.Id);

        builder.Property(claim => claim.Id)
            .HasElementName("_id")
            .ValueGeneratedOnAdd()
            .HasValueGenerator<GuidStringValueGenerator>();

        builder.Property(claim => claim.CoverId)
            .HasElementName("coverId");

        builder.Property(claim => claim.Created)
            .HasElementName("created")
            .HasConversion(
                value => value.Date,
                value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

        builder.Property(claim => claim.Name)
            .HasElementName("name");

        builder.Property(claim => claim.Type)
            .HasElementName("claimType");

        builder.Property(claim => claim.DamageCost)
            .HasElementName("damageCost");
    }
}
