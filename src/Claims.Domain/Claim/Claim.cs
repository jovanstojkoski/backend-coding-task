using Claims.Domain.Core.Primitives;

namespace Claims.Domain.Claim;

public class Claim
{
    public string Id { get; private set; } = null!;

    public string CoverId { get; init; } = null!;

    public DateTime Created { get; init; }

    public string Name { get; init; } = null!;

    public ClaimType Type { get; init; }

    public decimal DamageCost { get; init; }

    private Claim()
    {
    }

    public static Result<Claim> Create(
        string coverId,
        DateTime created,
        string name,
        ClaimType type,
        decimal damageCost,
        DateTime coverStartDate,
        DateTime coverEndDate)
    {
        if (string.IsNullOrWhiteSpace(coverId))
        {
            return Result.Failure<Claim>("Cover ID is required.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<Claim>("Name is required.");
        }

        if (created == default)
        {
            return Result.Failure<Claim>("Created date is required.");
        }

        if (!Enum.IsDefined(type))
        {
            return Result.Failure<Claim>("Claim type is invalid.");
        }

        if (damageCost is < 0 or > 100_000m)
        {
            return Result.Failure<Claim>("Damage cost must be between 0 and 100000.");
        }

        if (coverEndDate.Date < coverStartDate.Date)
        {
            return Result.Failure<Claim>("Cover period is invalid.");
        }

        if (created.Date < coverStartDate.Date || created.Date > coverEndDate.Date)
        {
            return Result.Failure<Claim>("Claim date must be within the cover period.");
        }

        return new Claim
        {
            CoverId = coverId,
            Created = created.Date,
            Name = name,
            Type = type,
            DamageCost = damageCost
        };
    }
}
