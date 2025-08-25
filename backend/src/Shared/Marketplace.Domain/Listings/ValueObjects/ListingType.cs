namespace Marketplace.Domain.Listings.ValueObjects;

public enum ListingType
{
    /// <summary>
    /// Free listing that expires in 30 days
    /// </summary>
    Free,

    /// <summary>
    /// Free initially, converts to forward auction on first bid
    /// </summary>
    FreeToAuction,

    /// <summary>
    /// Forward auction where highest bidder wins
    /// </summary>
    ForwardAuction,

    /// <summary>
    /// Reverse auction where lowest bidder wins
    /// </summary>
    ReverseAuction,

    /// <summary>
    /// Fixed price listing for immediate purchase
    /// </summary>
    FixedPrice
}