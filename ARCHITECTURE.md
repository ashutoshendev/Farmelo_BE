# ARCHITECTURE

This file is the implementation contract for Farmelo backend modules.

## Layers

- `Farmelo.API`: controllers, middleware, authentication/authorization hooks, dependency registration.
- `Farmelo.Business`: MediatR commands, queries, handlers, and application services.
- `Farmelo.Data`: EF Core write-side, Dapper read-side, repository abstractions.
- `Farmelo.Shared`: DTOs, validators, configuration models, helpers, result wrappers.
- `Farmelo.AutoMapper`: mapping profiles.
- `Farmelo.Integration`: external service clients and adapters.
- `Farmelo.Test`: NUnit tests.

## Request Flow

1. Controller receives a request DTO.
2. FluentValidation and `ModelStateFilter` validate input.
3. Controller sends a MediatR command/query.
4. Handler executes business rules.
5. Read operations use Dapper repositories; writes use EF repositories.
6. Handler returns `ServiceOperationResult` wrappers.

## New Module Checklist

1. Add DTOs in `Farmelo.Shared/DTO/<Module>`.
2. Add validators in `Farmelo.Shared/Validators/<Area>`.
3. Add commands or queries in `Farmelo.Business`.
4. Add handlers in `Farmelo.Business/Handlers/<Module>`.
5. Add read/write repositories when persistence is needed.
6. Add AutoMapper profiles when mapping is needed.
7. Add controller endpoints.
8. Register dependencies in `DependencyInjector`.
9. Add NUnit tests.

## Guardrails

- Do not put business logic in controllers.
- Do not bypass MediatR for standard business APIs.
- Do not mix Dapper read SQL into EF write repositories.
- Do not copy credentials or customer-specific settings from other systems.
- Keep configuration safe by default and environment-specific secrets outside source control.
