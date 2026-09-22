using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Audit;
using Claims.Application.Abstractions.Common;
using Claims.Domain.Auditing;
using Claims.Domain.Core.Primitives;
using FluentValidation;

namespace Claims.Application.UseCases.Claims.Delete;

internal sealed class DeleteClaimUseCase(
    IValidator<DeleteClaimRequest> validator,
    IClaimRepository claimRepository,
    IAuditQueue auditQueue,
    IDateTimeProvider dateTimeProvider,
    IClaimsUnitOfWork claimsUnitOfWork) : IDeleteClaimUseCase
{
    private readonly IValidator<DeleteClaimRequest> _validator = validator;
    private readonly IClaimRepository _claimRepository = claimRepository;
    private readonly IAuditQueue _auditQueue = auditQueue;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IClaimsUnitOfWork _claimsUnitOfWork = claimsUnitOfWork;

    public async Task<Result<DeleteClaimResponse>> ExecuteAsync(
        DeleteClaimRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<DeleteClaimResponse>(
                string.Join(Environment.NewLine, validationResult.Errors.Select(error => error.ErrorMessage)));
        }

        var claim = await _claimRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (claim is null)
        {
            return Result.Failure<DeleteClaimResponse>("Claim not found.");
        }

        _claimRepository.Remove(claim);
        await _claimsUnitOfWork.SaveChangesAsync(cancellationToken);

        var auditResult = ClaimAudit.Create(
            claim.Id,
            AuditHttpRequestTypes.Delete,
            _dateTimeProvider.UtcNow);
        if (auditResult.IsFailure)
        {
            return Result.Failure<DeleteClaimResponse>(auditResult.Error);
        }

        await _auditQueue.EnqueueAsync(
            auditResult.Value,
            cancellationToken);

        return new DeleteClaimResponse(claim.Id);
    }
}
