using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Application.Listings.DTOs;

public record ListingDto
{
    public ListingId ListingId { get; init; } = default!;
    public ItemId ItemId { get; init; } = default!;
    public UserId SellerId { get; init; } = default!;
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public Location Location { get; init; } = default!;
    public CategoryId CategoryId { get; init; } = default!;
    public ListingType ListingType { get; init; }
    public ListingStatus Status { get; init; }
    public Money CurrentPrice { get; init; } = default!;
    public AuctionSettingsDto? AuctionSettings { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int ViewCount { get; init; }
    public bool IsDeleted { get; init; }

    public static ListingDto FromDomain(Domain.Listings.Listing listing)
    {
        return new ListingDto
        {
            ListingId = listing.Id,
            ItemId = listing.ItemId,
            SellerId = listing.SellerId,
            Title = listing.Title,
            Description = listing.Description,
            Location = listing.Location,
            CategoryId = listing.CategoryId,
            ListingType = listing.ListingType,
            Status = listing.Status,
            CurrentPrice = listing.CurrentPrice,
            AuctionSettings = listing.AuctionSettings != null 
                ? AuctionSettingsDto.FromDomain(listing.AuctionSettings) 
                : null,
            CreatedAt = listing.CreatedAt,
            UpdatedAt = listing.UpdatedAt,
            ExpiresAt = listing.ExpiresAt,
            CompletedAt = listing.CompletedAt,
            ViewCount = listing.ViewCount,
            IsDeleted = listing.IsDeleted
        };
    }
}

public record AuctionSettingsDto
{
    public Money? StartingPrice { get; init; }
    public Money? ReservePrice { get; init; }
    public Money? MaxPrice { get; init; }
    public Money? BuyNowPrice { get; init; }
    public Money? MinimumBidIncrement { get; init; }
    public TimeSpan Duration { get; init; }
    public bool AllowAutoBidding { get; init; }

    public static AuctionSettingsDto FromDomain(Domain.Listings.ValueObjects.AuctionSettings settings)
    {
        return new AuctionSettingsDto
        {
            StartingPrice = settings.StartingPrice,
            ReservePrice = settings.ReservePrice,
            MaxPrice = settings.MaxPrice,
            BuyNowPrice = settings.BuyNowPrice,
            MinimumBidIncrement = settings.MinimumBidIncrement,
            Duration = settings.Duration,
            AllowAutoBidding = settings.AllowAutoBidding
        };
    }
}

public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}