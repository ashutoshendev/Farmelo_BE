# Farmelo Backend

Farmelo backend is a .NET 8 REST API built with layered architecture, MediatR-style request handling, EF Core for writes, Dapper for fast reads, FluentValidation, cookie authentication, NLog, and NUnit tests.

## Current Status

- Admin and Owner login is implemented with secure HTTP-only cookie authentication.
- Admin users can create, update, and deactivate Owner logins.
- Admin users can view business audit activity from the Admin audit timeline.
- Admin and Owner users can manage products/box variants; Owners can create products, edit product details, and change prices.
- Admin and Owner users can manage the Makhana business workflow: raw inventory, box production, B2B buyers/orders, B2C shops/distributors/stock assignments, returns, payments, pending receivables, and monthly reports.
- Raw stock deduction is strict. B2B orders and box production fail when stock is insufficient.
- B2B/B2C costs are calculated from available stock cost and margins are stored with the transaction.
- Product price changes are written to `ProductPriceHistories`.
- Public product listing is available for the marketing/frontend shop page.
- Backend validations are added through FluentValidation and controller-level null/route-id checks.
- Read operations use Dapper where fast list/query responses are needed.
- Write operations use EF Core repositories with async save patterns.
- Business audit logging is implemented with an in-memory queue and background batch writer so normal requests do not wait on audit DB inserts.
- Silent API request logging writes to `ApiLogs` through middleware and is never exposed in the UI.
- Audit date-range filters are interpreted as Indian calendar days and converted to UTC before querying.
- The local database migration has been applied to `FarmeloDB`.

## Solution Layout

- `Farmelo.API`: controllers, middleware, Swagger, CORS, authentication, dependency injection, audit queue/background service.
- `Farmelo.Business`: commands, queries, handlers, mapping helpers, security services.
- `Farmelo.Data`: EF Core write-side, Dapper read-side, repositories, migrations, database entities.
- `Farmelo.Shared`: DTOs, validators, constants, config classes, helpers, result wrappers.
- `Farmelo.AutoMapper`: mapping profiles.
- `Farmelo.Integration`: external integration adapters.
- `Farmelo.Test`: NUnit tests.

## Main API Areas

```text
GET    /api/health

POST   /api/auth/login
POST   /api/auth/logout
GET    /api/auth/me

GET    /api/products

GET    /api/admin/owners
POST   /api/admin/owners
PUT    /api/admin/owners/{ownerId}
DELETE /api/admin/owners/{ownerId}

GET    /api/owner/products
POST   /api/owner/products
PUT    /api/owner/products/{productId}

GET    /api/admin/audit
GET    /api/admin/audit/actors
GET    /api/admin/dashboard/summary
GET    /api/admin/dashboard/business-summary

GET    /api/inventory/summary
GET    /api/inventory/raw-stock
POST   /api/inventory/raw-stock
POST   /api/inventory/box-production

GET    /api/parties
POST   /api/parties
PUT    /api/parties/{partyId}

GET    /api/b2b/orders
POST   /api/b2b/orders

GET    /api/b2c/assignments
POST   /api/b2c/assignments
POST   /api/b2c/returns

GET    /api/payments
GET    /api/payments/pending-summary
POST   /api/payments

GET    /api/reports/monthly-summary
GET    /api/reports/export/excel
```

## Roles

- `Admin`: full system access, including Owner login/user-role management, business modules, product/pricing, reports, and all audit logs.
- `Owner`: full system access. Audit log queries are server-scoped so Owners can only see their own audit activity.

## Database

The API uses SQL Server. The configured local connection string is:

```json
"ConnectionStrings": {
  "DatabaseConnection": "Server=localhost;Database=FarmeloDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

`ConnectionStrings:DatabaseConnection` is required. There is no fallback local connection string; the API fails fast at startup when it is missing or empty.

Current schema includes:

- `UserAccounts`
- `Products`
- `ProductPriceHistories`
- `Parties`
- `RawStockEntries`
- `RawStockMovements`
- `BoxStockMovements`
- `B2BOrders`
- `B2CAssignments`
- `Payments`
- `AuditLogs`
- `ApiLogs`

Apply migrations with:

```powershell
dotnet ef database update --project src\Farmelo.Data --startup-project src\Farmelo.API
```

The first Admin should be created manually or through a controlled seed process. See `docs/admin-database-setup.md`. Do not store real passwords in source control or README files.

## Audit System

Visible audit rows record meaningful user actions only:

- who logged in or logged out
- when the action happened
- user role/email/name
- business module
- action type
- target id/label
- JSON diffs for changed safe fields only
- IP address

Sensitive fields such as passwords, password hashes, tokens, secrets, and payment details are never written into audit diffs.

Audit writes are optimized by queuing events in memory and persisting them through `AuditLogBackgroundService` in batches. API request logs are written separately to `ApiLogs` by `ApiLoggingMiddleware` and purged after 30 days by a hosted service.

## Date And Time

The backend stores timestamps in UTC for consistency and fast querying. User-facing date/time formatting is handled in the frontend with Indian locale/time zone rules. Audit log date filters from the Admin UI are treated as Indian dates (`Asia/Kolkata`) and converted to UTC ranges before querying `AuditLogs`.

## Run

```powershell
dotnet restore Farmelo.sln
dotnet build Farmelo.sln
dotnet run --project src\Farmelo.API\Farmelo.API.csproj --urls http://localhost:5076
```

Swagger is available at:

```text
http://localhost:5076/swagger
```

## Tests

```powershell
dotnet test Farmelo.sln
```

## Last Verification

The latest implementation was verified with:

```powershell
dotnet build src\Farmelo.API\Farmelo.API.csproj --no-restore -v minimal -o .tmp_build_api
dotnet test Farmelo.sln --no-build -v minimal
dotnet ef database update --project src\Farmelo.Data --startup-project src\Farmelo.API
```

Smoke-tested locally:

- Admin login
- Admin dashboard summary API
- Business dashboard summary API
- Inventory schema migration applied to `FarmeloDB`
- Admin audit search/filter/pagination API
- hidden API log writes
