using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common;
using Claims.Application.UseCases.Claims.Delete;
using Claims.Domain.Auditing;
using Claims.Domain.Claim;
using Claims.Domain.Core.Abstractions;
using Moq;
using NUnit.Framework;

namespace Claims.UnitTests.Application;

public sealed class DeleteClaimUseCaseTests
{
    private Mock<IClaimRepository> _claimRepository = null!;
    private Mock<IClaimsUnitOfWork> _unitOfWork = null!;
    private Mock<IAuditQueue> _auditQueue = null!;
    private Mock<IDateTimeProvider> _dateTimeProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _claimRepository = new Mock<IClaimRepository>();
        _unitOfWork = new Mock<IClaimsUnitOfWork>();
        _auditQueue = new Mock<IAuditQueue>();
        _dateTimeProvider = new Mock<IDateTimeProvider>();

        _dateTimeProvider
            .Setup(provider => provider.UtcNow)
            .Returns(new DateTime(2026, 1, 2));
    }

    [Test]
    public async Task ExecuteAsync_WhenRequestIsInvalid_DoesNotQueryRepository()
    {
        var result = await CreateSut().ExecuteAsync(
            new DeleteClaimRequest(string.Empty),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);

        _claimRepository.Verify(
            repository => repository.GetByIdForUpdateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenClaimDoesNotExist_ReturnsFailureWithoutSaving()
    {
        _claimRepository
            .Setup(repository => repository.GetByIdForUpdateAsync(
                "missing-claim",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Claim?)null);

        var result = await CreateSut().ExecuteAsync(
            new DeleteClaimRequest("missing-claim"),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Claim not found."));

        _auditQueue.Verify(
            queue => queue.EnqueueAsync(
                It.IsAny<IAuditRecord>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenClaimExists_RemovesAndQueuesAudit()
    {
        _claimRepository
            .Setup(repository => repository.GetByIdForUpdateAsync(
                "claim-id",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateClaim("claim-id"));

        _claimRepository
            .Setup(repository => repository.Remove(It.IsAny<Claim>()));

        _unitOfWork
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _auditQueue
            .Setup(queue => queue.EnqueueAsync(
                It.IsAny<IAuditRecord>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var result = await CreateSut().ExecuteAsync(
            new DeleteClaimRequest("claim-id"),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo("claim-id"));

        _claimRepository.Verify(
            repository => repository.Remove(
                It.Is<Claim>(claim => claim.Id == "claim-id")),
            Times.Once);

        _unitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);

        _auditQueue.Verify(
            queue => queue.EnqueueAsync(
                It.Is<ClaimAudit>(audit => audit.ClaimId == "claim-id"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private DeleteClaimUseCase CreateSut()
    {
        return new DeleteClaimUseCase(
            new DeleteClaimRequestValidator(),
            _claimRepository.Object,
            _unitOfWork.Object,
            _auditQueue.Object,
            _dateTimeProvider.Object);
    }

    private static Claim CreateClaim(string id)
    {
        var result = Claim.Create(
            "cover-id",
            new DateTime(2026, 1, 1),
            "Collision damage",
            ClaimType.Collision,
            10_000m,
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31));

        var claim = result.Value;
        typeof(Claim)
            .GetProperty(nameof(Claim.Id))!
            .SetValue(claim, id);

        return claim;
    }
}
