namespace Marketplace.Domain.Listings;

/// <summary>
/// Domain constants and business rules for listings
/// </summary>
public static class ListingConstants
{
    /// <summary>
    /// Default expiration period for free listings
    /// </summary>
    public static readonly TimeSpan FreeListingDuration = TimeSpan.FromDays(30);
    
    /// <summary>
    /// Minimum auction duration
    /// </summary>
    public static readonly TimeSpan MinimumAuctionDuration = TimeSpan.FromHours(1);
    
    /// <summary>
    /// Maximum auction duration
    /// </summary>
    public static readonly TimeSpan MaximumAuctionDuration = TimeSpan.FromDays(30);
    
    /// <summary>
    /// Default auction duration
    /// </summary>
    public static readonly TimeSpan DefaultAuctionDuration = TimeSpan.FromDays(7);
    
    /// <summary>
    /// Minimum bid increment percentage (e.g., 0.05 = 5%)
    /// </summary>
    public const decimal MinimumBidIncrementPercentage = 0.05m;
    
    /// <summary>
    /// Minimum absolute bid increment in cents
    /// </summary>
    public const int MinimumBidIncrementCents = 100; // $1.00
    
    /// <summary>
    /// Maximum title length
    /// </summary>
    public const int MaxTitleLength = 200;
    
    /// <summary>
    /// Maximum description length
    /// </summary>
    public const int MaxDescriptionLength = 5000;
    
    /// <summary>
    /// Auction extension time when bid placed near end
    /// </summary>
    public static readonly TimeSpan AuctionExtensionTime = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Time before auction end to trigger extension
    /// </summary>
    public static readonly TimeSpan AuctionExtensionTriggerTime = TimeSpan.FromMinutes(5);
}