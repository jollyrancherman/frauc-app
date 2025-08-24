-- Enable PostGIS extension for geospatial features
CREATE EXTENSION IF NOT EXISTS postgis;
CREATE EXTENSION IF NOT EXISTS postgis_topology;

-- Create schema for marketplace data
CREATE SCHEMA IF NOT EXISTS marketplace;

-- Create initial spatial reference system if needed
-- (SRID 4326 is WGS84 - standard for GPS coordinates)
-- This is typically already included in PostGIS but we ensure it exists
INSERT INTO spatial_ref_sys (srid, auth_name, auth_srid, proj4text, srtext)
SELECT 4326, 'EPSG', 4326,
       '+proj=longlat +ellps=WGS84 +datum=WGS84 +no_defs ',
       'GEOGCS["WGS 84",DATUM["WGS_1984",SPHEROID["WGS 84",6378137,298.257223563,AUTHORITY["EPSG","7030"]],AUTHORITY["EPSG","6326"]],PRIMEM["Greenwich",0,AUTHORITY["EPSG","8901"]],UNIT["degree",0.01745329251994328,AUTHORITY["EPSG","9122"]],AUTHORITY["EPSG","4326"]]'
WHERE NOT EXISTS (SELECT 1 FROM spatial_ref_sys WHERE srid = 4326);

-- Grant permissions
GRANT USAGE ON SCHEMA marketplace TO dev_user;
GRANT CREATE ON SCHEMA marketplace TO dev_user;