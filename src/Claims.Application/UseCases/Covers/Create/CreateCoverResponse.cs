using Claims.Domain.Cover;

namespace Claims.Application.UseCases.Covers.Create;

public sealed record CreateCoverResponse(
    string Id,
    DateTime StartDate,
    DateTime EndDate,
    CoverType Type,
    decimal Premium);
