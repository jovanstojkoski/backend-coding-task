using System.Net;
using System.Net.Http.Json;
using Claims.Application.Abstractions.Common.Models;
using Claims.Application.UseCases.Claims.Get;
using Claims.Domain.Claim;
using Claims.Domain.Cover;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Claims.IntegrationTests.Api;

public sealed class ClaimsControllerTests : ApiIntegrationTestBase
{
    private const string ControllerRoute = "/claims";
    private const string CoversRoute = "/covers";

    [Test]
    public async Task Create_ReturnsCreatedClaim()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();
        var cover = await CreateCoverAsync(client);

        var response = await client.PostAsJsonAsync(
            ControllerRoute,
            CreateClaimRequest(cover.Id),
            IntegrationTestFixture.JsonOptions,
            CancellationToken.None);

        var claim = await response.Content.ReadFromJsonAsync<CreatedClaimResponse>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response.Headers.Location, Is.Not.Null);
        Assert.That(claim, Is.Not.Null);
        Assert.That(
            response.Headers.Location!.AbsolutePath,
            Is.EqualTo($"{ControllerRoute}/{claim!.Id}"));
        Assert.That(claim!.Id, Is.Not.Null.And.Not.Empty);
        Assert.That(claim.CoverId, Is.EqualTo(cover.Id));
    }

    [Test]
    public async Task Create_ReturnsBadRequestForInvalidRequest()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.PostAsJsonAsync(
            ControllerRoute,
            new
            {
                coverId = string.Empty,
                created = DateTime.MinValue,
                name = string.Empty,
                type = ClaimType.Collision,
                damageCost = 0m
            },
            IntegrationTestFixture.JsonOptions,
            CancellationToken.None);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Title, Is.EqualTo("Claim Creation Error"));
    }

    [Test]
    public async Task GetAll_ReturnsCreatedClaim()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();
        var createdClaim = await CreateClaimAsync(client);

        var response = await client.GetAsync(
            $"{ControllerRoute}?pageNumber=1&pageSize=10",
            CancellationToken.None);

        var body = await response.Content.ReadAsStringAsync(CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(body, Does.Contain(createdClaim.Id));
    }

    [Test]
    public async Task GetAll_ReturnsBadRequestForInvalidPagination()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.GetAsync(
            $"{ControllerRoute}?pageNumber=1&pageSize=0",
            CancellationToken.None);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(problem, Is.Not.Null);
        Assert.That(problem!.Title, Is.EqualTo("Claims Retrieval Error"));
    }

    [Test]
    public async Task GetById_ReturnsExistingClaim()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();
        var createdClaim = await CreateClaimAsync(client);

        var response = await client.GetAsync(
            $"{ControllerRoute}/{createdClaim.Id}",
            CancellationToken.None);

        var claim = await response.Content.ReadFromJsonAsync<CreatedClaimResponse>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(claim, Is.Not.Null);
        Assert.That(claim!.Id, Is.EqualTo(createdClaim.Id));
        Assert.That(claim.CoverId, Is.EqualTo(createdClaim.CoverId));
    }

    [Test]
    public async Task GetById_ReturnsNotFoundForMissingClaim()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.GetAsync(
            $"{ControllerRoute}/missing-claim-id",
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ReturnsNoContentForExistingClaim()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();
        var createdClaim = await CreateClaimAsync(client);

        var response = await client.DeleteAsync(
            $"{ControllerRoute}/{createdClaim.Id}",
            CancellationToken.None);

        var getResponse = await client.GetAsync(
            $"{ControllerRoute}/{createdClaim.Id}",
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task Delete_ReturnsNotFoundForMissingClaim()
    {
        using var client = IntegrationTestFixture.Application.CreateClient();

        var response = await client.DeleteAsync(
            $"{ControllerRoute}/missing-claim-id",
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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

    private static object CreateClaimRequest(string coverId)
    {
        var startDate = IntegrationTestFixture.CurrentDate.AddDays(1);

        return new
        {
            coverId,
            created = startDate.AddDays(10),
            name = "Collision damage",
            type = ClaimType.Collision,
            damageCost = 10_000m
        };
    }

    private static async Task<CreatedCoverResponse> CreateCoverAsync(
        HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            CoversRoute,
            CreateCoverRequest(),
            IntegrationTestFixture.JsonOptions,
            CancellationToken.None);

        var cover = await response.Content.ReadFromJsonAsync<CreatedCoverResponse>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(cover, Is.Not.Null);

        return cover!;
    }

    private static async Task<CreatedClaimResponse> CreateClaimAsync(
        HttpClient client)
    {
        var cover = await CreateCoverAsync(client);

        var response = await client.PostAsJsonAsync(
            ControllerRoute,
            CreateClaimRequest(cover.Id),
            IntegrationTestFixture.JsonOptions,
            CancellationToken.None);

        var claim = await response.Content.ReadFromJsonAsync<CreatedClaimResponse>(
            CancellationToken.None);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(claim, Is.Not.Null);

        return claim!;
    }

    private sealed record CreatedCoverResponse(string Id, decimal Premium);

    private sealed record CreatedClaimResponse(string Id, string CoverId);
}
