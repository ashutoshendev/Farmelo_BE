# Farmelo Backend

Farmelo backend is a .NET 8 REST API skeleton built with a layered architecture, MediatR-style request handling, EF Core for writes, Dapper for reads, AutoMapper, FluentValidation, NLog, and NUnit tests.

## Solution Layout

- `Farmelo.API`: HTTP layer, middleware, Swagger, CORS, auth hooks, DI.
- `Farmelo.Business`: commands, queries, handlers, application services.
- `Farmelo.Data`: EF Core write-side, Dapper read-side, repositories.
- `Farmelo.Shared`: DTOs, validators, config classes, helpers, result wrappers.
- `Farmelo.AutoMapper`: mapping profiles.
- `Farmelo.Integration`: external integration adapters.
- `Farmelo.Test`: NUnit tests.

## Run

```powershell
dotnet restore Farmelo.sln
dotnet build Farmelo.sln
dotnet run --project src\Farmelo.API\Farmelo.API.csproj
```

Swagger is available at `/swagger` when the API is running.

## Health Check

```text
GET /api/health
```

The endpoint returns a `ServiceOperationResult<HealthStatusDto>` payload.

## Configuration

`src/Farmelo.API/appsettings.json` is intentionally sanitized. Put real connection strings and external service secrets in environment-specific configuration or user secrets, not in source control.

If `ConnectionStrings:DatabaseConnection` is empty, the API registers a local SQL Server LocalDB fallback connection string so the skeleton can start without copied credentials.

## Tests

```powershell
dotnet test Farmelo.sln
```
