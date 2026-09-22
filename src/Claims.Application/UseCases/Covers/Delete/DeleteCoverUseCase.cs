using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Audit;
using Claims.Application.Abstractions.Common;
using Claims.Domain.Auditing;
using Claims.Domain.Core.Primitives;
using FluentValidation;

namespace Claims.Application.UseCases.Covers.Delete;

internal sealed class DeleteCoverUseCase(
    IValidator<DeleteCoverRequest> validator,
    ICoverRepository coverRepository,
    IClaimRepository claimRepository,
    IClaimsUnitOfWork unitOfWork,
    IAuditQueue auditQueue,
    IDateTimeProvider dateTimeProvider) : IDeleteCoverUseCase
{
    private readonly IValidator<DeleteCoverRequest> _validator = validator;
    private readonly ICoverRepository _coverRepository = coverRepository;
    private readonly IClaimRepository _claimRepository = claimRepository;
    private readonly IClaimsUnitOfWork _unitOfWork = unitOfWork;
    private readonly IAuditQueue _auditQueue = auditQueue;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result<DeleteCoverResponse>> ExecuteAsync(
        DeleteCoverRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<DeleteCoverResponse>(
                string.Join(Environment.NewLine, validationResult.Errors.Select(error => error.ErrorMessage)));
        }

        var cover = await _coverRepository.GetByIdForUpdateAsync(
            request.Id,
            cancellationToken);
        if (cover is null)
        {
            return Result.Failure<DeleteCoverResponse>("Cover not found.");
        }

        var hasClaims = await _claimRepository.HasAnyForCoverAsync(
            cover.Id,
            cancellationToken);
        if (hasClaims)
        {
            return Result.Failure<DeleteCoverResponse>(
                "Cover cannot be deleted while claims exist.");
        }

        var auditResult = CoverAudit.Create(
            cover.Id,
            AuditHttpRequestTypes.Delete,
            _dateTimeProvider.UtcNow);
        if (auditResult.IsFailure)
        {
            return Result.Failure<DeleteCoverResponse>(auditResult.Error);
        }

        _coverRepository.Remove(cover);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditQueue.EnqueueAsync(
            auditResult.Value,
            cancellationToken);

        return new DeleteCoverResponse(cover.Id);
    }
}
