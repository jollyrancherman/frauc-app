namespace Marketplace.Domain.Listings.ValueObjects;

public enum ListingStatus
{
    /// <summary>
    /// Listing is active and available
    /// </summary>
    Active,

    /// <summary>
    /// Listing has been completed (sold/won)
    /// </summary>
    Completed,

    /// <summary>
    /// Listing has expired without completion
    /// </summary>
    Expired,

    /// <summary>
    /// Listing has been cancelled by seller
    /// </summary>
    Cancelled,

    /// <summary>
    /// Listing has been suspended by admin
    /// </summary>
    Suspended
}