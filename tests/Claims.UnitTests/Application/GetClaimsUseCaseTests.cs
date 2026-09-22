using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Claims.Get;
using Moq;
using NUnit.Framework;

namespace Claims.UnitTests.Application;

public sealed class GetClaimsUseCaseTests
{
    [Test]
    public async Task ExecuteAsync_WhenPaginationIsInvalid_ReturnsFailureWithoutQuerying()
    {
        var repository = new Mock<IClaimRepository>();
        var useCase = new GetClaimsUseCase(repository.Object);

        var result = await useCase.ExecuteAsync(
            new GetClaimsRequest(0, 10),
            CancellationToken.None);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Page must be at least 1."));
        repository.Verify(
            item => item.GetPagedAsync(
                It.IsAny<Pagination>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Test]
    public async Task ExecuteAsync_WhenPaginationIsValid_PassesNormalizedPagination()
    {
        var repository = new Mock<IClaimRepository>();
        var expectedPagination = Pagination.Create(2, 1_000).Value;
        repository
            .Setup(item => item.GetPagedAsync(
                It.Is<Pagination>(pagination =>
                    pagination.PageNumber == 2 &&
                    pagination.PageSize == Pagination.MaxPageSize),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<GetClaimsResponse>(
                [],
                0,
                expectedPagination.PageNumber,
                expectedPagination.PageSize));

        var useCase = new GetClaimsUseCase(repository.Object);

        var result = await useCase.ExecuteAsync(
            new GetClaimsRequest(2, 1_000),
            CancellationToken.None);

        Assert.That(result.IsSuccess, Is.True);
        repository.Verify(
            item => item.GetPagedAsync(
                It.Is<Pagination>(pagination =>
                    pagination.PageNumber == 2 &&
                    pagination.PageSize == Pagination.MaxPageSize),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
