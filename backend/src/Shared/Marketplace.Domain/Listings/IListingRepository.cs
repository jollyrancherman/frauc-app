using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Domain.Listings;

public interface IListingRepository
{
    // Basic CRUD operations
    Task<Listing?> GetByIdAsync(ListingId id, CancellationToken cancellationToken = default);
    Task<Listing?> GetByItemIdAsync(ItemId itemId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetBySellerIdAsync(UserId sellerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetByCategoryIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default);
    Task AddAsync(Listing listing, CancellationToken cancellationToken = default);
    Task UpdateAsync(Listing listing, CancellationToken cancellationToken = default);
    Task DeleteAsync(ListingId id, CancellationToken cancellationToken = default);

    // Query operations
    Task<IEnumerable<Listing>> GetActiveListingsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetExpiredListingsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Listing>> GetListingsByTypeAsync(ListingType type, CancellationToken cancellationToken = default);
    
    // Spatial queries (PostGIS)
    Task<IEnumerable<Listing>> GetNearbyListingsAsync(
        Location center, 
        double radiusKm, 
        CancellationToken cancellationToken = default);
    
    Task<IEnumerable<Listing>> GetListingsInBoundingBoxAsync(
        double minLat, 
        double minLon, 
        double maxLat, 
        double maxLon,
        CancellationToken cancellationToken = default);

    // Performance optimized methods
    Task<bool> ExistsAsync(ListingId id, CancellationToken cancellationToken = default);
    Task<int> CountBySellerAsync(UserId sellerId, CancellationToken cancellationToken = default);
    Task<int> CountByCategoryAsync(CategoryId categoryId, CancellationToken cancellationToken = default);
    Task<int> CountActiveListingsAsync(CancellationToken cancellationToken = default);

    // Pagination support
    Task<(IEnumerable<Listing> Listings, int TotalCount)> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        ListingType? type = null,
        ListingStatus? status = null,
        CancellationToken cancellationToken = default);

    // Advanced query methods
    Task<(IEnumerable<Listing> Listings, int TotalCount)> GetListingsBySellerAsync(
        UserId sellerId,
        int pageNumber,
        int pageSize,
        ListingStatus? status = null,
        ListingType? type = null,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<Listing> Listings, int TotalCount)> GetListingsByCategoryAsync(
        CategoryId categoryId,
        int pageNumber,
        int pageSize,
        ListingStatus? status = null,
        ListingType? type = null,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<Listing> Listings, int TotalCount)> SearchListingsAsync(
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
        CancellationToken cancellationToken = default);
}