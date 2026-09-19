using Claims.Domain.Core.Primitives;

namespace Claims.Domain.Cover;

public interface IPremiumCalculator
{
    Result<decimal> Calculate(
        DateTime startDate,
        DateTime endDate,
        CoverType type);
}
