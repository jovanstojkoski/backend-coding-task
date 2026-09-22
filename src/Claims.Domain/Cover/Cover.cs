using Claims.Domain.Core.Primitives;

namespace Claims.Domain.Cover;

public class Cover
{
    public string Id { get; private set; } = null!;

    public DateTime StartDate { get; private set; }

    public DateTime EndDate { get; private set; }

    public CoverType Type { get; private set; }

    public decimal Premium { get; private set; }

    private Cover()
    {
    }

    public static Result<Cover> Create(
        DateTime startDate,
        DateTime endDate,
        CoverType type,
        DateTime currentDate,
        IPremiumCalculator premiumCalculator)
    {
        var validationResult = ValidateDates(startDate, endDate, currentDate);
        if (validationResult.IsFailure)
        {
            return Result.Failure<Cover>(validationResult.Error);
        }

        var cover = new Cover
        {
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            Type = type
        };

        var premiumResult = premiumCalculator.Calculate(startDate, endDate, type);
        if (premiumResult.IsFailure)
        {
            return Result.Failure<Cover>(premiumResult.Error);
        }

        cover.Premium = premiumResult.Value;
        return cover;
    }

    public static Result<bool> ValidateDates(
        DateTime startDate,
        DateTime endDate,
        DateTime currentDate)
    {
        if (startDate.Date < currentDate.Date)
        {
            return Result.Failure<bool>("Start date cannot be in the past.");
        }

        if (endDate.Date <= startDate.Date)
        {
            return Result.Failure<bool>("End date must be after the start date.");
        }

        if (endDate.Date > startDate.Date.AddYears(1))
        {
            return Result.Failure<bool>("The insurance period cannot exceed one year.");
        }

        return Result.Success(true);
    }

}
