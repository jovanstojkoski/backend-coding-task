using Claims.Domain.Core.Primitives;

namespace Claims.Domain.Cover;

public class Cover
{
    private const decimal BaseDailyRate = 1250m;

    // Discount constants for tier 2 (Days 31–180)
    private const decimal YachtTier2Discount = 0.95m;  // 5% discount
    private const decimal OtherTier2Discount = 0.98m;  // 2% discount

    // Discount constants for tier 3 (Days 181+)
    private const decimal YachtTier3Discount = 0.92m;  // 5% + 3% = 8% discount
    private const decimal OtherTier3Discount = 0.97m;  // 2% + 1% = 3% discount

    public string Id { get; private set; } = null!;

    public DateTime StartDate { get; init; }

    public DateTime EndDate { get; init; }

    public CoverType Type { get; init; }

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

        var premiumResult = ComputePremium(startDate, endDate, type);
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

    public static Result<decimal> ComputePremium(
        DateTime startDate,
        DateTime endDate,
        CoverType type)
    {
        var totalDays = (endDate.Date - startDate.Date).Days; // 2 example: 2024-01-01 to 2024-01-03 = 2 days
        var dailyRate = BaseDailyRate * GetCoverTypeMultiplier(type); // Daily rate adjusted by cover type multiplier example: 1250 * 1.10 for Yacht
        var isYacht = type == CoverType.Yacht; // Check if the cover type is Yacht because in following days a different discount applies for Yacht vs other types

        // Breakdown into 3 progressive time brackets
        var tier1Days = Math.Min(totalDays, 30); // First 30 days: Days 1–30
        var tier2Days = Math.Min(Math.Max(totalDays - 30, 0), 150); // Following 150 days: Days 31–180, if totalDays > 30, then calculate the days in this tier, otherwise 0
        var tier3Days = Math.Max(totalDays - 180, 0); // Remaining Days: 181+, if totalDays > 180, then calculate the days in this tier, otherwise 0

        var tier2Multiplier = isYacht ? YachtTier2Discount : OtherTier2Discount; // Discount multiplier for tier 2 based on cover type Yacht vs Other 
        var tier3Multiplier = isYacht ? YachtTier3Discount : OtherTier3Discount; // Discount multiplier for tier 3 based on cover type Yacht vs Other

        decimal totalPremium = (tier1Days * dailyRate) // Premium for the first tier
                             + (tier2Days * dailyRate * tier2Multiplier) // Premium for the second tier
                             + (tier3Days * dailyRate * tier3Multiplier); // Premium for the third tier

        return Result.Success(totalPremium);
    }

    private static decimal GetCoverTypeMultiplier(CoverType type)
    {
        return type switch
        {
            CoverType.Yacht => 1.10m,
            CoverType.PassengerShip => 1.20m,
            CoverType.Tanker => 1.50m,
            _ => 1.30m
        };
    }
}
