using System.Net;
using System.Net.Http.Json;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Covers.Get;
using Claims.Domain.Cover;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Claims.IntegrationTests.Api;

public sealed class CoversControllerTests : ApiIntegrationTestBase
{
    private const string ControllerRoute = "/covers";

    [Test]
    public async Task ComputePremium_ReturnsPremiumForValidRequest()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.PostAsync(
            $"{ControllerRoute}/compute?startDate=2026-01-02&endDate=2026-02-01&type={CoverType.Yacht}",
            new StringContent(string.Empty),
            CancellationToken.None);

        var premium = await response.Content.ReadFromJsonAsync<decimal>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(premium, Is.EqualTo(41_250m));
    }

    [Test]
    public async Task ComputePremium_ReturnsBadRequestForInvalidDates()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.PostAsync(
            $"{ControllerRoute}/compute?startDate=2026-01-02&endDate=2026-01-01&type={CoverType.Yacht}",
            new StringContent(string.Empty),
            CancellationToken.None);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Title, Is.EqualTo("Premium Calculation Error"));
    }

    [Test]
    public async Task Create_ReturnsCreatedCover()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.PostAsJsonAsync(
            ControllerRoute,
            CreateCoverRequest(),
            IntegrationTestFixture.JsonOptions,
            CancellationToken.None);

        var cover = await response.Content.ReadFromJsonAsync<CreatedCoverResponse>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.Location, Is.Not.Null);
        Assert.That(cover, Is.Not.Null);
        Assert.That(
            response.Headers.Location!.AbsolutePath,
            Is.EqualTo($"{ControllerRoute}/{cover!.Id}"));
        Assert.That(cover!.Id, Is.Not.Null.And.Not.Empty);
        Assert.That(cover.Premium, Is.EqualTo(41_250m));
    }

    [Test]
    public async Task Create_ReturnsBadRequestForInvalidDates()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.PostAsJsonAsync(
            ControllerRoute,
            new
            {
                startDate = IntegrationTestFixture.CurrentDate.AddDays(-1),
                endDate = IntegrationTestFixture.CurrentDate.AddDays(10),
                type = CoverType.Yacht
            },
            IntegrationTestFixture.JsonOptions,
            CancellationToken.None);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Title, Is.EqualTo("Cover Creation Error"));
    }

    [Test]
    public async Task GetAll_ReturnsCreatedCover()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();
        var createdCover = await CreateCoverAsync(client);

        var response = await client.GetAsync(
            $"{ControllerRoute}?pageNumber=1&pageSize=100",
            CancellationToken.None);

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(body, Does.Contain(createdCover.Id));
    }

    [Test]
    public async Task GetAll_ReturnsBadRequestForInvalidPagination()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.GetAsync(
            $"{ControllerRoute}?pageNumber=0&pageSize=10",
            CancellationToken.None);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Title, Is.EqualTo("Covers Retrieval Error"));
    }

    [Test]
    public async Task GetById_ReturnsExistingCover()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();
        var createdCover = await CreateCoverAsync(client);

        var response = await client.GetAsync(
            $"{ControllerRoute}/{createdCover.Id}",
            CancellationToken.None);

        var cover = await response.Content.ReadFromJsonAsync<CreatedCoverResponse>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(cover, Is.Not.Null);
        Assert.That(cover!.Id, Is.EqualTo(createdCover.Id));
        Assert.That(cover.Premium, Is.EqualTo(createdCover.Premium));
    }

    [Test]
    public async Task GetById_ReturnsNotFoundForMissingCover()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.GetAsync(
            $"{ControllerRoute}/missing-cover-id",
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ReturnsNoContentForExistingCover()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();
        var createdCover = await CreateCoverAsync(client);

        var response = await client.DeleteAsync(
            $"{ControllerRoute}/{createdCover.Id}",
            CancellationToken.None);

        var getResponse = await client.GetAsync(
            $"{ControllerRoute}/{createdCover.Id}",
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ReturnsBadRequestForMissingCover()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.DeleteAsync(
            $"{ControllerRoute}/missing-cover-id",
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Delete_ReturnsBadRequestWhenCoverHasClaims()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();
        var cover = await CreateCoverAsync(client);
        var startDate = IntegrationTestFixture.CurrentDate.AddDays(1);

        var claimResponse = await client.PostAsJsonAsync(
            "/claims",
            new
            {
                coverId = cover.Id,
                created = startDate.AddDays(10),
                name = "Collision damage",
                type = Claims.Domain.Claim.ClaimType.Collision,
                damageCost = 10_000m
            },
            IntegrationTestFixture.JsonOptions,
            CancellationToken.None);

        Assert.That(claimResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var response = await client.DeleteAsync(
            $"{ControllerRoute}/{cover.Id}",
            CancellationToken.None);

        var getResponse = await client.GetAsync(
            $"{ControllerRoute}/{cover.Id}",
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    private static object CreateCoverRequest()
    {
        var startDate = IntegrationTestFixture.CurrentDate.AddDays(1);

        return new
        {
            startDate,
            endDate = startDate.AddDays(30),
            type = CoverType.Yacht
        };
    }

    private static async Task<CreatedCoverResponse> CreateCoverAsync(
        HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            ControllerRoute,
            CreateCoverRequest(),
            IntegrationTestFixture.JsonOptions,
            CancellationToken.None);

        var cover = await response.Content.ReadFromJsonAsync<CreatedCoverResponse>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(cover, Is.Not.Null);

        return cover!;
    }

    private sealed record CreatedCoverResponse(string Id, decimal Premium);
}
