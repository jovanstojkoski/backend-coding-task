using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common;
using Claims.Domain.Core.Abstractions;
using Claims.Infrastructure.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
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

                    services.RemoveAll<IAuditQueue>();
                    services.AddSingleton<IAuditQueue, NoOpAuditQueue>();
                });
            });
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        Application.Dispose();
    }

    public static async Task ResetDatabaseAsync()
    {
        using var client = Application.CreateClient();
        await using var scope = Application.Services.CreateAsyncScope();

        var settings = scope.ServiceProvider
            .GetRequiredService<IOptions<MongoDbOptions>>()
            .Value;

        var mongoClient = scope.ServiceProvider
            .GetRequiredService<IMongoClient>();

        var database = mongoClient.GetDatabase(settings.DatabaseName);

        await database
            .GetCollection<BsonDocument>("claims")
            .DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);

        await database
            .GetCollection<BsonDocument>("covers")
            .DeleteManyAsync(FilterDefinition<BsonDocument>.Empty);
    }

    private sealed class FixedDateTimeProvider(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }

    public sealed class NoOpAuditQueue : IAuditQueue
    {
        public ValueTask EnqueueAsync(
            IAuditRecord auditRecord,
            CancellationToken cancellationToken)
        {
            return ValueTask.CompletedTask;
        }

        public async IAsyncEnumerable<IAuditRecord> ReadAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            yield break;
        }
    }
}
