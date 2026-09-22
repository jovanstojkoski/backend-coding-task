using Claims.Application.Abstractions.Common.Models;
using NUnit.Framework;

namespace Claims.UnitTests.Application;

public sealed class PaginationTests
{
    [Test]
    public void Create_WhenValuesAreMissing_UsesDefaults()
    {
        var result = Pagination.Create();

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.PageNumber, Is.EqualTo(Pagination.DefaultPage));
        Assert.That(result.Value.PageSize, Is.EqualTo(Pagination.DefaultPageSize));
        Assert.That(result.Value.Skip, Is.Zero);
        Assert.That(result.Value.Take, Is.EqualTo(Pagination.DefaultPageSize));
    }

    [Test]
    public void Create_WhenPageSizeExceedsMaximum_CapsPageSize()
    {
        var result = Pagination.Create(2, Pagination.MaxPageSize + 1);

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value.PageNumber, Is.EqualTo(2));
        Assert.That(result.Value.PageSize, Is.EqualTo(Pagination.MaxPageSize));
        Assert.That(
            result.Value.Skip,
            Is.EqualTo(Pagination.MaxPageSize));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Create_WhenPageIsInvalid_ReturnsFailure(int pageNumber)
    {
        var result = Pagination.Create(pageNumber, 10);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("Page must be at least 1."));
    }

    [TestCase(0)]
    [TestCase(-1)]
    public void Create_WhenPageSizeIsInvalid_ReturnsFailure(int pageSize)
    {
        var result = Pagination.Create(1, pageSize);

        Assert.That(result.IsFailure, Is.True);
        Assert.That(result.Error, Is.EqualTo("PageSize must be at least 1."));
    }
}
