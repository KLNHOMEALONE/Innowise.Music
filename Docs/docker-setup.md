# Docker Development Setup

This document describes the Docker-based development environment for Innowise.Music.

## Quick Start

```bash
# 1. Create a .env file with database credentials
cat > .env << EOF
POSTGRES_USER=admin
POSTGRES_PASSWORD=admin
EOF

# 2. Start all services
docker-compose up --build
```

This starts four containers:

| Service | Port | URL |
|---------|------|-----|
| PostgreSQL | 5432 | `localhost:5432` |
| Adminer | 8080 | `http://localhost:8080` |
| Identity Server (HTTP) | 5236 | `http://localhost:5236` |
| Identity Server (HTTPS) | 7008 | `https://localhost:7008` |
| Admin Dashboard | 5237 | `http://localhost:5237` |

## HTTPS Development Certificate

The Identity Server requires HTTPS for secure communication. The Dockerfile automatically generates a development certificate during the build process — **no manual certificate export or setup is required**.

### How it works

1. During the Docker build, `dotnet dev-certs https` generates a PKCS#12 certificate inside the SDK build container
2. The certificate is converted to PEM format (separate certificate and private key files) using `openssl`
3. The PEM files are copied into the final runtime image with correct permissions for the non-root `app` user
4. ASP.NET Core is configured via environment variables to use these PEM files:
   - `ASPNETCORE_Kestrel__Certificates__Default__Path` — path to the certificate PEM
   - `ASPNETCORE_Kestrel__Certificates__Default__KeyPath` — path to the private key PEM

### Why this approach?

Previous approaches required developers to manually export their host machine's dev certificate and make it available to the container. This was:
- **Platform-dependent** — different cert store locations on Windows/macOS/Linux
- **Fragile** — volume mounts from the host could shadow the certificate files
- **Error-prone** — missing certs caused cryptic `FileNotFoundException` errors

The auto-generated certificate approach is fully self-contained in the Docker image and works identically on all platforms.

## Environment Variables

### Required (`.env` file)

| Variable | Description |
|----------|-------------|
| `POSTGRES_USER` | PostgreSQL username |
| `POSTGRES_PASSWORD` | PostgreSQL password |

### Docker Compose (set automatically)

| Variable | Description |
|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | `Development` |
| `ConnectionStrings__PostgresAppDbConnection` | Connection string to PostgreSQL (uses `postgres` service hostname) |

## Troubleshooting

### Container won't start — certificate errors

If you see `FileNotFoundException` for the certificate PEM files, the most likely cause is an outdated `docker-compose.override.yml` that mounts a host directory over the container's certificate path. Remove any `ASP.NET/Https` volume mounts from `docker-compose.override.yml`.

### Database connection errors

The Identity Server connects to PostgreSQL using the service hostname `postgres` (not `localhost`). If you see `NpgsqlException: Failed to connect`, verify:
1. The `.env` file exists with valid credentials
2. The PostgreSQL container is running (`docker-compose ps`)
3. The connection string in docker-compose.yml uses `Server=postgres`

### HTTPS certificate warnings in browser

The auto-generated dev certificate uses `CN=localhost` and is not trusted by browsers. This is expected for development. To trust the certificate:
1. Export it from the container: `docker cp music_identity_server:/home/app/.aspnet/https/https-dev-cert.pem .`
2. Import it into your OS/browser trust store

## Rebuilding

After changing the Dockerfile or docker-compose files:

```bash
docker-compose down
docker-compose build --no-cache innowise.musicidentityserver
docker-compose up -d
```
