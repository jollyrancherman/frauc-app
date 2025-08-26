using FluentAssertions;
using Xunit;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Listings.Services;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Items;
using Marketplace.Domain.Common;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Listing.Application.Tests.Services;

public class ListingDomainServiceTests
{
    private readonly IListingRepository _listingRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ListingDomainService> _logger;
    private readonly ListingDomainService _service;

    public ListingDomainServiceTests()
    {
        _listingRepository = Substitute.For<IListingRepository>();
        _itemRepository = Substitute.For<IItemRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<ListingDomainService>>();

        _service = new ListingDomainService(
            _listingRepository,
            _itemRepository,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task CreateFreeListing_WhenItemExists_ShouldSucceed()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());
        var categoryId = new CategoryId(Guid.NewGuid());
        var location = new Location(40.7128, -74.0060);

        var item = Item.Create(
            itemId,
            "Test Item",
            "Description",
            categoryId,
            sellerId,
            ItemCondition.Good,
            new List<ItemImageDto>());

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(item);

        _listingRepository.GetByItemIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns((Marketplace.Domain.Listings.Listing?)null);

        // Act
        var result = await _service.CreateFreeListingAsync(
            itemId,
            sellerId,
            "Test Listing",
            "Test Description",
            location,
            categoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        await _listingRepository.Received(1).AddAsync(Arg.Any<Marketplace.Domain.Listings.Listing>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFreeListing_WhenItemNotFound_ShouldFail()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns((Item?)null);

        // Act
        var result = await _service.CreateFreeListingAsync(
            itemId,
            sellerId,
            "Test",
            "Description",
            new Location(0, 0),
            new CategoryId(Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Item not found");

        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _listingRepository.DidNotReceive().AddAsync(Arg.Any<Marketplace.Domain.Listings.Listing>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFreeListing_WhenUserNotOwner_ShouldFail()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var actualOwnerId = new UserId(Guid.NewGuid());
        var attemptingUserId = new UserId(Guid.NewGuid());

        var item = Item.Create(
            itemId,
            "Test Item",
            "Description",
            new CategoryId(Guid.NewGuid()),
            actualOwnerId, // Different owner
            ItemCondition.Good,
            new List<ItemImageDto>());

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(item);

        // Act
        var result = await _service.CreateFreeListingAsync(
            itemId,
            attemptingUserId, // Different user attempting to list
            "Test",
            "Description",
            new Location(0, 0),
            new CategoryId(Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not authorized");
        
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateFreeListing_WhenListingAlreadyExists_ShouldFail()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());

        var item = Item.Create(
            itemId,
            "Test Item",
            "Description",
            new CategoryId(Guid.NewGuid()),
            sellerId,
            ItemCondition.Good,
            new List<ItemImageDto>());

        var existingListing = Marketplace.Domain.Listings.Listing.CreateFreeListing(
            ListingId.New(),
            itemId,
            sellerId,
            "Existing",
            "Description",
            new Location(0, 0),
            new CategoryId(Guid.NewGuid()));

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(item);

        _listingRepository.GetByItemIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(existingListing);

        // Act
        var result = await _service.CreateFreeListingAsync(
            itemId,
            sellerId,
            "Test",
            "Description",
            new Location(0, 0),
            new CategoryId(Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already has an active listing");
    }

    [Fact]
    public async Task CreateForwardAuctionListing_WithValidData_ShouldSucceed()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());
        var startingPrice = Money.FromCents(10000, "USD");
        var reservePrice = Money.FromCents(20000, "USD");

        var item = Item.Create(
            itemId,
            "Auction Item",
            "Description",
            new CategoryId(Guid.NewGuid()),
            sellerId,
            ItemCondition.Good,
            new List<ItemImageDto>());

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(item);

        _listingRepository.GetByItemIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns((Marketplace.Domain.Listings.Listing?)null);

        // Act
        var result = await _service.CreateForwardAuctionListingAsync(
            itemId,
            sellerId,
            "Auction Title",
            "Auction Description",
            new Location(40.7128, -74.0060),
            new CategoryId(Guid.NewGuid()),
            startingPrice,
            reservePrice,
            TimeSpan.FromDays(7));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ListingType.Should().Be(ListingType.ForwardAuction);
        result.Value.AuctionSettings.Should().NotBeNull();
        result.Value.AuctionSettings!.StartingPrice.Should().Be(startingPrice);
        result.Value.AuctionSettings.ReservePrice.Should().Be(reservePrice);
    }

    [Fact]
    public async Task CreateListingInternal_WhenExceptionThrown_ShouldRollbackTransaction()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Item?>(new Exception("Database error")));

        // Act
        var result = await _service.CreateFreeListingAsync(
            itemId,
            sellerId,
            "Test",
            "Description",
            new Location(0, 0),
            new CategoryId(Guid.NewGuid()));

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to create listing");

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }
}