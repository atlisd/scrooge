# EF Core Migration Helper

Assists with Entity Framework Core migrations for the Scrooge API.

## Usage

`/migrate` — run with an optional argument describing what you want:
- `/migrate add <MigrationName>` — scaffold a new migration
- `/migrate apply` — apply pending migrations to the local database
- `/migrate status` — list all migrations and which are applied

If no argument is given, show the current migration status and ask what to do.

## Steps

1. Ensure `dotnet-ef` is available:
   ```bash
   dotnet tool restore
   ```

2. For **add**: scaffold the migration, then immediately read the generated file in `src/Scrooge.Api/Migrations/` and summarize what it will do. Ask the user to confirm before applying.
   ```bash
   dotnet ef migrations add <MigrationName> --project src/Scrooge.Api
   ```

3. For **apply**: apply pending migrations to the local database.
   ```bash
   dotnet ef database update --project src/Scrooge.Api
   ```

4. For **status**: list all migrations.
   ```bash
   dotnet ef migrations list --project src/Scrooge.Api
   ```

## Important notes

- Always review the generated migration file before applying — check for unintended column drops or data-loss operations.
- The `--project` flag is required; the tool won't find the DbContext without it.
- Local DB connection: `Host=localhost;Database=splitclaude;Username=postgres;Password=postgres`
- In Docker, migrations run automatically on API startup — no manual `database update` needed.
