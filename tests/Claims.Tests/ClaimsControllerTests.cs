using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Claims.Tests
{
    public class ClaimsControllerTests
    {
        [Fact]
        public async Task Get_Claims()
        {
            using var application = CreateApplication();

            var client = application.CreateClient();

            var response = await client.GetAsync("/Claims", TestContext.Current.CancellationToken);

            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            Assert.Contains("items", body, StringComparison.OrdinalIgnoreCase);

            //TODO: Apart from ensuring 200 OK being returned, what else can be asserted?
        }

        [Fact]
        public async Task Create_Cover_GeneratesAnIdAndPremium()
        {
            using var application = CreateApplication();
            using var client = application.CreateClient();
            var startDate = DateTime.UtcNow.Date.AddDays(1);

            var response = await client.PostAsJsonAsync(
                "/Covers",
                new
                {
                    startDate,
                    endDate = startDate.AddDays(30),
                    type = "Yacht"
                },
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var cover = await response.Content.ReadFromJsonAsync<CreatedCoverResponse>(
                TestContext.Current.CancellationToken);

            Assert.NotNull(cover);
            Assert.False(string.IsNullOrWhiteSpace(cover.Id));
            Assert.Equal(41_250m, cover.Premium);

            var detailResponse = await client.GetAsync(
                $"/Covers/{cover.Id}",
                TestContext.Current.CancellationToken);

            var details = await detailResponse.Content.ReadFromJsonAsync<CreatedCoverResponse>(
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
            Assert.NotNull(details);
            Assert.Equal(cover.Id, details.Id);
            Assert.Equal(cover.Premium, details.Premium);

            var listResponse = await client.GetAsync(
                "/Covers?pageNumber=1&pageSize=10",
                TestContext.Current.CancellationToken);
            var list = await listResponse.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
            Assert.Contains(cover.Id, list, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Create_Claim_ForAnExistingCover_GeneratesAnId()
        {
            using var application = CreateApplication();
            using var client = application.CreateClient();
            var startDate = DateTime.UtcNow.Date.AddDays(1);

            var coverResponse = await client.PostAsJsonAsync(
                "/Covers",
                new
                {
                    startDate,
                    endDate = startDate.AddDays(30),
                    type = "Yacht"
                },
                TestContext.Current.CancellationToken);

            var cover = await coverResponse.Content.ReadFromJsonAsync<CreatedCoverResponse>(
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, coverResponse.StatusCode);
            Assert.NotNull(cover);

            var claimResponse = await client.PostAsJsonAsync(
                "/Claims",
                new
                {
                    coverId = cover.Id,
                    created = startDate.AddDays(10),
                    name = "Collision damage",
                    type = "Collision",
                    damageCost = 10_000m
                },
                TestContext.Current.CancellationToken);

            var claim = await claimResponse.Content.ReadFromJsonAsync<CreatedClaimResponse>(
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.Created, claimResponse.StatusCode);
            Assert.NotNull(claim);
            Assert.False(string.IsNullOrWhiteSpace(claim.Id));
            Assert.Equal(cover.Id, claim.CoverId);

            var detailResponse = await client.GetAsync(
                $"/Claims/{claim.Id}",
                TestContext.Current.CancellationToken);

            var details = await detailResponse.Content.ReadFromJsonAsync<CreatedClaimResponse>(
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, detailResponse.StatusCode);
            Assert.NotNull(details);
            Assert.Equal(claim.Id, details.Id);
            Assert.Equal(cover.Id, details.CoverId);

            var listResponse = await client.GetAsync(
                "/Claims?pageNumber=1&pageSize=10",
                TestContext.Current.CancellationToken);
            var list = await listResponse.Content.ReadAsStringAsync(
                TestContext.Current.CancellationToken);

            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
            Assert.Contains(claim.Id, list, StringComparison.OrdinalIgnoreCase);
        }

        private sealed record CreatedCoverResponse(string Id, decimal Premium);

        private sealed record CreatedClaimResponse(string Id, string CoverId);

        private static WebApplicationFactory<Program> CreateApplication()
        {
            return new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder => builder.UseEnvironment("Development"));
        }

    }
}
