# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

UnderworldAPI is a .NET 10 solution with two independently deployable microservices — **Sales**
and **Purchases** — that communicate asynchronously via Azure Service Bus. Each service follows
Clean Architecture (API → Application → Domain ← Infrastructure) and CQRS via MediatR. There is no
shared database; the services are decoupled and only talk to each other through integration events.

## Commands

### Build
```bash
dotnet build                                                    # whole solution (UnderworldAPI.slnx)
dotnet build src/Services/Sales/UnderworldAPI.Sales.API/UnderworldAPI.Sales.API.csproj
dotnet build src/Services/Purchases/UnderworldAPI.Purchases.API/UnderworldAPI.Purchases.API.csproj
```

### Test
```bash
dotnet test                                                      # all test projects
dotnet test tests/UnderworldAPI.Sales.Domain.Tests/UnderworldAPI.Sales.Domain.Tests.csproj
dotnet test tests/UnderworldAPI.Sales.Application.Tests/UnderworldAPI.Sales.Application.Tests.csproj
dotnet test tests/UnderworldAPI.Purchases.Domain.Tests/UnderworldAPI.Purchases.Domain.Tests.csproj
dotnet test tests/UnderworldAPI.Purchases.Application.Tests/UnderworldAPI.Purchases.Application.Tests.csproj

# Single test
dotnet test tests/UnderworldAPI.Sales.Application.Tests --filter "FullyQualifiedName~CreateOrderCommandHandlerTests.Handle_WithValidCommand_ShouldCreateOrderAndReturnId"
```
CI (`pipelines/azure-pipelines.yml`) runs these four test projects individually with
`--collect:"XPlat Code Coverage"` and publishes `trx`/cobertura results — there are only Domain and
Application test projects per service (no API/Infrastructure test projects exist).

Test stack: xUnit + FluentAssertions + NSubstitute (`Substitute.For<T>()` for mocking
repositories/`IUnitOfWork`/publishers). Domain tests hit aggregates/value objects directly;
Application tests hit MediatR command/query handlers with mocked dependencies.

### Run locally
Two ways to run a service:
1. **F5 / VS Code debug** — `.vscode/launch.json` has "Debug Sales API" / "Debug Purchases API"
   configs, each with a `preLaunchTask` (`.vscode/tasks.json`: `build-sales` / `build-purchases`).
   These run against `appsettings.Development.json` config directly (no Docker, no Key Vault unless
   `AZURE_KEYVAULT_URI` is uncommented in the env block).
2. **Docker Compose** — `docker-compose.yml` (base, no dev overrides) + `docker-compose.override.yml`
   (gitignored — copy from `docker-compose.override.yml.example`) brings up `sales-api`,
   `purchases-api`, and a `sqlserver` container. Secrets come from `.env` (copy from `.env.example`):
   `SQL_SA_PASSWORD`, `SALES_DB_CONNECTION_STRING`, `PURCHASES_DB_CONNECTION_STRING`,
   `SERVICE_BUS_CONNECTION_STRING`.
   ```bash
   docker compose up --build
   ```

Both services expose Scalar API docs (Swagger UI replacement) at `/scalar/v1` in Development, and a
health check at `/health`. In Development, `Program.cs` calls `dbContext.Database.MigrateAsync()`
automatically at startup — no manual `dotnet ef database update` needed locally.

### EF Core migrations
Run from the relevant `*.Infrastructure` project (each service owns its own `DbContext` and
migrations folder under `Persistence/Migrations`):
```bash
dotnet ef migrations add <Name> \
  --project src/Services/Sales/UnderworldAPI.Sales.Infrastructure \
  --startup-project src/Services/Sales/UnderworldAPI.Sales.API
```
(swap `Sales` for `Purchases` as needed).

## Architecture

### Per-service layering (identical shape for Sales and Purchases)
```
<Service>.API             ASP.NET Core host: Controllers, Program.cs, JWT/versioning/OpenAPI wiring
<Service>.Application      MediatR commands/queries + handlers, FluentValidation validators,
                           AutoMapper profiles, DTOs, abstractions (IUnitOfWork, IEventPublisher)
<Service>.Domain           Aggregates, value objects, domain events, domain exceptions, repository
                           ports (interfaces only — no EF/infra dependency)
<Service>.Infrastructure   EF Core DbContext + migrations + repositories, Azure Service Bus
                           publisher/consumer, JWT token generator
```
Dependency direction: API → Application → Domain, and Infrastructure → Application/Domain (never
the reverse — Domain has no dependency on Infrastructure or Application).

`src/Shared/UnderworldAPI.Shared.Domain` holds cross-service primitives referenced by both services:
`Result`/`Result<T>`/`Error` (railway-style outcome type — handlers return `Result<T>` instead of
throwing for expected failures), `AggregateRoot`/`Entity`/`ValueObject`/`IDomainEvent` base types,
`ITokenGenerator`, and `OrderCreatedIntegrationEvent` (the cross-service contract, see below).
`UnderworldAPI.Shared.Infrastructure` currently has no real content (`Class1.cs` placeholder).

### CQRS pipeline (per service, wired in `Application/DependencyInjection/ApplicationServiceExtensions.cs`)
MediatR registers all handlers in the assembly plus an open `ValidationPipelineBehavior<,>` that
runs FluentValidation validators before a handler executes and short-circuits to a failed `Result`
on validation errors. AutoMapper profiles map domain aggregates to DTOs.

### Cross-service integration (Sales → Purchases, one-directional today)
This is the piece that requires reading multiple files/projects to see:
1. `Sales` creates an `Order` → `CreateOrderCommandHandler` persists it, then publishes an
   `OrderCreatedIntegrationEvent` (in `UnderworldAPI.Shared.Domain`) via `IIntegrationEventPublisher`
   → `IntegrationEventPublisher` (Sales.Infrastructure) → Azure Service Bus topic `sales-events`.
2. `Purchases.Infrastructure.Messaging.SalesEventsConsumer` (a `BackgroundService`, singleton) listens
   on topic `sales-events` / subscription `purchases-restock-subscription`. On
   `OrderCreatedIntegrationEvent`, it resolves a scoped `ISender` (via `IServiceScopeFactory`, since
   the consumer itself is a singleton) and dispatches a `CreatePurchaseOrderCommand` to
   auto-generate a restock `PurchaseOrder` mirroring the sold items.
3. There's a second, separate publishing path: `IEventPublisher`/`AzureServiceBusPublisher` publishes
   raw domain events (`IDomainEvent`) to a topic derived from the event type name (PascalCase →
   kebab-case, `Event` suffix stripped) — used for other domain events, distinct from the
   `IIntegrationEventPublisher` path used specifically for `OrderCreatedIntegrationEvent`. Don't
   conflate the two publisher abstractions when tracing how an event reaches Service Bus.

### Config & secrets resolution order (see comments in each `Program.cs` — they differ per service)
- **Sales**: reads `AZURE_KEYVAULT_URI` env var first, falls back to `AzureKeyVault:Uri` config; if
  neither is set, Key Vault is skipped entirely (local dev uses `appsettings.Development.json` or
  `.env`/compose override instead). Uses `DefaultAzureCredential` with most non-CLI credential
  sources excluded.
- **Purchases**: only wires Key Vault when `IsProduction()`, with a hardcoded Key Vault URI.
- JWT auth (`AddJwtAuthentication`) is added *after* Key Vault config in both, since the secret key
  may come from Key Vault. Secret resolution order: `JwtSecretKey` → `Jwt:SecretKey` → throws.
- `ExceptionHandlingMiddleware` is registered first in the pipeline in both services so it wraps
  everything else.

### Result pattern
Application handlers return `Result` / `Result<T>` (`UnderworldAPI.Shared.Domain.Results`) rather
than throwing for expected/business failures — check `IsSuccess`/`IsFailure` and `Error` rather than
try/catch. `Result<T>` has an implicit conversion from `T`, so handlers can `return order;` directly.
Reserve exceptions for truly exceptional/unexpected cases (caught centrally by
`ExceptionHandlingMiddleware`).

### Known rough edges (don't "fix" without checking if it's intentional/in progress)
- Several `Class1.cs` placeholder files remain from `dotnet new classlib` scaffolding (Application/
  Domain/Infrastructure projects on both services, plus `Shared.Infrastructure`).
- A few filenames have a duplicated extension: `Sales.Application/Auth/Commands/Login/LoginCommandValidator.cs.cs`,
  `Sales.Infrastructure/Auth/JwtTokenGenerator.cs.cs`.
- Test projects `tests/*.Tests` each still contain a scaffolded `UnitTest1.cs` alongside the real
  tests.
- Comments throughout `Program.cs`/DI extensions are in Spanish and explain non-obvious
  environment-specific decisions (Key Vault fallback order, why migrations auto-run only in Dev,
  why JWT is wired after Key Vault) — read them before changing startup behavior.
