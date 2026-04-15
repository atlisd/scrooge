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

# EF Core migrations (run dotnet tool restore first if ef is missing)
dotnet tool restore
dotnet ef migrations add <MigrationName> --project src/Scrooge.Api
dotnet ef migrations list --project src/Scrooge.Api
dotnet ef database update --project src/Scrooge.Api
```

**Dev URLs (local):** API at `http://localhost:5169`, Web at `http://localhost:5173` (Vite dev server), Swagger at `http://localhost:5169/swagger`
**Docker URLs:** API at `http://localhost:5001`, Web at `http://localhost:5002`
**DB connection (local):** `Host=localhost;Database=splitclaude;Username=postgres;Password=postgres`

## Architecture

Scrooge is a two-person expense-splitting app. The .NET API backend serves a SvelteKit SPA frontend:

- **`Scrooge.Api`** — ASP.NET Core REST API with EF Core + PostgreSQL. Auto-migrates on startup. Controllers are thin; all logic lives in scoped services (`UserService`, `ExpenseService`, `BalanceService`). `ExceptionHandlingMiddleware` maps `KeyNotFoundException` → 404, everything else → 500.
- **`Scrooge.Web`** — SvelteKit 5 SPA with adapter-static (SSG, no SSR). TypeScript + Bootstrap 5. Uses native JS `Intl.NumberFormat` for currency formatting. All API calls go through `src/lib/api.ts`. In Docker, nginx serves static files and proxies `/api/*` and `/hubs/*` to the API container. Vite dev server proxies `/api` to localhost:5169.
- **`Scrooge.Shared`** — Record-type DTOs only. Referenced by the API. The SvelteKit frontend has its own TypeScript interfaces in `src/lib/types.ts`.

### Key domain concepts

**Amount is stored as `long` (integer cents/ISK øre)** — not decimal. Currency formatting uses `Intl.NumberFormat` with locale mapped from currency code (e.g. ISK → is-IS).

**SplitType** drives balance calculation:
- `Equal` — payer is owed 50% by the other person
- `FullOther` — payer is owed 100% by the other person
- `NotShared` — personal expense, no effect on balance

**Setup flow** — the app requires exactly 2 users (`POST /api/setup`). `IsSetupCompleteAsync()` checks `Users.Count >= 2`. Setup can only be done once.

### Authentication

Cookie-based sessions (`session_token` cookie). Session tokens are stored in the `AppSessions` DB table (multiple sessions supported). Auth middleware in `Program.cs` checks all `/api/*` routes except `/api/auth/login` and `/api/setup`.

- **Password policy:** minimum 12 chars, uppercase, lowercase, digit, special character
- **Account lockout:** 10 failed login attempts → 5-minute lockout (returns 429)
- **Credentials:** stored in `AppCredentials` table (one row, app-wide) — also holds global currency preference
- **Rate limiting:** 100 req/min per IP via `FixedWindowRateLimiter` (returns 429)

### SignalR / Live Updates

Expense mutations broadcast an `ExpenseChanged` event via SignalR hub at `/hubs/expenses`.

- API hub: `src/Scrooge.Api/Hubs/ExpenseHub.cs` (mapped in `Program.cs`)
- Frontend: `src/lib/hub.svelte.ts` — connects with `@microsoft/signalr`, auto-reconnects, re-connects on tab visibility change
- Pattern: `hub.expenseRevision` ($state rune) increments on each `ExpenseChanged` → components watch it and refetch
- Nginx must proxy WebSocket `Upgrade` headers to `/hubs/*` — already configured in `src/Scrooge.Web/nginx.conf`
- SignalR failure is silent — app still functions, just without live cross-tab updates

### Currency system

`src/lib/currency.ts` handles all formatting. 11 currencies supported: ISK, DKK, NOK, SEK, EUR, GBP, USD, CAD, AUD, CHF, JPY.

- `toMinorUnits(display, currency)` — converts display string to `long` for storage
- `toDisplayValue(minor, currency)` — converts `long` back to display string
- `formatAmount(minor, currency)` — formats for display with symbol
- `formatDate(date)` / `isoToDisplayDate(iso)` — locale-aware date formatting (d.m.yyyy for EU, m/d/yyyy for US, etc.)
- Currency code → locale mapping drives decimal separators, grouping, and date format

### Frontend structure

```
src/Scrooge.Web/src/
  lib/
    api.ts          # Fetch-based API client (credentials: include for cookies); 401 → redirect to login
    currency.ts     # Intl-based formatting, minor-unit conversion, date helpers
    hub.svelte.ts   # SignalR connection, expenseRevision $state rune
    user.ts         # Active user store (Svelte writable) + localStorage persistence
    types.ts        # TypeScript DTOs matching Scrooge.Shared
    NavMenu.svelte, BalanceSummary.svelte, ExpenseCard.svelte, ExpenseForm.svelte
  routes/
    +layout.svelte      # Root: setup/auth check, spinner, currency init, hub start/stop
    (app)/              # Authenticated routes with NavMenu: /, /add, /edit/[id], /history, /about
    (auth)/             # Plain layout: /login, /setup
```

Uses **Svelte 5 runes** syntax (`$state`, `$derived`, `$effect`) — not Svelte 4 stores in new code.

### Testing

- **API tests** (`Scrooge.Api.Tests`) use xUnit + EF Core in-memory database. Tests instantiate services directly with a real `AppDbContext` backed by in-memory storage.
- Currently covers `BalanceService` (5 tests: zero balance, equal split, fullOther split, bidirectional, mixed types) and `ExpenseService`.

## CI/CD

GitHub Actions workflow at `.github/workflows/release.yml`:
- Triggers on tags matching `v*`
- **test** job: runs `dotnet test` on .NET 10
- **build-and-push** job (depends on test): builds and pushes two Docker images to GitHub Container Registry (`ghcr.io`)
  - `ghcr.io/<owner>/scrooge-api` — from `src/Scrooge.Api/Dockerfile`
  - `ghcr.io/<owner>/scrooge-web` — from `src/Scrooge.Web/Dockerfile`
- Tags pushed: `v*` version tag + `latest`
- `package.json` version is set from the git tag before the web image build

To release: `git tag v1.2.3 && git push --tags`
