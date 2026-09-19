# Claims API

Backend API for managing insurance covers and claims.

## Architecture

- `Claims.API` contains HTTP controllers and application composition.
- `Claims.Application` contains use cases and application abstractions.
- `Claims.Domain` contains entities and business rules.
- `Claims.Infrastructure` contains database, repository, queue, and background-service implementations.

The API stores claims and covers in MongoDB. Audit records are persisted to SQL Server asynchronously through an in-memory background queue.

## Prerequisites

- .NET 9 SDK
- Docker Desktop or another Docker daemon

Development mode starts MongoDB and SQL Server through Testcontainers, so Docker must be running.

## Run locally

```bash
dotnet run --project src/Claims.API/Claims.API.csproj
```

Swagger is available at `/swagger`.

The health endpoint is available at `/health`.

On startup, pending migrations are applied to the SQL Server audit database.
The `/health` endpoint checks both the audit SQL Server database and the claims MongoDB database.

In Development, SQL Server and MongoDB are started automatically through Testcontainers.

## Test

```bash
dotnet test Claims.sln --configuration Release
```

The test suite uses NUnit and Moq. Unit tests are in `tests/Claims.UnitTests`, while API and infrastructure integration tests are in `tests/Claims.IntegrationTests`.

## Production configuration

The following configuration values are required outside the Development environment:

```text
ConnectionStrings__AuditDatabase
MongoDb__ConnectionString
MongoDb__DatabaseName
```

These should be supplied through the hosting platform's application settings or secret store.

The audit queue capacity can be configured with:

```text
AuditQueue__Capacity
```

It defaults to `1000` and must be greater than zero.

## Audit queue policy

The audit queue is bounded to the configured capacity, which defaults to 1,000 records. When the queue is full, producers wait for capacity instead of dropping records. Waiting operations observe the request cancellation token.

The queue is in memory and is not durable across process crashes or restarts. A durable outbox or external message broker would be required if audit delivery must survive process failure.

## CI/CD

The GitHub Actions workflow builds, tests, publishes, and deploys the API to Azure App Service. The deployment job runs only after the build and test jobs succeed.

Configure the Azure App Service Health Check path as:

```text
/health
```
