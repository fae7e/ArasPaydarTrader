# ArasTrader

A trading platform REST API built with **.NET 8 Minimal API** and Clean Architecture. It manages customers, wallets, and buy/sell orders, processing pending orders asynchronously via a Hangfire background job.

---

## Table of Contents

- [Architecture](#architecture)
- [Domain Model](#domain-model)
- [Application Layer](#application-layer)
- [Infrastructure Layer](#infrastructure-layer)
- [API Layer & Gateway Pipeline](#api-layer--gateway-pipeline)
- [Background Job](#background-job)
- [Optimistic Concurrency](#optimistic-concurrency)
- [External API Integration](#external-api-integration)
- [API Endpoints](#api-endpoints)
- [Running with Docker](#running-with-docker)
- [Running Locally](#running-locally)

---

## Architecture

The solution follows **Clean Architecture**, organized into four projects with a strict one-way dependency rule:

```
ArasTrader.Domain
    ↑
ArasTrader.Application
    ↑
ArasTrader.Infrastructure
    ↑
ArasTrader.Web.Api
```

| Project | Responsibility |
|---|---|
| `Domain` | Entities, enums, repository interfaces — no external dependencies |
| `Application` | Use-case services, DTOs, service interfaces |
| `Infrastructure` | EF Core, repositories, external HTTP client, Hangfire jobs |
| `Web.Api` | Minimal API endpoints, Gateway pipeline, DI wiring |

---

## Domain Model

### Entities

**`Customer`** — A person who can place orders and hold a wallet.

**`Order`** — Represents a Buy or Sell instruction placed by a customer.
- Status lifecycle: `Pending` → `Processing` → `Processed` / `Failed`
- `MarkAsFailed()` and `MarkAsProcessed()` are the only ways to change status, enforcing invariants inside the entity.

**`Wallet`** — Holds a customer's balance.
- `Debit(amount)` — subtracts from balance; throws if insufficient funds.
- `Credit(amount)` — adds to balance.
- Both methods increment `Version` (used for optimistic concurrency — see below).

### Enums

| Enum | Values |
|---|---|
| `OrderType` | `Buy`, `Sell` |
| `OrderStatus` | `Pending`, `Processing`, `Processed`, `Failed` |

---

## Application Layer

### Services

**`CustomerQueryService`** — Fetches customers from the local database.

**`CustomerSyncService`** — Pulls customers from the external API and upserts them locally.

**`WalletQueryService`** — Reads a customer's wallet balance.

**`OrderService`** — Creates new orders and retrieves existing ones.
- Uses `IdempotencyKey` to deduplicate requests: if the same key arrives twice, the original response is returned with `IsIdempotentDuplicate: true` instead of creating a duplicate order.

### DTOs

**`PlaceOrderRequest`**

```json
{
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": 0,
  "amount": 100.00,
  "idempotencyKey": "unique-client-key"
}
```

**`PlaceOrderResponse`**

```json
{
  "orderId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "customerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "type": 0,
  "amount": 100.00,
  "status": 0,
  "createdAt": "2026-01-01T00:00:00Z",
  "isIdempotentDuplicate": false
}
```

---

## Infrastructure Layer

### Persistence

- **SQLite** via EF Core 8.
- `ApplicationDbContext` implements `IUnitOfWork`, keeping the `SaveChanges` contract behind an interface.
- Migrations run automatically on startup — no manual `dotnet ef` step needed.

### Repositories

| Repository | Notes |
|---|---|
| `CustomerRepository` | Standard CRUD |
| `WalletRepository` | Tracked reads — required for concurrency token to work correctly (see [Optimistic Concurrency](#optimistic-concurrency)) |
| `OrderRepository` | Includes `TryClaimForProcessingAsync` for safe concurrent job processing |

### Resilience (Polly)

All outbound HTTP clients are wrapped with two Polly policies:

1. **Exponential backoff retry** — retries 3 times on transient HTTP errors (5xx, network failures), doubling the wait each time: 2s → 4s → 8s.
2. **Rate-limit retry** — retries 3 times on `429 Too Many Requests`, waiting 10s → 20s → 30s between attempts.

---

## API Layer & Gateway Pipeline

### Order Gateway Pipeline

Placing an order passes through a middleware pipeline before reaching `IOrderService`. This separates cross-cutting concerns from business logic and makes it easy to add new steps without touching the service.

```
Request
  │
  ▼
LoggingGatewayHandler     ← logs request details and measures elapsed time
  │
  ▼
ValidationGatewayHandler  ← validates CustomerId, Amount, IdempotencyKey
  │
  ▼
IOrderService.PlaceOrderAsync
  │
  ▼
Response
```

The pipeline is built with `Aggregate` over the registered handler list — handlers are composed right-to-left so the first registered handler executes first.

**Validation rules enforced by `ValidationGatewayHandler`:**
- `CustomerId` must not be empty.
- `Amount` must be greater than zero.
- `IdempotencyKey` is required and must not exceed 128 characters.

**Adding a new handler** — implement `IOrderGatewayHandler` and register it in `Program.cs` before `OrderGatewayPipeline`:
```csharp
builder.Services.AddScoped<IOrderGatewayHandler, YourNewHandler>();
```

---

## Background Job

### `WalletUpdateJob` — runs every 2 minutes via Hangfire

Picks up all `Pending` orders and applies the corresponding wallet balance change.

**Processing flow per order:**

```
1. TryClaimForProcessingAsync
      └─ UPDATE Pending → Processing
         (only one job instance wins; others skip this order)

2. Load the wallet for the order's customer

3. Apply balance change inside a DB transaction
      ├─ Buy  → Wallet.Debit(amount)
      └─ Sell → Wallet.Credit(amount)

4. Mark order as Processed and commit

5. On DbUpdateConcurrencyException → reload entity, retry (up to 5 times, back-off 50ms × attempt)
   On InvalidOperationException    → mark order as Failed (e.g. insufficient funds)
   On any other exception          → mark order as Failed (best-effort save)
```

The claim step (1) is an optimistic atomic `UPDATE ... WHERE Status = Pending`. If two job instances run simultaneously, only one flips the status to `Processing`; the other gets 0 rows affected and skips that order — no double-processing.

---

## Optimistic Concurrency

### The problem

Without concurrency control, two job instances loading the same wallet simultaneously would each read `Balance = 1000, Version = 5`, apply their changes independently, and one would silently overwrite the other's update (lost update problem).

### How `Version` solves it

`Wallet.Version` is an application-managed counter. Every `Debit` or `Credit` call increments it:

```csharp
public void Debit(decimal amount)
{
    Balance -= amount;
    Version++;          // 5 → 6
    UpdatedAt = DateTime.UtcNow;
}
```

`Version` is configured as an EF Core concurrency token:

```csharp
builder.Property(w => w.Version).IsConcurrencyToken();
```

This tells EF to include the **original** value of `Version` in every UPDATE's WHERE clause:

```sql
UPDATE Wallets
SET Balance = 900, Version = 6, UpdatedAt = ...
WHERE Id = 'xxx' AND Version = 5   -- original value at load time
```

If another instance already saved (bumping the DB version to 6), this WHERE matches 0 rows → EF throws `DbUpdateConcurrencyException` → the job reloads the entity with the fresh value and retries.

### Why tracked reads matter

`WalletRepository.GetByCustomerIdAsync` must **not** use `AsNoTracking()`. With no tracking, EF has no record of the original `Version`. When `Update(wallet)` is called, EF treats the post-increment value as both current and original, generating `WHERE Version = 6` against a database that still holds `5` — causing an immediate exception on every save. Tracked reads let EF remember `Version = 5` was the value at load time.

### `IsConcurrencyToken` vs `IsRowVersion`

`IsConcurrencyToken` works on any database (including SQLite) and the application controls the value. `IsRowVersion` is SQL Server-specific and the database manages the value automatically. Since this project uses SQLite, `IsConcurrencyToken` is the correct choice.

---

## External API Integration

### Token Service

`TokenService` fetches a bearer token from the external provider and caches it in-memory until it expires. All subsequent requests within the cache window reuse the same token without extra HTTP calls.

### Customer Sync

`ExternalApiClient` attaches the cached bearer token to each request and calls the external customer API. `CustomerSyncService` maps the response to local `Customer` entities and upserts them — inserting new ones and updating existing ones matched by their external ID.

---

## API Endpoints

| Method | Path | Description |
|---|---|---|
| `GET` | `/api/customers` | Returns all customers in the local database |
| `POST` | `/api/customers/sync` | Pulls customers from the external API and upserts them locally |
| `GET` | `/api/wallets/customer/{customerId}` | Returns the wallet balance for a given customer |
| `POST` | `/api/orders` | Places a new order through the Gateway Pipeline |
| `GET` | `/api/orders/{orderId}` | Returns an existing order by ID |

Interactive documentation: `/swagger`
Hangfire dashboard: `/hangfire`

---

## Running with Docker

```bash
# Build image (run from the solution root)
docker build -f ArasTrader.Web.Api/Dockerfile -t arastrader .

# Run with a persistent volume for the SQLite file
docker run -p 8080:8080 -v arastrader-data:/app/data arastrader
```

- SQLite database is stored at `/app/data/arasTrader.db` inside the container.
- Mount a named volume (`-v`) to persist data across container restarts.
- Swagger UI: `http://localhost:8080/swagger`

---

## Running Locally

**Prerequisites:** .NET 8 SDK

```bash
cd ArasTrader.Web.Api
dotnet run
```

The SQLite database file is created automatically and migrations are applied on startup. No manual `dotnet ef database update` needed.

The connection string can be overridden via environment variable:

```
ConnectionStrings__DefaultConnection=Data Source=/app/data/arasTrader.db
```

---

## Migrations

Migrations live in `ArasTrader.Infrastructure`.

**Add a new migration**
```bash
dotnet ef migrations add <MigrationName> \
  --project ArasTrader.Infrastructure \
  --startup-project ArasTrader.Web.Api
```

**Apply migrations to the database**
```bash
dotnet ef database update \
  --project ArasTrader.Infrastructure \
  --startup-project ArasTrader.Web.Api
```

