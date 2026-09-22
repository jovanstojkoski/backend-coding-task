using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Covers.Get;
using Moq;
using NUnit.Framework;

namespace Claims.UnitTests.Application;

public sealed class GetCoversUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_WhenPaginationIsInvalid_ReturnsFailureWithoutQuerying()
    {
        var repository = new Mock<ICoverRepository>();
        var useCase = new GetCoversUseCase(repository.Object);

        var result = await useCase.ExecuteAsync(
            new GetCoversRequest(1, 0),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("PageSize must be at least 1."));

        repository.Verify(
            item => item.GetPagedAsync(
                It.IsAny<Pagination>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenPaginationIsValid_PassesNormalizedPagination()
    {
        var repository = new Mock<ICoverRepository>();
        var expectedPagination = Pagination.Create(3, 25).Value;

        repository
            .Setup(item => item.GetPagedAsync(
                It.Is<Pagination>(pagination =>
                    pagination.PageNumber == 3 &&
                    pagination.PageSize == 25),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<GetCoversResponse>(
                [],
                0,
                expectedPagination.PageNumber,
                expectedPagination.PageSize));

        var useCase = new GetCoversUseCase(repository.Object);

        var result = await useCase.ExecuteAsync(
            new GetCoversRequest(3, 25),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);

        repository.Verify(
            item => item.GetPagedAsync(
                It.Is<Pagination>(pagination =>
                    pagination.PageNumber == 3 &&
                    pagination.PageSize == 25),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
