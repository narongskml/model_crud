# PortModelApi (Backend)

A .NET 8 Web API for managing portfolio security models and currency hedging models. The API provides CRUD operations for portfolio models, audit history, and authentication via Keycloak (OpenID Connect / OAuth 2.0).

---

## Table of Contents
- [Overview](#overview)
- [Architecture](#architecture)
- [Configuration](#configuration)
  - [appsettings files](#appsettings-files)
  - [Environment variables](#environment-variables)
- [Build](#build)
- [Run (Local Development)](#run-local-development)
- [Testing](#testing)
- [Container (Docker)](#container-docker)
  - [Build image](#build-image)
  - [Run container](#run-container)
  - [Docker Compose example](#docker-compose-example)
- [Swagger / API docs](#swagger--api-docs)
- [Keycloak (Auth) Integration](#keycloak-auth-integration)
- [Troubleshooting](#troubleshooting)

---

## Overview

`PortModelApi` is the backend service for the Portfolio Model Management System. It exposes REST endpoints used by the SvelteKit frontend to:
- Authenticate users via Keycloak (JWT)
- Read portfolio lists
- Create, read, update, delete portfolio models (security and hedging models)
- Provide audit history for models

The project uses Entity Framework Core for database access and SQL Server as the primary datastore.

---

## Architecture

- ASP.NET Core 8 Web API
- Entity Framework Core (SQL Server)
- Keycloak for authentication and token issuance
- CORS policy configured to allow the frontend origin (configurable via `Cors:AllowedOrigins`)
- Swagger for interactive API documentation


---

## Configuration

### appsettings files
- `appsettings.json`: production/general configuration
- `appsettings.Development.json`: development configuration (overrides production when `ASPNETCORE_ENVIRONMENT=Development`)

Key configuration sections used:
- `ConnectionStrings:DefaultConnection` - SQL Server connection string
- `Keycloak` - `Authority`, `ClientId`, `ClientSecret`
- `Cors:AllowedOrigins` - comma-separated allowed frontend origins
- `DatabaseRetry` - retry configuration for database connection attempts

> Note: Do not commit secrets (DB passwords, client secrets) to the repository. Use environment variables or secrets management for production.

### Environment variables (examples)
- `ConnectionStrings__DefaultConnection` - override DB connection
- `Keycloak__Authority` - Keycloak server URL
- `Cors__AllowedOrigins` - e.g., `http://localhost:5173` or `https://app.example.com`
- `ASPNETCORE_ENVIRONMENT` - `Development` or `Production`


---

## Build

Prerequisites:
- .NET 8 SDK installed

To restore and build:

```bash
cd backend/PortModelApi
dotnet restore
dotnet build -c Debug
```

To publish (for container or production):

```bash
dotnet publish -c Release -o ./publish
```


---

## Run (Local Development)

You can run directly with `dotnet`:

```bash
cd backend/PortModelApi
dotnet run
```

By default the app listens on the URL(s) configured by `launchSettings.json` or `builder.WebHost.UseUrls(...)` if present. In this repository the default dev port is typically `http://localhost:5137`.

If you need to override the listening URL:

```bash
# Windows PowerShell example
$env:ASPNETCORE_URLS='http://0.0.0.0:5137'; dotnet run
```


---

## Testing

Unit tests are included in `PortModelApi.Tests` (xUnit).

To run tests:

```bash
cd backend/PortModelApi.Tests
dotnet test
```


---

## Container (Docker)

### Build image (multi-stage Docker recommended)

From repository root (example):

```bash
docker build -t portmodelapi:latest -f backend/PortModelApi/Dockerfile .
```

The project Dockerfile uses a multi-stage build to produce a small runtime image. Check `backend/PortModelApi/Dockerfile` for details and adjust arguments if needed.


### Run container

```bash
docker run --rm -p 5137:5137 \
  -e ConnectionStrings__DefaultConnection="Server=db;Database=model_crud_db;User Id=dbuser;Password=secret;" \
  -e Keycloak__Authority="http://keycloak:8080/realms/model_crud" \
  -e Cors__AllowedOrigins="http://localhost:5173" \
  portmodelapi:latest
```

This exposes the API on port `5137` and passes runtime config via environment variables.

### Docker Compose example

A simplified compose service:

```yaml
services:
  portmodelapi:
    image: portmodelapi:latest
    build:
      context: .
      dockerfile: backend/PortModelApi/Dockerfile
    ports:
      - "5137:5137"
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;Database=model_crud_db;User Id=dbuser;Password=secret;
      - Keycloak__Authority=http://keycloak:8080/realms/model_crud
      - Cors__AllowedOrigins=http://localhost:5173
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:2019-latest
    environment:
      - SA_PASSWORD=YourStrong!Passw0rd
      - ACCEPT_EULA=Y
    ports:
      - "1433:1433"
```


---

## Swagger / API docs

When running in Development the app exposes Swagger UI at:

```
http://localhost:5137/swagger
```

Use this to explore endpoints and test requests.


---

## Keycloak (Auth) Integration

- Keycloak is used as the Authorization Server (OpenID Connect).
- Configure a realm, client (`backend-api`), and roles as required.
- Add the Keycloak `Authority` and `ClientId` to `appsettings` or environment variables.

On a successful login, the frontend will obtain a JWT and attach it to requests to the API (`Authorization: Bearer <token>`). The API validates the token using the `Keycloak:Authority` configuration.


---

## Troubleshooting

- CORS errors: Verify `Cors:AllowedOrigins` includes the frontend origin (e.g., `http://localhost:5173`) or use the Vite proxy during development.
- DB connection errors: Confirm `ConnectionStrings:DefaultConnection` and that SQL Server accepts connections from the API container/host.
- Keycloak login issues: Ensure the realm and client exist and that `Keycloak:Authority` points to the correct server URL.
- Port conflicts: Update `ASPNETCORE_URLS` or Docker port mappings.


---

## Additional Notes

- Do not store secrets in source control; use environment variables or a secrets manager.
- Consider enabling HTTPS redirection and certs in production.
- The project contains a small retry policy to help with transient DB errors during startup.

---

If you'd like, I can also add a short `Makefile` or `scripts` folder with common tasks (`build`, `test`, `docker:build`, `docker:run`) for convenience.
