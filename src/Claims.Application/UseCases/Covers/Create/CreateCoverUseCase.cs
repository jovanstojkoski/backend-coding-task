using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common;
using Claims.Domain.Auditing;
using Claims.Domain.Core.Primitives;
using Claims.Domain.Cover;
using FluentValidation;

namespace Claims.Application.UseCases.Covers.Create;

internal sealed class CreateCoverUseCase(
    IValidator<CreateCoverRequest> validator,
    ICoverRepository coverRepository,
    IClaimsUnitOfWork unitOfWork,
    IAuditQueue auditQueue,
    IDateTimeProvider dateTimeProvider) : ICreateCoverUseCase
{
    private readonly IValidator<CreateCoverRequest> _validator = validator;
    private readonly ICoverRepository _coverRepository = coverRepository;
    private readonly IClaimsUnitOfWork _unitOfWork = unitOfWork;
    private readonly IAuditQueue _auditQueue = auditQueue;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result<CreateCoverResponse>> ExecuteAsync(
        CreateCoverRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<CreateCoverResponse>(
                string.Join(Environment.NewLine, validationResult.Errors.Select(error => error.ErrorMessage)));
        }

        var coverResult = Cover.Create(
            request.StartDate,
            request.EndDate,
            request.Type,
            _dateTimeProvider.UtcNow);

        if (coverResult.IsFailure)
        {
            return Result.Failure<CreateCoverResponse>(coverResult.Error);
        }

        var cover = coverResult.Value;

        _coverRepository.AddItem(cover);

        var auditResult = CoverAudit.Create(
            cover.Id,
            "POST",
            _dateTimeProvider.UtcNow);

        if (auditResult.IsFailure)
        {
            return Result.Failure<CreateCoverResponse>(auditResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _auditQueue.Enqueue(auditResult.Value);

        return new CreateCoverResponse(
            cover.Id,
            cover.StartDate,
            cover.EndDate,
            cover.Type,
            cover.Premium);
    }
}
