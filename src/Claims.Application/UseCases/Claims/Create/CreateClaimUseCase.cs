using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common;
using Claims.Domain.Auditing;
using Claims.Domain.Claim;
using Claims.Domain.Core.Primitives;
using FluentValidation;

namespace Claims.Application.UseCases.Claims.Create;

internal sealed class CreateClaimUseCase(
    IValidator<CreateClaimRequest> validator,
    IClaimRepository claimRepository,
    ICoverRepository coverRepository,
    IClaimsUnitOfWork unitOfWork,
    IAuditQueue auditQueue,
    IDateTimeProvider dateTimeProvider) : ICreateClaimUseCase
{
    private readonly IValidator<CreateClaimRequest> _validator = validator;
    private readonly IClaimRepository _claimRepository = claimRepository;
    private readonly ICoverRepository _coverRepository = coverRepository;
    private readonly IClaimsUnitOfWork _unitOfWork = unitOfWork;
    private readonly IAuditQueue _auditQueue = auditQueue;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public async Task<Result<CreateClaimResponse>> ExecuteAsync(
        CreateClaimRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<CreateClaimResponse>(
                string.Join(Environment.NewLine, validationResult.Errors.Select(error => error.ErrorMessage)));
        }

        var cover = await _coverRepository.GetByIdAsync(request.CoverId, cancellationToken);
        if (cover is null)
        {
            return Result.Failure<CreateClaimResponse>("Cover not found.");
        }

        var claimResult = Claim.Create(
            request.CoverId,
            request.Created,
            request.Name,
            request.Type,
            request.DamageCost,
            cover.StartDate,
            cover.EndDate);

        if (claimResult.IsFailure)
        {
            return Result.Failure<CreateClaimResponse>(claimResult.Error);
        }

        var claim = claimResult.Value;
        _claimRepository.AddItem(claim);

        var auditResult = ClaimAudit.Create(
            claim.Id,
            "POST",
            _dateTimeProvider.UtcNow);
        if (auditResult.IsFailure)
        {
            return Result.Failure<CreateClaimResponse>(auditResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _auditQueue.Enqueue(auditResult.Value);

        return new CreateClaimResponse(
            claim.Id,
            claim.CoverId,
            claim.Created,
            claim.Name,
            claim.Type,
            claim.DamageCost);
    }
}
