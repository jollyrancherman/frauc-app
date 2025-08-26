using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Infrastructure.Persistence;
using Location = Marketplace.Domain.Listings.ValueObjects.Location;

namespace Marketplace.Infrastructure.Persistence.Repositories;

public class ListingRepository : IListingRepository
{
    private readonly ApplicationDbContext _context;

    public ListingRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Listing?> GetByIdAsync(ListingId id, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id && !l.IsDeleted, cancellationToken);
    }

    public async Task<Listing?> GetByItemIdAsync(ItemId itemId, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.ItemId == itemId && !l.IsDeleted, cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetBySellerIdAsync(UserId sellerId, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.SellerId == sellerId && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetByCategoryIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.CategoryId == categoryId && !l.IsDeleted && l.Status == ListingStatus.Active)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        await _context.Listings.AddAsync(listing, cancellationToken);
    }

    public Task UpdateAsync(Listing listing, CancellationToken cancellationToken = default)
    {
        _context.Listings.Update(listing);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(ListingId id, CancellationToken cancellationToken = default)
    {
        var listing = await _context.Listings.FindAsync(new object[] { id }, cancellationToken);
        if (listing != null)
        {
            _context.Listings.Remove(listing);
        }
    }

    public async Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Active && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetExpiredListingsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.ExpiresAt.HasValue && l.ExpiresAt <= now && l.Status == ListingStatus.Active)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetListingsByTypeAsync(ListingType type, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.ListingType == type && l.Status == ListingStatus.Active && !l.IsDeleted)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetNearbyListingsAsync(
        Location center, 
        double radiusKm, 
        CancellationToken cancellationToken = default)
    {
        // Convert Location to NetTopologySuite Point for PostGIS queries
        var centerPoint = new Point(center.Longitude, center.Latitude) { SRID = 4326 };
        var radiusMeters = radiusKm * 1000;

        return await _context.Listings
            .AsNoTracking()
            .Where(l => l.Status == ListingStatus.Active && 
                       !l.IsDeleted &&
                       l.LocationPoint.Distance(centerPoint) <= radiusMeters)
            .OrderBy(l => l.LocationPoint.Distance(centerPoint))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Listing>> GetListingsInBoundingBoxAsync(
        double minLat, 
        double minLon, 
        double maxLat, 
        double maxLon, 
        CancellationToken cancellationToken = default)
    {
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
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(ListingId id, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .AnyAsync(l => l.Id == id && !l.IsDeleted, cancellationToken);
    }

    public async Task<int> CountBySellerAsync(UserId sellerId, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .CountAsync(l => l.SellerId == sellerId && !l.IsDeleted, cancellationToken);
    }

    public async Task<int> CountByCategoryAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .CountAsync(l => l.CategoryId == categoryId && l.Status == ListingStatus.Active && !l.IsDeleted, cancellationToken);
    }

    public async Task<int> CountActiveListingsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Listings
            .AsNoTracking()
            .CountAsync(l => l.Status == ListingStatus.Active && !l.IsDeleted, cancellationToken);
    }

    public async Task<(IEnumerable<Listing> Listings, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        ListingType? type = null, 
        ListingStatus? status = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _context.Listings.AsNoTracking().Where(l => !l.IsDeleted);

        if (type.HasValue)
            query = query.Where(l => l.ListingType == type.Value);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (listings, totalCount);
    }

    public async Task<(IEnumerable<Listing> Listings, int TotalCount)> GetListingsBySellerAsync(
        UserId sellerId,
        int pageNumber,
        int pageSize,
        ListingStatus? status = null,
        ListingType? type = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Listings.AsNoTracking()
            .Where(l => l.SellerId == sellerId && !l.IsDeleted);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (type.HasValue)
            query = query.Where(l => l.ListingType == type.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (listings, totalCount);
    }

    public async Task<(IEnumerable<Listing> Listings, int TotalCount)> GetListingsByCategoryAsync(
        CategoryId categoryId,
        int pageNumber,
        int pageSize,
        ListingStatus? status = null,
        ListingType? type = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Listings.AsNoTracking()
            .Where(l => l.CategoryId == categoryId && !l.IsDeleted);

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (type.HasValue)
            query = query.Where(l => l.ListingType == type.Value);

        var totalCount = await query.CountAsync(cancellationToken);
        
        var listings = await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (listings, totalCount);
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
        var query = _context.Listings.AsNoTracking()
            .Where(l => !l.IsDeleted);

        // Text search
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(l => EF.Functions.ILike(l.Title, $"%{searchTerm}%") ||
                                   EF.Functions.ILike(l.Description, $"%{searchTerm}%"));
        }

        // Category filter
        if (categoryId != null)
            query = query.Where(l => l.CategoryId == categoryId);

        // Status filter
        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);
        else
            query = query.Where(l => l.Status == ListingStatus.Active); // Default to active

        // Type filter
        if (type.HasValue)
            query = query.Where(l => l.ListingType == type.Value);

        // Price range filter
        if (minPrice != null)
            query = query.Where(l => l.CurrentPrice.Amount >= minPrice.Amount);

        if (maxPrice != null)
            query = query.Where(l => l.CurrentPrice.Amount <= maxPrice.Amount);

        // Spatial filter
        if (center != null && radiusKm.HasValue)
        {
            var centerPoint = new Point(center.Longitude, center.Latitude) { SRID = 4326 };
            var radiusMeters = radiusKm.Value * 1000;
            query = query.Where(l => l.LocationPoint.Distance(centerPoint) <= radiusMeters);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Sorting
        query = sortBy.ToLowerInvariant() switch
        {
            "price" => sortDirection.ToUpperInvariant() == "ASC" 
                ? query.OrderBy(l => l.CurrentPrice.Amount)
                : query.OrderByDescending(l => l.CurrentPrice.Amount),
            "distance" when center != null => 
                query.OrderBy(l => l.LocationPoint.Distance(new Point(center.Longitude, center.Latitude) { SRID = 4326 })),
            "title" => sortDirection.ToUpperInvariant() == "ASC"
                ? query.OrderBy(l => l.Title)
                : query.OrderByDescending(l => l.Title),
            _ => sortDirection.ToUpperInvariant() == "ASC"
                ? query.OrderBy(l => l.CreatedAt)
                : query.OrderByDescending(l => l.CreatedAt)
        };

        var listings = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (listings, totalCount);
    }
}