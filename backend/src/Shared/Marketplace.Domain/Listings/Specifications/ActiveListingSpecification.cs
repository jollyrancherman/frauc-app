using System.Linq.Expressions;
using Marketplace.Domain.Common;
using Marketplace.Domain.Listings.ValueObjects;

namespace Marketplace.Domain.Listings.Specifications;

public class ActiveListingSpecification : Specification<Listing>
{
    public override Expression<Func<Listing, bool>> ToExpression()
    {
        return listing => listing.Status == ListingStatus.Active 
                       && !listing.IsDeleted
                       && (listing.ExpiresAt == null || listing.ExpiresAt > DateTime.UtcNow);
    }
}

public class ListingByTypeSpecification : Specification<Listing>
{
    private readonly ListingType _type;
    
    public ListingByTypeSpecification(ListingType type)
    {
        _type = type;
    }
    
    public override Expression<Func<Listing, bool>> ToExpression()
    {
        return listing => listing.ListingType == _type;
    }
}

public class ListingNearLocationSpecification : Specification<Listing>
{
    private readonly Location _center;
    private readonly double _radiusKm;
    
    public ListingNearLocationSpecification(Location center, double radiusKm)
    {
        _center = center;
        _radiusKm = radiusKm;
    }
    
    public override Expression<Func<Listing, bool>> ToExpression()
    {
        // Note: This is a simplified version. 
        // In production, you'd use PostGIS ST_DWithin or similar spatial functions
        return listing => listing.Location.DistanceTo(_center) <= _radiusKm;
    }
}

public class ListingBySellerSpecification : Specification<Listing>
{
    private readonly Guid _sellerId;
    
    public ListingBySellerSpecification(Guid sellerId)
    {
        _sellerId = sellerId;
    }
    
    public override Expression<Func<Listing, bool>> ToExpression()
    {
        return listing => listing.SellerId.Value == _sellerId;
    }
}