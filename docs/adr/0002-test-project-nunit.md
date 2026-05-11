# ADR 0002: NUnit Test Project

## Status

Accepted

## Context

The backend needs a test project that matches the Prestige reference and works well for handler, validator, and service tests.

## Decision

Use `Farmelo.Test` as a .NET 8 NUnit test project with `Microsoft.NET.Test.Sdk`, `NUnit`, `NUnit3TestAdapter`, and `coverlet.collector`.

## Consequences

Tests can be run with `dotnet test Farmelo.sln`, and future modules should add focused tests alongside implementation work.
