using Claims.Domain.Core.Primitives;

namespace Claims.Domain.Cover;

public class Cover
{
    private const decimal BaseDailyRate = 1250m;

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
        DateTime currentDate)
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

        var premiumResult = cover.ComputePremium();
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

    public Result<decimal> ComputePremium()
    {
        if (EndDate.Date <= StartDate.Date)
        {
            return Result.Failure<decimal>("End date must be after the start date.");
        }

        var days = (EndDate.Date - StartDate.Date).Days;
        var dailyRate = BaseDailyRate * GetTypeMultiplier();
        var yacht = Type == CoverType.Yacht;

        var firstPeriodDays = Math.Min(days, 30);
        var secondPeriodDays = Math.Min(Math.Max(days - 30, 0), 150);
        var remainingPeriodDays = Math.Max(days - 180, 0);

        decimal premium = firstPeriodDays * dailyRate;
        premium += secondPeriodDays * dailyRate * (yacht ? 0.95m : 0.98m);
        premium += remainingPeriodDays * dailyRate * (yacht ? 0.92m : 0.97m);

        return premium;
    }

    public static Result<decimal> ComputePremium(
        DateTime startDate,
        DateTime endDate,
        CoverType type)
    {
        var cover = new Cover
        {
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            Type = type
        };

        return cover.ComputePremium();
    }

    private decimal GetTypeMultiplier()
    {
        return Type switch
        {
            CoverType.Yacht => 1.10m,
            CoverType.PassengerShip => 1.20m,
            CoverType.Tanker => 1.50m,
            _ => 1.30m
        };
    }
}
