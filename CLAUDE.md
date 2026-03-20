# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build .NET solution (API + Shared)
dotnet build Scrooge.slnx

# Run API tests
dotnet test Scrooge.slnx
dotnet test tests/Scrooge.Api.Tests/Scrooge.Api.Tests.csproj

# Run a specific test by name (partial match)
dotnet test --filter "BalanceServiceTests"
dotnet test --filter "EqualSplit_CorrectBalance"

# Run the API (requires PostgreSQL on localhost:5432)
dotnet run --project src/Scrooge.Api

# Run the SvelteKit frontend (requires Node.js)
cd src/Scrooge.Web && npm install && npm run dev

# Run everything via Docker (recommended for full-stack dev)
docker compose up --build

# EF Core migrations
dotnet ef migrations add <MigrationName> --project src/Scrooge.Api
dotnet ef database update --project src/Scrooge.Api
```

**Dev URLs (local):** API at `http://localhost:5169`, Web at `http://localhost:5173` (Vite dev server), Swagger at `http://localhost:5169/swagger`
**Docker URLs:** API at `http://localhost:5001`, Web at `http://localhost:5002`
**DB connection (local):** `Host=localhost;Database=splitclaude;Username=postgres;Password=postgres`

## Architecture

Scrooge is a two-person expense-splitting app. The .NET API backend serves a SvelteKit SPA frontend:

- **`Scrooge.Api`** — ASP.NET Core REST API with EF Core + PostgreSQL. Auto-migrates on startup. Controllers are thin; all logic lives in scoped services (`UserService`, `ExpenseService`, `BalanceService`). `ExceptionHandlingMiddleware` maps `KeyNotFoundException` → 404, everything else → 500.
- **`Scrooge.Web`** — SvelteKit SPA with adapter-static. TypeScript + Bootstrap 5. Uses native JS `Intl.NumberFormat` for currency formatting. All API calls go through `src/lib/api.ts`. In Docker, nginx serves static files and proxies `/api/*` to the API container. Vite dev server proxies `/api` to localhost:5169.
- **`Scrooge.Shared`** — Record-type DTOs only. Referenced by the API. The SvelteKit frontend has its own TypeScript interfaces in `src/lib/types.ts`.

### Key domain concepts

**Amount is stored as `long` (integer cents/ISK øre)** — not decimal. Currency formatting uses `Intl.NumberFormat` with locale mapped from currency code (e.g. ISK → is-IS).

**SplitType** drives balance calculation:
- `Equal` — payer is owed 50% by the other person
- `FullOther` — payer is owed 100% by the other person
- `NotShared` — personal expense, no effect on balance

**Setup flow** — the app requires exactly 2 users (`POST /api/setup`). `IsSetupCompleteAsync()` checks `Users.Count >= 2`. Setup can only be done once.

### Frontend structure

```
src/Scrooge.Web/src/
  lib/
    api.ts          # Fetch-based API client (credentials: include for cookies)
    currency.ts     # Intl-based formatting, currency stores
    types.ts        # TypeScript DTOs matching Scrooge.Shared
    NavMenu.svelte, BalanceSummary.svelte, ExpenseCard.svelte, ExpenseForm.svelte
  routes/
    +layout.svelte  # Root: setup/auth check, spinner, currency init
    (app)/           # Authenticated routes with NavMenu
    (auth)/          # Setup and login (plain layout)
```

### Testing

- **API tests** (`Scrooge.Api.Tests`) use xUnit + EF Core in-memory database. Tests instantiate services directly with a real `AppDbContext` backed by in-memory storage.
