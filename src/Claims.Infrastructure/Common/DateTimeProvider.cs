using Claims.Application.Abstractions.Common;

namespace Claims.Infrastructure.Common;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
