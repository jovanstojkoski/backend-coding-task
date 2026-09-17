using Claims.Application.Abstractions;
using Claims.Application.Abstractions.Common;
using Claims.Infrastructure.Common;
using Claims.Infrastructure.Data;
using Claims.Infrastructure.Data.Queues;
using Claims.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
            auditConnectionString = configuration.GetConnectionString("AuditDatabase")
                ?? throw new InvalidOperationException(
                    "The 'ConnectionStrings:AuditDatabase' configuration value is required outside Development.");

            mongoConnectionString = configuration["MongoDb:ConnectionString"]
                ?? throw new InvalidOperationException(
                    "The 'MongoDb:ConnectionString' configuration value is required outside Development.");
        }

        var mongoDatabaseName = configuration["MongoDb:DatabaseName"]
            ?? throw new InvalidOperationException(
                "The 'MongoDb:DatabaseName' configuration value is required.");

        services
            .AddSqlServerDb(auditConnectionString)
            .AddMongoDb(mongoConnectionString, mongoDatabaseName)
            .AddSingleton<IDateTimeProvider, DateTimeProvider>()
            .AddRepositories()
            .AddScoped<IClaimsUnitOfWork>(serviceProvider =>
                serviceProvider.GetRequiredService<ClaimsContext>())
            .AddSingleton<IAuditQueue, AuditQueue>()
            .AddHostedService<AuditBackgroundService>();

        return services;
    }

    public static IServiceCollection AddSqlServerDb(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AuditContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(
                    typeof(AuditContext).Assembly.FullName)));

        return services;
    }

    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        string connectionString,
        string databaseName)
    {
        services.AddDbContext<ClaimsContext>(options =>
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);

            options.UseMongoDB(
                database.Client,
                database.DatabaseNamespace.DatabaseName);
        });

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
}
