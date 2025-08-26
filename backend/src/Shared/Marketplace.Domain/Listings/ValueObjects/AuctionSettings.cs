using Marketplace.Domain.Common;

namespace Marketplace.Domain.Listings.ValueObjects;

public sealed class AuctionSettings : ValueObject
{
    public Money? StartingPrice { get; }
    public Money? ReservePrice { get; }
    public Money? MaxPrice { get; }
    public Money? BuyNowPrice { get; }
    public Money? MinimumBidIncrement { get; }
    public TimeSpan Duration { get; }
    public bool AllowAutoBidding { get; }

    private AuctionSettings(
        Money? startingPrice, 
        Money? reservePrice, 
        Money? maxPrice, 
        Money? buyNowPrice,
        Money? minimumBidIncrement,
        TimeSpan duration,
        bool allowAutoBidding = true)
    {
        if (duration <= TimeSpan.Zero)
            throw new ArgumentException("Duration must be positive", nameof(duration));
        
        if (duration < ListingConstants.MinimumAuctionDuration)
            throw new ArgumentException($"Duration must be at least {ListingConstants.MinimumAuctionDuration}", nameof(duration));
        
        if (duration > ListingConstants.MaximumAuctionDuration)
            throw new ArgumentException($"Duration cannot exceed {ListingConstants.MaximumAuctionDuration}", nameof(duration));

        StartingPrice = startingPrice;
        ReservePrice = reservePrice;
        MaxPrice = maxPrice;
        BuyNowPrice = buyNowPrice;
        MinimumBidIncrement = minimumBidIncrement;
        Duration = duration;
        AllowAutoBidding = allowAutoBidding;
    }

    public static AuctionSettings CreateForwardAuction(
        Money startingPrice, 
        Money? reservePrice, 
        TimeSpan duration,
        Money? buyNowPrice = null,
        bool allowAutoBidding = true)
    {
        if (reservePrice != null && reservePrice < startingPrice)
            throw new ArgumentException("Reserve price must be greater than or equal to starting price");
        
        if (buyNowPrice != null && buyNowPrice < startingPrice)
            throw new ArgumentException("Buy now price must be greater than starting price");

        var minimumIncrement = CalculateMinimumBidIncrement(startingPrice);
        return new AuctionSettings(startingPrice, reservePrice, null, buyNowPrice, minimumIncrement, duration, allowAutoBidding);
    }

    public static AuctionSettings CreateReverseAuction(Money maxPrice, TimeSpan duration)
    {
        var minimumIncrement = CalculateMinimumBidIncrement(maxPrice);
        return new AuctionSettings(null, null, maxPrice, null, minimumIncrement, duration, false);
    }

    public static AuctionSettings CreateFreeToAuction(TimeSpan duration)
    {
        return new AuctionSettings(Money.Zero, null, null, null, Money.FromCents(ListingConstants.MinimumBidIncrementCents), duration, true);
    }
    
    private static Money CalculateMinimumBidIncrement(Money basePrice)
    {
        var percentageIncrement = basePrice.Amount * ListingConstants.MinimumBidIncrementPercentage;
        var minCentsIncrement = ListingConstants.MinimumBidIncrementCents / 100m;
        var increment = Math.Max(percentageIncrement, minCentsIncrement);
        return new Money(Math.Round(increment, 2), basePrice.Currency);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return StartingPrice ?? Money.Zero;
        yield return ReservePrice ?? Money.Zero;
        yield return MaxPrice ?? Money.Zero;
        yield return BuyNowPrice ?? Money.Zero;
        yield return MinimumBidIncrement ?? Money.Zero;
        yield return Duration;
        yield return AllowAutoBidding;
    }
}