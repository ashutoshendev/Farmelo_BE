# Farmelo Backend Agent Guide

## Purpose

Use this file to bootstrap development sessions in this repository.

## Project Summary

Farmelo is a .NET 8 backend skeleton using the same layered architecture as the Prestige backend reference:

Controller -> FluentValidation / model validation -> MediatR command or query -> Business handler -> Repository -> EF Core writes or Dapper reads -> `ServiceOperationResult` response.

## Solution Structure

- `src/Farmelo.API`: HTTP entrypoint, controllers, middleware, authentication hooks, DI.
- `src/Farmelo.Business`: MediatR commands, queries, handlers, and business services.
- `src/Farmelo.Data`: EF Core write-side, Dapper read-side, repositories, entities, DB context.
- `src/Farmelo.Integration`: external API clients and service adapters.
- `src/Farmelo.AutoMapper`: mapping profiles.
- `src/Farmelo.Shared`: DTOs, validators, config, helpers, result wrappers.
- `src/Farmelo.Test`: NUnit test project.

## Core Files

- API startup: `src/Farmelo.API/Program.cs`
- DI registration: `src/Farmelo.API/DI/DependencyInjector.cs`
- EF Core context: `src/Farmelo.Data/Write/EFContext/FarmeloDbContext.cs`
- Result wrappers: `src/Farmelo.Shared/OperationResult/ServiceOperationResult.cs`

## Implementation Conventions

- Keep controllers thin; they should bind input, call MediatR, and return results.
- Put commands in `Business/Commands/<Feature>`.
- Put queries in `Business/Queries/<Feature>`.
- Put handlers in `Business/Handlers/<Feature>`.
- Put DTOs in `Shared/DTO/<Feature>`.
- Put validators in `Shared/Validators/<Area>`.
- Put read repositories in `Data/Read/Queries`.
- Put write repositories in `Data/Write`.
- Return `ServiceOperationResult<T>` or `ServiceOperationPaginatedResult<T, P>` from handlers.

## Testing

Run from the solution root:

```powershell
dotnet test Farmelo.sln
```

Prefer focused NUnit tests for handlers, validators, and small services.
