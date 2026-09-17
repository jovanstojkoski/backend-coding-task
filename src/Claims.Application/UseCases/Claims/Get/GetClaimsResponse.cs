using Claims.Domain.Claim;

namespace Claims.Application.UseCases.Claims.Get;

public sealed record GetClaimsResponse(
    string Id,
    string CoverId,
    DateTime Created,
    string Name,
    ClaimType Type,
    decimal DamageCost);
