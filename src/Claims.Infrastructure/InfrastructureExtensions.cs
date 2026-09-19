using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common;
using Claims.Infrastructure.Common;
using Claims.Infrastructure.Data;
using Claims.Infrastructure.Data.Queues;
using Claims.Infrastructure.Data.Repositories;
using Claims.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Runtime.InteropServices;
using Testcontainers.MongoDb;
using Testcontainers.MsSql;

namespace Claims.Infrastructure;

public static class InfrastructureExtensions
{
    public static async Task<IServiceCollection> AddInfrastructureAsync(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var databaseSettings = await ResolveDatabaseSettingsAsync(
            configuration,
            environment);

        services
            .AddOptions<ConnectionStringsOptions>()
            .Configure(options => options.AuditDatabase = databaseSettings.AuditConnectionString)
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.AuditDatabase),
                "The audit database connection string is required.")
            .ValidateOnStart();

        services
            .AddOptions<MongoDbOptions>()
            .Configure(options =>
            {
                options.ConnectionString = databaseSettings.MongoConnectionString;
                options.DatabaseName = databaseSettings.MongoDatabaseName;
            })
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.ConnectionString),
                "The MongoDB connection string is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.DatabaseName),
                "The MongoDB database name is required.")
            .ValidateOnStart();

        services
            .AddSqlServerDb()
            .AddMongoDb()
            .AddInfrastructureHealthChecks()
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .AddRepositories()
            .AddAuditProcessing(configuration);

        return services;
    }

    public static IServiceCollection AddInfrastructureHealthChecks(
        this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddDbContextCheck<AuditContext>("audit-database")
            .AddDbContextCheck<ClaimsContext>("claims-database");

        return services;
    }

    public static IServiceCollection AddAuditProcessing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<AuditQueueOptions>()
            .Bind(configuration.GetSection(AuditQueueOptions.SectionName))
            .Validate(
                options => options.Capacity > 0,
                "Audit queue capacity must be greater than zero.")
            .ValidateOnStart();

        return services
            .AddSingleton<IAuditQueue, AuditQueue>()
            .AddHostedService<AuditBackgroundService>();
    }

    public static IServiceCollection AddSqlServerDb(
        this IServiceCollection services)
    {
        services.AddDbContext<AuditContext>((serviceProvider, options) =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<ConnectionStringsOptions>>()
                .Value;

            options.UseSqlServer(
                settings.AuditDatabase,
                sqlOptions => sqlOptions.MigrationsAssembly(
                    typeof(AuditContext).Assembly.FullName));
        });

        return services;
    }

    public static IServiceCollection AddMongoDb(
        this IServiceCollection services)
    {
        services.AddSingleton<IMongoClient>(serviceProvider =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<MongoDbOptions>>()
                .Value;

            return new MongoClient(settings.ConnectionString);
        });

        services.AddDbContext<ClaimsContext>((serviceProvider, options) =>
        {
            var settings = serviceProvider
                .GetRequiredService<IOptions<MongoDbOptions>>()
                .Value;
            var client = serviceProvider.GetRequiredService<IMongoClient>();

            options.UseMongoDB(client, settings.DatabaseName);
        });

        services.AddScoped<IClaimsUnitOfWork>(serviceProvider =>
            serviceProvider.GetRequiredService<ClaimsContext>());

        return services;
    }

    public static async Task MigrateSqlServerDbAsync(
        this IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AuditContext>();

        await context.Database.MigrateAsync();
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<ICoverRepository, CoverRepository>();

        return services;
    }

    private static async Task<DatabaseSettings> ResolveDatabaseSettingsAsync(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var connectionStrings = configuration
            .GetSection(ConnectionStringsOptions.SectionName)
            .Get<ConnectionStringsOptions>()
            ?? new ConnectionStringsOptions();
        var mongoDb = configuration
            .GetSection(MongoDbOptions.SectionName)
            .Get<MongoDbOptions>()
            ?? new MongoDbOptions();

        string auditConnectionString;
        string mongoConnectionString;

        if (environment.IsDevelopment())
        {
            var sqlContainer = (
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux)
                    ? new MsSqlBuilder()
                        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                    : new MsSqlBuilder())
                .Build();

            var mongoContainer = new MongoDbBuilder()
                .WithImage("mongo:latest")
                .Build();

            await sqlContainer.StartAsync();
            await mongoContainer.StartAsync();

            auditConnectionString = sqlContainer.GetConnectionString();
            mongoConnectionString = mongoContainer.GetConnectionString();
        }
        else
        {
            auditConnectionString = RequireValue(
                connectionStrings.AuditDatabase,
                "ConnectionStrings:AuditDatabase",
                "outside Development");

            mongoConnectionString = RequireValue(
                mongoDb.ConnectionString,
                "MongoDb:ConnectionString",
                "outside Development");
        }

        var mongoDatabaseName = RequireValue(
            mongoDb.DatabaseName,
            "MongoDb:DatabaseName",
            "in all environments");

        return new DatabaseSettings(
            auditConnectionString,
            mongoConnectionString,
            mongoDatabaseName);
    }

    private static string RequireValue(
        string value,
        string configurationKey,
        string environmentScope)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"The '{configurationKey}' configuration value is required {environmentScope}.");
        }

        return value;
    }

    private sealed record DatabaseSettings(
        string AuditConnectionString,
        string MongoConnectionString,
        string MongoDatabaseName);
}
