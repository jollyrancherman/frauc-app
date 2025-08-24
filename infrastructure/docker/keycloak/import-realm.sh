#!/bin/bash

# Keycloak realm import script
# This script imports the marketplace realm configuration into Keycloak

KEYCLOAK_URL="http://localhost:8080"
KEYCLOAK_ADMIN="admin"
KEYCLOAK_ADMIN_PASSWORD="admin123"
REALM_FILE="/keycloak/import/realm-export.json"

echo "🔐 Importing Keycloak Marketplace Realm Configuration..."

# Wait for Keycloak to be ready
echo "⏳ Waiting for Keycloak to be ready..."
until curl -s -f -o /dev/null "${KEYCLOAK_URL}/health/ready"; do
    echo "   Keycloak is not ready yet, waiting..."
    sleep 5
done

echo "✅ Keycloak is ready!"

# Get admin access token
echo "🔑 Getting admin access token..."
ACCESS_TOKEN=$(curl -s -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    -d "username=${KEYCLOAK_ADMIN}" \
    -d "password=${KEYCLOAK_ADMIN_PASSWORD}" \
    -d "grant_type=password" \
    -d "client_id=admin-cli" | jq -r '.access_token')

if [ -z "$ACCESS_TOKEN" ] || [ "$ACCESS_TOKEN" = "null" ]; then
    echo "❌ Failed to get access token. Please check admin credentials."
    exit 1
fi

echo "✅ Access token obtained!"

# Check if realm already exists
echo "🔍 Checking if marketplace realm already exists..."
REALM_EXISTS=$(curl -s -o /dev/null -w "%{http_code}" \
    -H "Authorization: Bearer ${ACCESS_TOKEN}" \
    "${KEYCLOAK_URL}/admin/realms/marketplace")

if [ "$REALM_EXISTS" = "200" ]; then
    echo "⚠️  Marketplace realm already exists. Skipping import."
    echo "   To reimport, delete the existing realm first."
else
    echo "📥 Importing marketplace realm..."
    
    # Import the realm
    IMPORT_RESPONSE=$(curl -s -o /dev/null -w "%{http_code}" -X POST \
        -H "Authorization: Bearer ${ACCESS_TOKEN}" \
        -H "Content-Type: application/json" \
        -d @"${REALM_FILE}" \
        "${KEYCLOAK_URL}/admin/realms")
    
    if [ "$IMPORT_RESPONSE" = "201" ]; then
        echo "✅ Marketplace realm imported successfully!"
        echo ""
        echo "📋 Realm Details:"
        echo "   - Realm Name: marketplace"
        echo "   - Clients:"
        echo "     • user-api (Backend service)"
        echo "     • marketplace-web (Frontend app)"
        echo "     • marketplace-mobile (Mobile app)"
        echo "   - Roles:"
        echo "     • buyer (default for new users)"
        echo "     • seller"
        echo "     • verified_seller"
        echo "     • moderator"
        echo "     • platform_admin"
        echo "   - Test Users:"
        echo "     • testbuyer / Test123!@# (buyer role)"
        echo "     • testseller / Test123!@# (seller role)"
        echo "     • testadmin / Admin123!@# (admin role)"
        echo ""
        echo "🌐 Access URLs:"
        echo "   - Admin Console: ${KEYCLOAK_URL}/admin"
        echo "   - Account Console: ${KEYCLOAK_URL}/realms/marketplace/account"
        echo "   - OpenID Config: ${KEYCLOAK_URL}/realms/marketplace/.well-known/openid-configuration"
    else
        echo "❌ Failed to import realm. Response code: ${IMPORT_RESPONSE}"
        exit 1
    fi
fi

echo ""
echo "🎉 Keycloak setup complete!"