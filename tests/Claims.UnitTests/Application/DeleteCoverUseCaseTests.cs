using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common;
using Claims.Application.UseCases.Claims.Delete;
using Claims.Application.UseCases.Claims.GetById;
using Claims.Application.UseCases.Covers.Delete;
using Claims.Domain.Auditing;
using Claims.Domain.Claim;
using Claims.Domain.Core.Abstractions;
using Claims.Domain.Cover;
using Moq;
using NUnit.Framework;

namespace Claims.UnitTests.Application;

public sealed class DeleteCoverUseCaseTests
{
    private Mock<ICoverRepository> _coverRepository = null!;
    private Mock<IAuditQueue> _auditQueue = null!;
    private Mock<IDateTimeProvider> _dateTimeProvider = null!;
    private Mock<IClaimsUnitOfWork> _claimsUnitOfWork = null!;

    [SetUp]
    public void SetUp()
    {
        _coverRepository = new Mock<ICoverRepository>();
        _auditQueue = new Mock<IAuditQueue>();
        _dateTimeProvider = new Mock<IDateTimeProvider>();
        _claimsUnitOfWork = new Mock<IClaimsUnitOfWork>();

        _dateTimeProvider
            .Setup(provider => provider.UtcNow)
            .Returns(new DateTime(2026, 1, 2));
    }

    [Test]
    public async Task ExecuteAsync_WhenRequestIsInvalid_DoesNotQueryRepository()
    {
        var result = await CreateDeleteUseCaseRequest().ExecuteAsync(
            new DeleteCoverRequest(string.Empty),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);

        _coverRepository.Verify(
            repository => repository.GetByIdAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenCoverDoesNotExist_ReturnsFailureWithoutSaving()
    {
        _coverRepository
            .Setup(repository => repository.GetByIdForUpdateAsync(
                "missing-cover",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cover?)null);

        var result = await CreateDeleteUseCaseRequest().ExecuteAsync(
            new DeleteCoverRequest("missing-cover"),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Cover not found."));

        _auditQueue.Verify(
            queue => queue.EnqueueAsync(
                It.IsAny<IAuditRecord>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenCoverExists_RemovesAndQueuesAudit()
    {
        _coverRepository
            .Setup(repository => repository.GetByIdForUpdateAsync(
                "cover-id",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateCover("cover-id"));
        _auditQueue
            .Setup(queue => queue.EnqueueAsync(
                It.IsAny<IAuditRecord>(),
                It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var result = await CreateDeleteUseCaseRequest().ExecuteAsync(
            new DeleteCoverRequest("cover-id"),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.Id, Is.EqualTo("cover-id"));

        _coverRepository.Verify(
            repository => repository.Remove(
                It.Is<Cover>(cover => cover.Id == "cover-id")),
            Times.Once);

        _auditQueue.Verify(
            queue => queue.EnqueueAsync(
                It.Is<CoverAudit>(audit => audit.CoverId == "cover-id"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private DeleteCoverUseCase CreateDeleteUseCaseRequest()
    {
        return new DeleteCoverUseCase(
            new DeleteCoverRequestValidator(),
            _coverRepository.Object,
            _auditQueue.Object,
            _dateTimeProvider.Object,
            _claimsUnitOfWork.Object);
    }

    private static Cover CreateCover(string id)
    {
        var result = Cover.Create(
            new DateTime(2026, 1, 2),
            new DateTime(2026, 2, 1),
            CoverType.Yacht,
            new DateTime(2026, 1, 1),
            new PremiumCalculator());

        var cover = result.Value;
        typeof(Cover)
            .GetProperty(nameof(Cover.Id))!
            .SetValue(cover, id);

        return cover;
    }
}
