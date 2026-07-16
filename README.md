# finorganizer-api

💰🧮 Personal finance API — income organizer, planner &amp; assets tracker. .NET 10, Clean Architecture, EF Core + SQLite

## Solution structure

```
FinOrganizer.slnx
src/
  FinOrganizer.Domain/         # Entities, enums, domain services — no dependencies
  FinOrganizer.Application/    # Use-case services, DTOs, FluentValidation validators, interfaces
  FinOrganizer.Infrastructure/ # EF Core DbContext, SQLite config, migrations, seed data
  FinOrganizer.Api/            # Minimal API endpoints, DI wiring, OpenAPI, background service
tests/
  FinOrganizer.UnitTests/        # xUnit + NSubstitute — domain and application layer
  FinOrganizer.IntegrationTests/ # WebApplicationFactory + in-memory SQLite — endpoint happy paths
```

Dependencies point inward only: `Api → Infrastructure/Application → Domain`. `Infrastructure` implements
`IApplicationDbContext`, `IPriceProvider`, and `IDateTimeProvider`, all defined in `Application`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- (Optional, for creating new migrations) the EF Core CLI tool: `dotnet tool install --global dotnet-ef`

## Running locally

```bash
dotnet restore
dotnet run --project src/FinOrganizer.Api
```

On startup in the `Development` environment the app automatically applies any pending EF Core
migrations against the SQLite database configured by `ConnectionStrings:Default` in
`src/FinOrganizer.Api/appsettings.json` (default: `Data Source=finorganizer.db`, created next to the
running executable). Default categories (Salary, Freelance, Dividends, Rent/Mortgage, Groceries,
Utilities, Subscriptions, Transport, Health, Leisure) are seeded as part of the initial migration.

Swagger UI is available at `/swagger` in Development; the raw OpenAPI document is served at
`/swagger/v1/swagger.json` for the frontend to consume.

## Migrations

Migrations live in `src/FinOrganizer.Infrastructure/Persistence/Migrations`. To add a new one after
changing an entity or an `IEntityTypeConfiguration<T>`:

```bash
dotnet ef migrations add <Name> \
  --project src/FinOrganizer.Infrastructure \
  --startup-project src/FinOrganizer.Infrastructure \
  -o Persistence/Migrations
```

The `Infrastructure` project ships its own `IDesignTimeDbContextFactory`, so migrations can be
generated without the Api project's DI container. To apply migrations without starting the API:

```bash
dotnet ef database update \
  --project src/FinOrganizer.Infrastructure \
  --startup-project src/FinOrganizer.Infrastructure
```

In non-Development environments migrations are **not** applied automatically — run the command above
(or your deployment pipeline's equivalent) as part of release.

## Tests

```bash
dotnet test
```

- **Unit tests** cover pure domain logic (weighted-average cost / realized P&L, recurrence next-due-date
  computation, account balance calculation), the dependency-free CSV transaction parser, and
  application services that are cheapest to test against a real EF Core context — `BudgetsService`
  (budget vs. actual) and `ProjectionService` (cash-flow projection) run against an in-memory SQLite
  connection with `IPriceProvider`/`IDateTimeProvider` substituted via NSubstitute.
- **Integration tests** boot the actual `Program` host via `WebApplicationFactory<Program>` against an
  in-memory SQLite connection and exercise endpoint happy paths over HTTP (accounts, transactions
  including CSV import, assets/positions, dashboard aggregates, budget vs. actual).

## Docker

```bash
docker compose up --build
```

This builds the API image and starts it on `http://localhost:8080`, persisting the SQLite database file
to a named volume (`finorganizer-data`) so data survives container restarts. See `Dockerfile` and
`docker-compose.yml`.

## API overview

All endpoints are versioned under `/api/v1/`:

| Area | Base route | Notes |
|---|---|---|
| Accounts | `/api/v1/accounts` | CRUD; current balance is always computed from transactions, never stored |
| Categories | `/api/v1/categories` | CRUD; one level of nesting via `parentCategoryId` |
| Transactions | `/api/v1/transactions` | CRUD, filtered/paged listing, `POST /import` for CSV |
| Recurrence | `/api/v1/recurrence-rules` | CRUD, `GET /pending-occurrences`, `POST /occurrences/{id}/confirm\|skip` |
| Assets | `/api/v1/assets`, `/api/v1/asset-events` | CRUD, `GET /positions` (computed avg cost/P&L), price snapshots |
| Budgets | `/api/v1/budgets` | CRUD, `GET /vs-actual?month=` |
| Savings goals | `/api/v1/savings-goals` | CRUD, `/{id}/contributions` |
| Dashboard | `/api/v1/dashboard` | net-worth, allocation, income-vs-expense, passive-income, top-expense-categories, projection |

A `RecurrencePostingBackgroundService` runs on startup and every 24 hours: rules with `autoPost: true`
are materialized directly into transactions, others are queued as `PendingConfirmation` occurrences for
the UI to confirm or skip via the endpoints above.
