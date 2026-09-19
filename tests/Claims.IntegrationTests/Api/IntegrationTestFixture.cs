using System.Text.Json;
using System.Text.Json.Serialization;
using Claims.Application.Abstractions.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NUnit.Framework;

namespace Claims.IntegrationTests.Api;

[SetUpFixture]
public sealed class IntegrationTestFixture
{
    public static readonly DateTime CurrentDate = new(2026, 1, 1);

    public static JsonSerializerOptions JsonOptions { get; } =
        new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

    public static WebApplicationFactory<Program> Application { get; private set; } = null!;

    [OneTimeSetUp]
    public void SetUp()
    {
        Application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.ConfigureServices(services =>
                {
                    services.RemoveAll<IDateTimeProvider>();
                    services.AddSingleton<IDateTimeProvider>(
                        new FixedDateTimeProvider(CurrentDate));
                });
            });
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Application.Dispose();
    }

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
