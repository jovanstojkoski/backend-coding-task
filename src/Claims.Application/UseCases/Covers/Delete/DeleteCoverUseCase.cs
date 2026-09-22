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
    IAuditQueue auditQueue,
    IDateTimeProvider dateTimeProvider,
    IClaimsUnitOfWork claimsUnitOfWork) : IDeleteCoverUseCase
{
    private readonly IValidator<DeleteCoverRequest> _validator = validator;
    private readonly ICoverRepository _coverRepository = coverRepository;
    private readonly IAuditQueue _auditQueue = auditQueue;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;
    private readonly IClaimsUnitOfWork _claimsUnitOfWork = claimsUnitOfWork;

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

        var cover = await _coverRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (cover is null)
        {
            return Result.Failure<DeleteCoverResponse>("Cover not found.");
        }

        _coverRepository.Remove(cover);
        await _claimsUnitOfWork.SaveChangesAsync(cancellationToken);

        var auditResult = CoverAudit.Create(
            cover.Id,
            AuditHttpRequestTypes.Delete,
            _dateTimeProvider.UtcNow);

        if (auditResult.IsFailure)
        {
            return Result.Failure<DeleteCoverResponse>(auditResult.Error);
        }

        await _auditQueue.EnqueueAsync(
            auditResult.Value,
            cancellationToken);

        return new DeleteCoverResponse(cover.Id);
    }
}
