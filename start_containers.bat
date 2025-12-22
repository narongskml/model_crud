@echo off
SETLOCAL EnableDelayedExpansion

:: --- Configuration ---
SET BACKEND_IMAGE=model_crud_backend:1.0.0
SET FRONTEND_IMAGE=model_crud_frontend:bun
SET NETWORK=tomlab-network
SET DB_CONN=Server=DBSERVER;Database=model_crud_db;User Id=dbuser1;Password=Secret123;Encrypt=False;TrustServerCertificate=True;
SET KEYCLOAK_AUTH=http://host.containers.internal:8080/realms/model_crud
SET KEYCLOAK_CLIENT_ID=backend-api
SET KEYCLOAK_CLIENT_SECRET=your_keycloak_client_secret
echo [1/4] Ensuring network !NETWORK! exists...
podman network inspect !NETWORK! >nul 2>&1
if !errorlevel! neq 0 (
    podman network create !NETWORK!
)

echo [2/4] Stopping existing containers if any...
podman stop model-backend model-frontend >nul 2>&1
podman rm model-backend model-frontend >nul 2>&1

echo [3/4] Starting Backend Container...
:: We map port 5137 as expected by frontend code
podman run -d ^
    --name model-backend ^
    --network !NETWORK! ^
    --add-host host.containers.internal:host-gateway ^
    -p 5137:8080 ^
    -e ASPNETCORE_ENVIRONMENT=Production ^
    -e ConnectionStrings__DefaultConnection="!DB_CONN!" ^
    -e Keycloak__Authority="!KEYCLOAK_AUTH!" ^
    -e Keycloak__ClientId="!KEYCLOAK_CLIENT_ID!" ^
    -e Keycloak__ClientSecret="!KEYCLOAK_CLIENT_SECRET!" ^
    !BACKEND_IMAGE!

echo [4/4] Starting Frontend Container...
:: Frontend code expects backend at localhost:5137 (browser-side fetch)
podman run -d ^
    --name model-frontend ^
    --network !NETWORK! ^
    -p 5173:3000 ^
    -e ORIGIN=http://localhost:5173 ^
    !FRONTEND_IMAGE!

echo.
echo All containers started!
echo Frontend: http://localhost:5173
echo Backend:  http://localhost:5137/api
echo.
echo Use "podman logs -f model-frontend" or "podman logs -f model-backend" to view logs.
pause
