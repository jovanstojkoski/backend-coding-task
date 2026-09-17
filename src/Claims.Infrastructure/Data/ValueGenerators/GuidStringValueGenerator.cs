using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Claims.Infrastructure.Data.ValueGenerators;

public sealed class GuidStringValueGenerator : ValueGenerator<string>
{
    public override bool GeneratesTemporaryValues => false;

    public override string Next(EntityEntry entry)
        => Guid.NewGuid().ToString();
}
