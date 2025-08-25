using Marketplace.Domain.Common;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Domain.Listings.Events;

namespace Marketplace.Domain.Listings;

public class Listing : Entity<ListingId>
{
    public ItemId ItemId { get; private set; }
    public UserId SellerId { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public Location Location { get; private set; }
    public CategoryId CategoryId { get; private set; }
    public ListingType ListingType { get; private set; }
    public ListingStatus Status { get; private set; }
    public Money CurrentPrice { get; private set; }
    public AuctionSettings? AuctionSettings { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public int ViewCount { get; private set; }
    public bool IsDeleted => DeletedAt.HasValue;

    // EF Core constructor
    private Listing() : base(default!) { }

    private Listing(
        ListingId id,
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId,
        ListingType listingType,
        Money currentPrice,
        AuctionSettings? auctionSettings = null,
        DateTime? expiresAt = null) : base(id)
    {
        ValidateTitle(title);
        ValidateDescription(description);

        ItemId = itemId;
        SellerId = sellerId;
        Title = title;
        Description = description;
        Location = location;
        CategoryId = categoryId;
        ListingType = listingType;
        Status = ListingStatus.Active;
        CurrentPrice = currentPrice;
        AuctionSettings = auctionSettings;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
        ViewCount = 0;

        AddDomainEvent(new ListingCreatedEvent(Id, ItemId, SellerId, listingType));
    }

    public static Listing CreateFreeListing(
        ListingId listingId,
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId)
    {
        return new Listing(
            listingId,
            itemId,
            sellerId,
            title,
            description,
            location,
            categoryId,
            ListingType.Free,
            Money.Zero,
            null,
            DateTime.UtcNow.Add(ListingConstants.FreeListingDuration));
    }

    public static Listing CreateFreeToAuctionListing(
        ListingId listingId,
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId,
        TimeSpan auctionDuration)
    {
        var auctionSettings = AuctionSettings.CreateFreeToAuction(auctionDuration);

        return new Listing(
            listingId,
            itemId,
            sellerId,
            title,
            description,
            location,
            categoryId,
            ListingType.FreeToAuction,
            Money.Zero,
            auctionSettings);
    }

    public static Listing CreateForwardAuctionListing(
        ListingId listingId,
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId,
        Money startingPrice,
        Money? reservePrice,
        TimeSpan duration)
    {
        var auctionSettings = AuctionSettings.CreateForwardAuction(startingPrice, reservePrice, duration);
        var expiresAt = DateTime.UtcNow.Add(duration);

        return new Listing(
            listingId,
            itemId,
            sellerId,
            title,
            description,
            location,
            categoryId,
            ListingType.ForwardAuction,
            startingPrice,
            auctionSettings,
            expiresAt);
    }

    public static Listing CreateReverseAuctionListing(
        ListingId listingId,
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId,
        Money maxPrice,
        TimeSpan duration)
    {
        var auctionSettings = AuctionSettings.CreateReverseAuction(maxPrice, duration);
        var expiresAt = DateTime.UtcNow.Add(duration);

        return new Listing(
            listingId,
            itemId,
            sellerId,
            title,
            description,
            location,
            categoryId,
            ListingType.ReverseAuction,
            maxPrice,
            auctionSettings,
            expiresAt);
    }

    public static Listing CreateFixedPriceListing(
        ListingId listingId,
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId,
        Money price)
    {
        return new Listing(
            listingId,
            itemId,
            sellerId,
            title,
            description,
            location,
            categoryId,
            ListingType.FixedPrice,
            price);
    }

    public void ConvertToForwardAuction(Money firstBidAmount)
    {
        if (ListingType != ListingType.FreeToAuction)
            throw new InvalidOperationException("Only FreeToAuction listings can be converted to ForwardAuction");

        if (Status != ListingStatus.Active)
            throw new InvalidOperationException("Only active listings can be converted");

        ListingType = ListingType.ForwardAuction;
        CurrentPrice = firstBidAmount;

        if (AuctionSettings != null)
        {
            ExpiresAt = DateTime.UtcNow.Add(AuctionSettings.Duration);
        }

        AddDomainEvent(new ListingConvertedToAuctionEvent(Id, firstBidAmount));
    }

    public void UpdateLocation(Location newLocation)
    {
        if (Status != ListingStatus.Active)
            throw new InvalidOperationException("Cannot update location of inactive listing");

        Location = newLocation;
        AddDomainEvent(new ListingLocationUpdatedEvent(Id, newLocation));
    }

    public void MarkAsExpired()
    {
        if (Status != ListingStatus.Active)
            throw new InvalidOperationException("Only active listings can be marked as expired");

        Status = ListingStatus.Expired;
        AddDomainEvent(new ListingExpiredEvent(Id));
    }

    public void MarkAsCompleted()
    {
        if (Status != ListingStatus.Active)
            throw new InvalidOperationException("Only active listings can be marked as completed");

        Status = ListingStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        AddDomainEvent(new ListingCompletedEvent(Id, CompletedAt.Value));
    }

    public void IncrementViewCount()
    {
        ViewCount++;
    }

    public void SoftDelete()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Listing is already deleted");
        
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        Status = ListingStatus.Cancelled;
        AddDomainEvent(new ListingSoftDeletedEvent(Id, DeletedAt.Value));
    }
    
    public void Restore()
    {
        if (!IsDeleted)
            throw new InvalidOperationException("Listing is not deleted");
        
        DeletedAt = null;
        UpdatedAt = DateTime.UtcNow;
        Status = ListingStatus.Active;
        AddDomainEvent(new ListingRestoredEvent(Id));
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be null or empty", nameof(title));

        if (title.Length > ListingConstants.MaxTitleLength)
            throw new ArgumentException($"Title cannot exceed {ListingConstants.MaxTitleLength} characters", nameof(title));
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Description cannot be null or empty", nameof(description));

        if (description.Length > ListingConstants.MaxDescriptionLength)
            throw new ArgumentException($"Description cannot exceed {ListingConstants.MaxDescriptionLength} characters", nameof(description));
    }
}