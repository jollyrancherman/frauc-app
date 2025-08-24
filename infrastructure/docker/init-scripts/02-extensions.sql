-- Enable PostGIS extension for geospatial features
CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS postgis_topology;

-- Create schemas for different services
CREATE SCHEMA IF NOT EXISTS user_service;
CREATE SCHEMA IF NOT EXISTS product_service;
CREATE SCHEMA IF NOT EXISTS listing_service;
CREATE SCHEMA IF NOT EXISTS bidding_service;
CREATE SCHEMA IF NOT EXISTS payment_service;
CREATE SCHEMA IF NOT EXISTS messaging_service;
CREATE SCHEMA IF NOT EXISTS notification_service;

-- Set up default permissions
GRANT USAGE ON SCHEMA user_service TO marketplace_user;
GRANT CREATE ON SCHEMA user_service TO marketplace_user;
GRANT USAGE ON SCHEMA product_service TO marketplace_user;
GRANT CREATE ON SCHEMA product_service TO marketplace_user;
GRANT USAGE ON SCHEMA listing_service TO marketplace_user;
GRANT CREATE ON SCHEMA listing_service TO marketplace_user;
GRANT USAGE ON SCHEMA bidding_service TO marketplace_user;
GRANT CREATE ON SCHEMA bidding_service TO marketplace_user;
GRANT USAGE ON SCHEMA payment_service TO marketplace_user;
GRANT CREATE ON SCHEMA payment_service TO marketplace_user;
GRANT USAGE ON SCHEMA messaging_service TO marketplace_user;
GRANT CREATE ON SCHEMA messaging_service TO marketplace_user;
GRANT USAGE ON SCHEMA notification_service TO marketplace_user;
GRANT CREATE ON SCHEMA notification_service TO marketplace_user;