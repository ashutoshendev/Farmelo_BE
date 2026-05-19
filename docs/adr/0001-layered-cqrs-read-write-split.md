# ADR 0001: Layered CQRS Read/Write Split

## Status

Accepted

## Context

Farmelo needs a backend structure that can grow module by module while keeping HTTP, business logic, persistence, and shared contracts separate.

## Decision

Use a layered .NET 8 solution:

- API controllers dispatch MediatR commands and queries.
- Business handlers contain application logic.
- EF Core handles write-side persistence.
- Dapper handles read-side projections and query-heavy endpoints.
- Shared result wrappers standardize API responses.

## Consequences

New modules require several small artifacts, but dependencies stay explicit and query/write responsibilities remain clear.
