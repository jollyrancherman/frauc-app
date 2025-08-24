#!/bin/bash

# Setup Keycloak for Marketplace Platform
# This script configures Keycloak with the marketplace realm and necessary clients

KEYCLOAK_URL="http://localhost:8080"
KEYCLOAK_ADMIN="admin"
KEYCLOAK_ADMIN_PASSWORD="admin123"
REALM_FILE="keycloak-realm-config.json"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Setting up Keycloak for Marketplace Platform...${NC}"

# Wait for Keycloak to be ready
echo -e "${YELLOW}Waiting for Keycloak to be ready...${NC}"
until curl -s -f -o /dev/null "${KEYCLOAK_URL}/health/ready"; do
  echo -n "."
  sleep 5
done
echo -e "\n${GREEN}Keycloak is ready!${NC}"

# Get admin token
echo -e "${YELLOW}Obtaining admin access token...${NC}"
TOKEN=$(curl -s -X POST "${KEYCLOAK_URL}/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "username=${KEYCLOAK_ADMIN}" \
  -d "password=${KEYCLOAK_ADMIN_PASSWORD}" \
  -d "grant_type=password" \
  -d "client_id=admin-cli" \
  | jq -r '.access_token')

if [ -z "$TOKEN" ] || [ "$TOKEN" == "null" ]; then
  echo -e "${RED}Failed to obtain admin token. Make sure Keycloak is running and credentials are correct.${NC}"
  exit 1
fi

echo -e "${GREEN}Admin token obtained successfully!${NC}"

# Check if realm already exists
REALM_EXISTS=$(curl -s -o /dev/null -w "%{http_code}" \
  -H "Authorization: Bearer ${TOKEN}" \
  "${KEYCLOAK_URL}/admin/realms/marketplace")

if [ "$REALM_EXISTS" == "200" ]; then
  echo -e "${YELLOW}Marketplace realm already exists. Updating...${NC}"
  # Update existing realm
  curl -s -X PUT "${KEYCLOAK_URL}/admin/realms/marketplace" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json" \
    -d @"${REALM_FILE}"
else
  echo -e "${YELLOW}Creating marketplace realm...${NC}"
  # Create new realm
  curl -s -X POST "${KEYCLOAK_URL}/admin/realms" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json" \
    -d @"${REALM_FILE}"
fi

if [ $? -eq 0 ]; then
  echo -e "${GREEN}Marketplace realm configured successfully!${NC}"
else
  echo -e "${RED}Failed to configure marketplace realm.${NC}"
  exit 1
fi

# Create sample users for testing
echo -e "${YELLOW}Creating sample users for testing...${NC}"

# Function to create a user
create_user() {
  local username=$1
  local email=$2
  local firstName=$3
  local lastName=$4
  local password=$5
  local role=$6

  # Create user
  curl -s -X POST "${KEYCLOAK_URL}/admin/realms/marketplace/users" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json" \
    -d '{
      "username": "'"${username}"'",
      "email": "'"${email}"'",
      "firstName": "'"${firstName}"'",
      "lastName": "'"${lastName}"'",
      "enabled": true,
      "emailVerified": true,
      "credentials": [{
        "type": "password",
        "value": "'"${password}"'",
        "temporary": false
      }]
    }'

  # Get user ID
  USER_ID=$(curl -s "${KEYCLOAK_URL}/admin/realms/marketplace/users?username=${username}" \
    -H "Authorization: Bearer ${TOKEN}" \
    | jq -r '.[0].id')

  # Get role ID
  ROLE_ID=$(curl -s "${KEYCLOAK_URL}/admin/realms/marketplace/roles/${role}" \
    -H "Authorization: Bearer ${TOKEN}" \
    | jq -r '.id')

  # Assign role to user
  curl -s -X POST "${KEYCLOAK_URL}/admin/realms/marketplace/users/${USER_ID}/role-mappings/realm" \
    -H "Authorization: Bearer ${TOKEN}" \
    -H "Content-Type: application/json" \
    -d '[{
      "id": "'"${ROLE_ID}"'",
      "name": "'"${role}"'"
    }]'

  echo -e "${GREEN}Created user: ${username} with role: ${role}${NC}"
}

# Create test users
create_user "buyer1" "buyer1@test.com" "John" "Buyer" "Test123!" "buyer"
create_user "seller1" "seller1@test.com" "Jane" "Seller" "Test123!" "seller"
create_user "business1" "business1@test.com" "Business" "Owner" "Test123!" "business_seller"
create_user "premium1" "premium1@test.com" "Premium" "Seller" "Test123!" "premium_seller"
create_user "moderator1" "moderator1@test.com" "Mike" "Moderator" "Test123!" "moderator"
create_user "admin1" "admin1@test.com" "Admin" "User" "Test123!" "admin"

echo -e "${GREEN}✅ Keycloak setup completed successfully!${NC}"
echo -e "${YELLOW}You can access Keycloak at: ${KEYCLOAK_URL}${NC}"
echo -e "${YELLOW}Admin console: ${KEYCLOAK_URL}/admin${NC}"
echo -e "${YELLOW}Username: ${KEYCLOAK_ADMIN}, Password: ${KEYCLOAK_ADMIN_PASSWORD}${NC}"
echo ""
echo -e "${GREEN}Test users created:${NC}"
echo "  buyer1@test.com (buyer role)"
echo "  seller1@test.com (seller role)"
echo "  business1@test.com (business_seller role)"
echo "  premium1@test.com (premium_seller role)"
echo "  moderator1@test.com (moderator role)"
echo "  admin1@test.com (admin role)"
echo "  Password for all: Test123!"