using Claims.Domain.Cover;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MongoDB.EntityFrameworkCore.Extensions;
using Claims.Infrastructure.Data.ValueGenerators;

namespace Claims.Infrastructure.Data.Configurations;

public sealed class CoverConfiguration : IEntityTypeConfiguration<Cover>
{
    public void Configure(EntityTypeBuilder<Cover> builder)
    {
        builder.ToCollection("covers");
        builder.HasKey(cover => cover.Id);

        builder.Property(cover => cover.Id)
            .HasElementName("_id")
            .ValueGeneratedOnAdd()
            .HasValueGenerator<GuidStringValueGenerator>();

        builder.Property(cover => cover.StartDate)
            .HasElementName("startDate")
            .HasConversion(
                value => value.Date,
                value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

        builder.Property(cover => cover.EndDate)
            .HasElementName("endDate")
            .HasConversion(
                value => value.Date,
                value => DateTime.SpecifyKind(value, DateTimeKind.Utc));

        builder.Property(cover => cover.Type)
            .HasElementName("claimType");

        builder.Property(cover => cover.Premium)
            .HasElementName("premium");
    }
}
