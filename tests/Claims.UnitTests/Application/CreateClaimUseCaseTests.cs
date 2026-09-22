using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common;
using Claims.Application.UseCases.Claims.Create;
using Claims.Application.UseCases.Covers.GetById;
using Claims.Domain.Auditing;
using Claims.Domain.Claim;
using Claims.Domain.Cover;
using Claims.Domain.Core.Abstractions;
using Moq;
using NUnit.Framework;

namespace Claims.UnitTests.Application;

public sealed class CreateClaimUseCaseTests
{
    private Mock<ICoverRepository> _coverRepository = null!;
    private Mock<IClaimRepository> _claimRepository = null!;
    private Mock<IClaimsUnitOfWork> _unitOfWork = null!;
    private Mock<IAuditQueue> _auditQueue = null!;
    private Mock<IDateTimeProvider> _dateTimeProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _coverRepository = new Mock<ICoverRepository>();
        _claimRepository = new Mock<IClaimRepository>();
        _unitOfWork = new Mock<IClaimsUnitOfWork>();
        _auditQueue = new Mock<IAuditQueue>();
        _dateTimeProvider = new Mock<IDateTimeProvider>();

        _dateTimeProvider
            .Setup(provider => provider.UtcNow)
            .Returns(new DateTime(2026, 1, 2));
    }

    [Test]
    public async Task ExecuteAsync_WhenCoverDoesNotExist_ReturnsFailureWithoutSaving()
    {
        _coverRepository
            .Setup(repository => repository.GetByIdAsync(
                "missing-cover",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((GetCoverResponse?)null);

        var result = await CreateSut().ExecuteAsync(
            new CreateClaimRequest(
                "missing-cover",
                new DateTime(2026, 1, 10),
                "Collision damage",
                ClaimType.Collision,
                10_000m),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Cover not found."));

        _unitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        _auditQueue.Verify(
            queue => queue.EnqueueAsync(
                It.IsAny<IAuditRecord>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenRequestIsInvalid_DoesNotQueryRepositories()
    {
        var result = await CreateSut().ExecuteAsync(
            new CreateClaimRequest(
                string.Empty,
                default,
                string.Empty,
                (ClaimType)999,
                0m),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);

        _coverRepository.Verify(
            repository => repository.GetByIdForUpdateAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _unitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenClaimIsOutsideCover_ReturnsFailureWithoutSaving()
    {
        var startDate = new DateTime(2026, 1, 1);
        _coverRepository
            .Setup(repository => repository.GetByIdAsync(
                "cover-id",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCoverResponse(
                "cover-id",
                startDate,
                startDate.AddDays(30),
                CoverType.Yacht,
                41_250m));

        var result = await CreateSut().ExecuteAsync(
            new CreateClaimRequest(
                "cover-id",
                startDate.AddDays(31),
                "Collision damage",
                ClaimType.Collision,
                10_000m),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(
            result.Error,
            Is.EqualTo("Claim date must be within the cover period."));

        _claimRepository.Verify(
            repository => repository.AddItem(It.IsAny<Claim>()),
            Times.Never);
        _unitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenValid_SavesAndQueuesAudit()
    {
        var startDate = new DateTime(2026, 1, 1);
        _coverRepository
            .Setup(repository => repository.GetByIdAsync(
                "cover-id",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetCoverResponse(
                "cover-id",
                startDate,
                startDate.AddDays(30),
                CoverType.Yacht,
                41_250m));

        _claimRepository
            .Setup(repository => repository.AddItem(It.IsAny<Claim>()))
            .Callback<Claim>(claim =>
                typeof(Claim)
                    .GetProperty(nameof(Claim.Id))!
                    .SetValue(claim, "claim-id"));

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
            new CreateClaimRequest(
                "cover-id",
                startDate.AddDays(5),
                "Collision damage",
                ClaimType.Collision,
                10_000m),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);

        _claimRepository.Verify(
            repository => repository.AddItem(It.IsAny<Claim>()),
            Times.Once);
        _unitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        _auditQueue.Verify(
            queue => queue.EnqueueAsync(
                It.Is<ClaimAudit>(audit => audit.ClaimId == result.Value.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private CreateClaimUseCase CreateSut()
    {
        return new CreateClaimUseCase(
            new CreateClaimRequestValidator(),
            _claimRepository.Object,
            _coverRepository.Object,
            _unitOfWork.Object,
            _auditQueue.Object,
            _dateTimeProvider.Object);
    }
}
