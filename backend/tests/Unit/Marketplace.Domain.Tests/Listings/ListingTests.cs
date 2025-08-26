using FluentAssertions;
using Xunit;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Domain.Tests.Listings;

public class ListingTests
{
    [Fact]
    public void Create_WithValidData_ShouldCreateFreeListing()
    {
        // Arrange
        var listingId = new ListingId(Guid.NewGuid());
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());
        var title = "Free Couch - Good Condition";
        var description = "Giving away a comfortable couch";
        var location = new Location(40.7128, -74.0060); // New York coordinates
        var categoryId = new CategoryId(Guid.NewGuid());

        // Act
        var listing = Listing.CreateFreeListing(
            listingId, 
            itemId, 
            sellerId, 
            title, 
            description, 
            location, 
            categoryId);

        // Assert
        listing.Should().NotBeNull();
        listing.Id.Should().Be(listingId);
        listing.ItemId.Should().Be(itemId);
        listing.SellerId.Should().Be(sellerId);
        listing.Title.Should().Be(title);
        listing.Description.Should().Be(description);
        listing.Location.Should().Be(location);
        listing.CategoryId.Should().Be(categoryId);
        listing.ListingType.Should().Be(ListingType.Free);
        listing.Status.Should().Be(ListingStatus.Active);
        listing.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(30), TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateFreeToAuctionListing()
    {
        // Arrange
        var listingId = new ListingId(Guid.NewGuid());
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());
        var location = new Location(40.7128, -74.0060);
        var categoryId = new CategoryId(Guid.NewGuid());
        var auctionDuration = TimeSpan.FromDays(7);

        // Act
        var listing = Listing.CreateFreeToAuctionListing(
            listingId, 
            itemId, 
            sellerId, 
            "Free to Auction Item", 
            "Free initially, becomes auction on first bid", 
            location, 
            categoryId,
            auctionDuration);

        // Assert
        listing.Should().NotBeNull();
        listing.ListingType.Should().Be(ListingType.FreeToAuction);
        listing.Status.Should().Be(ListingStatus.Active);
        listing.AuctionSettings.Should().NotBeNull();
        listing.AuctionSettings!.Duration.Should().Be(auctionDuration);
        listing.CurrentPrice.Should().Be(Money.Zero);
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateForwardAuctionListing()
    {
        // Arrange
        var listingId = new ListingId(Guid.NewGuid());
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());
        var location = new Location(40.7128, -74.0060);
        var categoryId = new CategoryId(Guid.NewGuid());
        var startingPrice = new Money(100m, "USD");
        var reservePrice = new Money(150m, "USD");
        var duration = TimeSpan.FromDays(7);

        // Act
        var listing = Listing.CreateForwardAuctionListing(
            listingId, 
            itemId, 
            sellerId, 
            "Auction Item", 
            "Item for auction", 
            location, 
            categoryId,
            startingPrice,
            reservePrice,
            duration);

        // Assert
        listing.Should().NotBeNull();
        listing.ListingType.Should().Be(ListingType.ForwardAuction);
        listing.Status.Should().Be(ListingStatus.Active);
        listing.CurrentPrice.Should().Be(startingPrice);
        listing.AuctionSettings.Should().NotBeNull();
        listing.AuctionSettings!.StartingPrice.Should().Be(startingPrice);
        listing.AuctionSettings.ReservePrice.Should().Be(reservePrice);
        listing.AuctionSettings.Duration.Should().Be(duration);
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateReverseAuctionListing()
    {
        // Arrange
        var listingId = new ListingId(Guid.NewGuid());
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());
        var location = new Location(40.7128, -74.0060);
        var categoryId = new CategoryId(Guid.NewGuid());
        var maxPrice = new Money(200m, "USD");
        var duration = TimeSpan.FromDays(5);

        // Act
        var listing = Listing.CreateReverseAuctionListing(
            listingId, 
            itemId, 
            sellerId, 
            "Reverse Auction Item", 
            "Lowest bidder wins", 
            location, 
            categoryId,
            maxPrice,
            duration);

        // Assert
        listing.Should().NotBeNull();
        listing.ListingType.Should().Be(ListingType.ReverseAuction);
        listing.Status.Should().Be(ListingStatus.Active);
        listing.CurrentPrice.Should().Be(maxPrice);
        listing.AuctionSettings.Should().NotBeNull();
        listing.AuctionSettings!.MaxPrice.Should().Be(maxPrice);
        listing.AuctionSettings.Duration.Should().Be(duration);
    }

    [Fact]
    public void Create_WithValidData_ShouldCreateFixedPriceListing()
    {
        // Arrange
        var listingId = new ListingId(Guid.NewGuid());
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());
        var location = new Location(40.7128, -74.0060);
        var categoryId = new CategoryId(Guid.NewGuid());
        var price = new Money(50m, "USD");

        // Act
        var listing = Listing.CreateFixedPriceListing(
            listingId, 
            itemId, 
            sellerId, 
            "For Sale Item", 
            "Fixed price item", 
            location, 
            categoryId,
            price);

        // Assert
        listing.Should().NotBeNull();
        listing.ListingType.Should().Be(ListingType.FixedPrice);
        listing.Status.Should().Be(ListingStatus.Active);
        listing.CurrentPrice.Should().Be(price);
        listing.AuctionSettings.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithInvalidTitle_ShouldThrowArgumentException(string invalidTitle)
    {
        // Arrange
        var listingId = new ListingId(Guid.NewGuid());
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());
        var location = new Location(40.7128, -74.0060);
        var categoryId = new CategoryId(Guid.NewGuid());

        // Act & Assert
        var action = () => Listing.CreateFreeListing(
            listingId, 
            itemId, 
            sellerId, 
            invalidTitle, 
            "Description", 
            location, 
            categoryId);

        action.Should().Throw<ArgumentException>()
            .WithMessage("*Title*");
    }

    [Fact]
    public void ConvertToAuction_FromFreeToAuction_ShouldUpdateListingType()
    {
        // Arrange
        var listing = CreateSampleFreeToAuctionListing();
        var firstBidAmount = new Money(10m, "USD");

        // Act
        listing.ConvertToForwardAuction(firstBidAmount);

        // Assert
        listing.ListingType.Should().Be(ListingType.ForwardAuction);
        listing.CurrentPrice.Should().Be(firstBidAmount);
        listing.Status.Should().Be(ListingStatus.Active);
    }

    [Fact]
    public void UpdateLocation_WithValidCoordinates_ShouldUpdateLocation()
    {
        // Arrange
        var listing = CreateSampleFreeListing();
        var newLocation = new Location(34.0522, -118.2437); // Los Angeles coordinates

        // Act
        listing.UpdateLocation(newLocation);

        // Assert
        listing.Location.Should().Be(newLocation);
    }

    [Fact]
    public void MarkAsExpired_WhenActive_ShouldUpdateStatus()
    {
        // Arrange
        var listing = CreateSampleFreeListing();

        // Act
        listing.MarkAsExpired();

        // Assert
        listing.Status.Should().Be(ListingStatus.Expired);
    }

    private static Listing CreateSampleFreeListing()
    {
        return Listing.CreateFreeListing(
            new ListingId(Guid.NewGuid()),
            new ItemId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Sample Free Item",
            "Sample description",
            new Location(40.7128, -74.0060),
            new CategoryId(Guid.NewGuid()));
    }

    private static Listing CreateSampleFreeToAuctionListing()
    {
        return Listing.CreateFreeToAuctionListing(
            new ListingId(Guid.NewGuid()),
            new ItemId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Sample Free to Auction Item",
            "Sample description",
            new Location(40.7128, -74.0060),
            new CategoryId(Guid.NewGuid()),
            TimeSpan.FromDays(7));
    }
}