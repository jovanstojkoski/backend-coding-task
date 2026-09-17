using Claims.Domain.Claim;

namespace Claims.Application.UseCases.Claims.Create;

public sealed record CreateClaimResponse(
    string Id,
    string CoverId,
    DateTime Created,
    string Name,
    ClaimType Type,
    decimal DamageCost);
