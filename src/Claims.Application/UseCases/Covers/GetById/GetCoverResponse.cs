using Claims.Domain.Cover;

namespace Claims.Application.UseCases.Covers.GetById;

public sealed record GetCoverResponse(
    string Id,
    DateTime StartDate,
    DateTime EndDate,
    CoverType Type,
    decimal Premium);
