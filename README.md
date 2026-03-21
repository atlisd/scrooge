# Scrooge

A simple expense-splitting app for two people. Track shared expenses, see who paid what, and know exactly who owes whom at any time.

## Stack

- **Backend:** ASP.NET Core Web API (.NET 10), Entity Framework Core, PostgreSQL
- **Frontend:** SvelteKit (static adapter, served by nginx)
- **Images:** Published to GitHub Container Registry on every release

---

## Deploy with Docker Compose

No need to clone the repository. Just create two files on your Docker host.

### 1. Create a `.env` file

```env
POSTGRES_DB=splitclaude
POSTGRES_USER=postgres
POSTGRES_PASSWORD=CHANGE_ME_TO_A_STRONG_PASSWORD
```

> **Change `POSTGRES_PASSWORD` before starting.** Never use the default in production.

### 2. Create a `docker-compose.yml` file

```yaml
services:
  db:
    image: postgres:17-alpine
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER}"]
      interval: 5s
      timeout: 3s
      retries: 5
    restart: unless-stopped

  api:
    image: ghcr.io/atlisd/scrooge-api:latest
    ports:
      - "5001:8080"
    environment:
      ConnectionStrings__DefaultConnection: "Host=db;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
    depends_on:
      db:
        condition: service_healthy
    restart: unless-stopped

  web:
    image: ghcr.io/atlisd/scrooge-web:latest
    ports:
      - "5002:80"
    depends_on:
      api:
        condition: service_healthy
    restart: unless-stopped

volumes:
  pgdata:
```

### 3. Start the stack

```bash
docker compose up -d
```

Docker will pull the images from GitHub Container Registry and start all three services. The API migrates the database automatically on first boot.

### Access the app

| | URL |
|-|-----|
| Web UI | http://your-host:5002 |
| API | http://your-host:5001 |
| Swagger | http://your-host:5001/swagger |

On first launch you will be prompted to enter two names to set up the app. This is a one-time step.

---

## Backup & Restore

These scripts require the `backup.sh` and `restore.sh` files from the repository. Download them to the same directory as your `docker-compose.yml` and make them executable:

```bash
chmod +x backup.sh restore.sh
```

### Backup

```bash
./backup.sh
```

Creates a timestamped file (`scrooge_YYYYMMDD-HHMM.bck`) in the current directory. Pass a filename to choose the destination:

```bash
./backup.sh /path/to/my-backup.bck
```

### Restore

```bash
./restore.sh scrooge_YYYYMMDD-HHMM.bck
```

You will be prompted to confirm before any data is overwritten. To restore non-interactively (e.g. from a script):

```bash
echo y | ./restore.sh scrooge_YYYYMMDD-HHMM.bck
```

Both scripts read credentials from the `.env` file automatically.

---

## Updating to a new version

```bash
docker compose pull
docker compose up -d
```

Database migrations run automatically on API startup.

## Stopping

```bash
# Stop without losing data
docker compose down

# Stop and delete all data (irreversible)
docker compose down -v
```

---

## Production setup (HTTPS)

The app is designed to run behind a reverse proxy that handles SSL termination. The API sets `Secure` + `HttpOnly` cookies, so **HTTPS is required** — without it the browser will not send auth cookies.

### Example: Nginx Proxy Manager

1. In Nginx Proxy Manager, add a new **Proxy Host**
2. Forward it to `<docker-host-ip>:5002`
3. Enable SSL and issue a Let's Encrypt certificate
4. Under **Advanced**, add the following custom nginx config to enable WebSocket support (required for live updates):
   ```nginx
   proxy_set_header Upgrade $http_upgrade;
   proxy_set_header Connection "upgrade";
   ```
5. Done — no changes to `docker-compose.yml` needed

> **WebSockets are required for live updates.** Without the `Upgrade` headers, the SignalR connection will fall back to long-polling, which may not work through all reverse proxies.

You can also expose only port `5002` and keep the API internal if you don't need direct API access from outside the host.

---

## How it works

| Split type | Effect on balance |
|------------|-------------------|
| **Equal** | Payer is owed 50% by the other person |
| **Owed by other** | Payer is owed 100% by the other person |
| **Not shared** | Personal expense, no effect on balance |

Amounts are stored as integer cents/øre — no floating-point rounding issues.

---

## License

[MIT](LICENSE)
