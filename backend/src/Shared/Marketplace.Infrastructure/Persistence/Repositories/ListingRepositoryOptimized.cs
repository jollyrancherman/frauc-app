using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Infrastructure.Persistence;
using Location = Marketplace.Domain.Listings.ValueObjects.Location;

namespace Marketplace.Infrastructure.Persistence.Repositories;

public class ListingRepositoryOptimized : IListingRepository
{
    private readonly ApplicationDbContext _context;
    private const int MaxSpatialQueryResults = 1000; // Safety limit for spatial queries
    private const double MaxSearchRadiusKm = 100; // Limit search radius

    public ListingRepositoryOptimized(ApplicationDbContext context)
    {
        _context = context;
    }

    // ... [Previous basic CRUD methods remain the same] ...

    public async Task<IEnumerable<Listing>> GetNearbyListingsAsync(
        Location center, 
        double radiusKm, 
        CancellationToken cancellationToken = default)
    {
        // Security: Limit search radius to prevent abuse
        radiusKm = Math.Min(radiusKm, MaxSearchRadiusKm);
        
        // Convert Location to NetTopologySuite Point for PostGIS queries
        var centerPoint = new Point(center.Longitude, center.Latitude) { SRID = 4326 };
        var radiusMeters = radiusKm * 1000;

        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Active && 
                       !l.IsDeleted &&
                       l.LocationPoint.Distance(centerPoint) <= radiusMeters)
            .OrderBy(l => l.LocationPoint.Distance(centerPoint))
            .Take(MaxSpatialQueryResults) // Performance: Limit results
            .Select(l => new Listing // Projection to avoid loading all data
            {
                // Only load essential fields for spatial queries
                Id = l.Id,
                Title = l.Title,
                CurrentPrice = l.CurrentPrice,
                Location = l.Location,
                CreatedAt = l.CreatedAt,
                Status = l.Status,
                ListingType = l.ListingType
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetListingsInBoundingBoxAsync(
        double minLat, 
        double minLon, 
        double maxLat, 
        double maxLon, 
        CancellationToken cancellationToken = default)
    {
        // Validate bounding box size to prevent abuse
        var latDiff = Math.Abs(maxLat - minLat);
        var lonDiff = Math.Abs(maxLon - minLon);
        
        if (latDiff > 10 || lonDiff > 10) // Max 10 degrees
        {
            throw new ArgumentException("Bounding box too large for efficient querying");
        }

        // Create bounding box geometry
        var coordinates = new[]
        {
            new Coordinate(minLon, minLat),
            new Coordinate(maxLon, minLat),
            new Coordinate(maxLon, maxLat),
            new Coordinate(minLon, maxLat),
            new Coordinate(minLon, minLat)
        };
        
        var boundingBox = new Polygon(new LinearRing(coordinates)) { SRID = 4326 };

        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Active && 
                       !l.IsDeleted &&
                       boundingBox.Contains(l.LocationPoint))
            .OrderByDescending(l => l.CreatedAt)
            .Take(MaxSpatialQueryResults)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Listing> Listings, int TotalCount)> SearchListingsAsync(
        string? searchTerm,
        CategoryId? categoryId,
        Location? center,
        double? radiusKm,
        Money? minPrice,
        Money? maxPrice,
        ListingType? type,
        ListingStatus? status,
        int pageNumber,
        int pageSize,
        string sortBy,
        string sortDirection,
        CancellationToken cancellationToken = default)
    {
        // Input sanitization
        searchTerm = SanitizeSearchTerm(searchTerm);
        pageSize = Math.Min(pageSize, 100); // Max 100 results per page
        pageNumber = Math.Max(1, pageNumber);
        
        if (radiusKm.HasValue)
        {
            radiusKm = Math.Min(radiusKm.Value, MaxSearchRadiusKm);
        }

        var query = _context.Listings.AsNoTracking()
            .Where(l => !l.IsDeleted);

        // Text search with Full-Text Search (safer than ILIKE)
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            // Use PostgreSQL Full-Text Search instead of ILIKE for better performance and security
            query = query.Where(l => EF.Functions.ToTsVector("english", l.Title + " " + l.Description)
                                   .Matches(EF.Functions.ToTsQuery("english", searchTerm)));
        }

        // Apply filters
        query = ApplyFilters(query, categoryId, status, type, minPrice, maxPrice);

        // Spatial filtering
        if (center != null && radiusKm.HasValue)
        {
            var centerPoint = new Point(center.Longitude, center.Latitude) { SRID = 4326 };
            var radiusMeters = radiusKm.Value * 1000;
            query = query.Where(l => l.LocationPoint.Distance(centerPoint) <= radiusMeters);
        }

        // Get total count before pagination (for performance, limit to reasonable maximum)
        var totalCount = await query.Take(10000).CountAsync(cancellationToken);

        // Apply sorting and pagination
        query = ApplySorting(query, sortBy, sortDirection, center);
        
        var listings = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (listings, totalCount);
    }

    private static string? SanitizeSearchTerm(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return null;

        // Remove potentially dangerous characters and limit length
        searchTerm = searchTerm.Trim()
            .Replace("'", "")
            .Replace("\"", "")
            .Replace(";", "")
            .Replace("--", "")
            .Replace("/*", "")
            .Replace("*/", "");

        return searchTerm.Length > 100 ? searchTerm[..100] : searchTerm;
    }

    private static IQueryable<Listing> ApplyFilters(
        IQueryable<Listing> query,
        CategoryId? categoryId,
        ListingStatus? status,
        ListingType? type,
        Money? minPrice,
        Money? maxPrice)
    {
        if (categoryId != null)
            query = query.Where(l => l.CategoryId == categoryId);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);
        else
            query = query.Where(l => l.Status == ListingStatus.Active); // Default to active

        if (type.HasValue)
            query = query.Where(l => l.ListingType == type.Value);

        if (minPrice != null)
            query = query.Where(l => l.CurrentPrice.Amount >= minPrice.Amount);

        if (maxPrice != null)
            query = query.Where(l => l.CurrentPrice.Amount <= maxPrice.Amount);

        return query;
    }

    private static IQueryable<Listing> ApplySorting(
        IQueryable<Listing> query,
        string sortBy,
        string sortDirection,
        Location? center)
    {
        return sortBy.ToLowerInvariant() switch
        {
            "price" => sortDirection.ToUpperInvariant() == "ASC"
                ? query.OrderBy(l => l.CurrentPrice.Amount)
                : query.OrderByDescending(l => l.CurrentPrice.Amount),
            "distance" when center != null =>
                query.OrderBy(l => l.LocationPoint.Distance(
                    new Point(center.Longitude, center.Latitude) { SRID = 4326 })),
            "title" => sortDirection.ToUpperInvariant() == "ASC"
                ? query.OrderBy(l => l.Title)
                : query.OrderByDescending(l => l.Title),
            _ => sortDirection.ToUpperInvariant() == "ASC"
                ? query.OrderBy(l => l.CreatedAt)
                : query.OrderByDescending(l => l.CreatedAt)
        };
    }

    // ... [Rest of the basic CRUD methods remain the same] ...
}