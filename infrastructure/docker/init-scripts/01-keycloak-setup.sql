-- Create Keycloak database and user if they don't exist
SELECT 'CREATE DATABASE keycloak' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'keycloak')\gexec

-- Create user for Keycloak if it doesn't exist
DO
$do$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_catalog.pg_roles
      WHERE  rolname = 'keycloak_user') THEN
      
      CREATE ROLE keycloak_user LOGIN PASSWORD 'keycloak_pass';
   END IF;
END
$do$;

-- Grant privileges to keycloak user
GRANT ALL PRIVILEGES ON DATABASE keycloak TO keycloak_user;