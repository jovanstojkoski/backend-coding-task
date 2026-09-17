using Claims.Domain.Claim;

namespace Claims.Application.UseCases.Claims.GetById;

public sealed record GetClaimResponse(
    string Id,
    string CoverId,
    DateTime Created,
    string Name,
    ClaimType Type,
    decimal DamageCost);
