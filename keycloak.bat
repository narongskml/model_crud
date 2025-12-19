podman run -d ^
  --name keycloak ^
  --restart unless-stopped ^
  --network tomlab-network ^
  -e KEYCLOAK_ADMIN=admin ^
  -e KEYCLOAK_ADMIN_PASSWORD=admin123 ^
  -e KC_DB=dev-file ^
  -v keycloak_data:/opt/keycloak/data ^
  -p 8080:8080 ^
  quay.io/keycloak/keycloak:26.4.6 ^
  start-dev
