using Claims.Domain.Cover;

namespace Claims.Application.UseCases.Covers.Get;

public sealed record GetCoversResponse(
    string Id,
    DateTime StartDate,
    DateTime EndDate,
    CoverType Type,
    decimal Premium);
