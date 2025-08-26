using FluentAssertions;
using Xunit;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Listings.Queries;
using Marketplace.Application.Listings.Handlers;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Microsoft.Extensions.Caching.Memory;

namespace Listing.Application.Tests.Queries;

public class GetListingQueryHandlerTests
{
    private readonly IListingRepository _listingRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetListingByIdQueryHandler> _logger;
    private readonly GetListingByIdQueryHandler _handler;

    public GetListingQueryHandlerTests()
    {
        _listingRepository = Substitute.For<IListingRepository>();
        _cache = Substitute.For<IMemoryCache>();
        _logger = Substitute.For<ILogger<GetListingByIdQueryHandler>>();
        
        _handler = new GetListingByIdQueryHandler(
            _listingRepository,
            _cache,
            _logger);
    }

    [Fact]
    public async Task Handle_GetListingById_WhenExists_ShouldReturnListing()
    {
        // Arrange
        var listingId = ListingId.New();
        var query = new GetListingByIdQuery(listingId);
        
        var listing = Marketplace.Domain.Listings.Listing.CreateFreeListing(
            listingId,
            new ItemId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            "Test Listing",
            "Description",
            new Location(40.7128, -74.0060),
            new CategoryId(Guid.NewGuid())
        );

        _cache.TryGetValue(Arg.Any<object>(), out Arg.Any<object>())
            .Returns(false);
        
        _listingRepository.GetByIdAsync(listingId, Arg.Any<CancellationToken>())
            .Returns(listing);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ListingId.Should().Be(listingId);
    }

    [Fact]
    public async Task Handle_GetListingById_WhenNotExists_ShouldReturnFailure()
    {
        // Arrange
        var listingId = ListingId.New();
        var query = new GetListingByIdQuery(listingId);
        
        _cache.TryGetValue(Arg.Any<object>(), out Arg.Any<object>())
            .Returns(false);
        
        _listingRepository.GetByIdAsync(listingId, Arg.Any<CancellationToken>())
            .Returns((Marketplace.Domain.Listings.Listing?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Listing not found");
    }

    [Fact]
    public async Task Handle_GetListingById_WhenCached_ShouldReturnFromCache()
    {
        // Arrange
        var listingId = ListingId.New();
        var query = new GetListingByIdQuery(listingId);
        
        var cachedListing = new ListingDto
        {
            ListingId = listingId,
            Title = "Cached Listing",
            ListingType = ListingType.Free
        };

        object cacheValue = cachedListing;
        _cache.TryGetValue($"listing_{listingId}", out Arg.Any<object>())
            .Returns(x => 
            {
                x[1] = cacheValue;
                return true;
            });

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(cachedListing);
        
        await _listingRepository.DidNotReceive().GetByIdAsync(Arg.Any<ListingId>(), Arg.Any<CancellationToken>());
    }
}

public class GetNearbyListingsQueryHandlerTests
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<GetNearbyListingsQueryHandler> _logger;
    private readonly GetNearbyListingsQueryHandler _handler;

    public GetNearbyListingsQueryHandlerTests()
    {
        _listingRepository = Substitute.For<IListingRepository>();
        _logger = Substitute.For<ILogger<GetNearbyListingsQueryHandler>>();
        
        _handler = new GetNearbyListingsQueryHandler(
            _listingRepository,
            _logger);
    }

    [Fact]
    public async Task Handle_GetNearbyListings_ShouldReturnListingsWithinRadius()
    {
        // Arrange
        var center = new Location(40.7128, -74.0060);
        var radiusKm = 10.0;
        var query = new GetNearbyListingsQuery(center, radiusKm);
        
        var listings = new List<Marketplace.Domain.Listings.Listing>
        {
            Marketplace.Domain.Listings.Listing.CreateFreeListing(
                ListingId.New(),
                new ItemId(Guid.NewGuid()),
                new UserId(Guid.NewGuid()),
                "Nearby Item 1",
                "Description",
                new Location(40.7580, -73.9855), // Times Square
                new CategoryId(Guid.NewGuid())
            ),
            Marketplace.Domain.Listings.Listing.CreateFreeListing(
                ListingId.New(),
                new ItemId(Guid.NewGuid()),
                new UserId(Guid.NewGuid()),
                "Nearby Item 2",
                "Description",
                new Location(40.7489, -73.9680), // Grand Central
                new CategoryId(Guid.NewGuid())
            )
        };

        _listingRepository.GetNearbyListingsAsync(center, radiusKm, Arg.Any<CancellationToken>())
            .Returns(listings);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().HaveCount(2);
        result.Value!.All(l => l.Location.DistanceTo(center) <= radiusKm).Should().BeTrue();
    }

    [Fact]
    public async Task Handle_GetNearbyListings_WhenNoListings_ShouldReturnEmpty()
    {
        // Arrange
        var center = new Location(40.7128, -74.0060);
        var radiusKm = 1.0;
        var query = new GetNearbyListingsQuery(center, radiusKm);
        
        _listingRepository.GetNearbyListingsAsync(center, radiusKm, Arg.Any<CancellationToken>())
            .Returns(new List<Marketplace.Domain.Listings.Listing>());

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Should().BeEmpty();
    }
}