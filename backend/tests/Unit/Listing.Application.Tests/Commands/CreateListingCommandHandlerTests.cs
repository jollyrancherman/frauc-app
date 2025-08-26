using FluentAssertions;
using Xunit;
using NSubstitute;
using MediatR;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Listings.Commands;
using Marketplace.Application.Listings.Handlers;
using Marketplace.Application.Common;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Domain.Items;

namespace Listing.Application.Tests.Commands;

public class CreateListingCommandHandlerTests
{
    private readonly IListingRepository _listingRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateListingCommandHandler> _logger;
    private readonly CreateListingCommandHandler _handler;

    public CreateListingCommandHandlerTests()
    {
        _listingRepository = Substitute.For<IListingRepository>();
        _itemRepository = Substitute.For<IItemRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<CreateListingCommandHandler>>();
        
        _handler = new CreateListingCommandHandler(
            _listingRepository,
            _itemRepository,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_CreateFreeListing_ShouldSucceed()
    {
        // Arrange
        var command = new CreateFreeListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "Free Couch",
            Description: "Comfortable couch in good condition",
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid())
        );

        var item = Substitute.For<Marketplace.Domain.Items.Item>();
        _itemRepository.GetByIdAsync(command.ItemId, Arg.Any<CancellationToken>())
            .Returns(item);
        
        _listingRepository.ExistsAsync(Arg.Any<ListingId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ListingType.Should().Be(ListingType.Free);
        
        await _listingRepository.Received(1).AddAsync(Arg.Any<Marketplace.Domain.Listings.Listing>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _domainEventDispatcher.Received(1).DispatchDomainEventsAsync<Marketplace.Domain.Listings.Listing, ListingId>(
            Arg.Any<Marketplace.Domain.Listings.Listing>(), 
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CreateForwardAuctionListing_ShouldSucceed()
    {
        // Arrange
        var command = new CreateForwardAuctionListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "Vintage Watch",
            Description: "Rare vintage watch in excellent condition",
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid()),
            StartingPrice: new Money(100m, "USD"),
            ReservePrice: new Money(200m, "USD"),
            BuyNowPrice: new Money(300m, "USD"),
            Duration: TimeSpan.FromDays(7)
        );

        var item = Substitute.For<Marketplace.Domain.Items.Item>();
        _itemRepository.GetByIdAsync(command.ItemId, Arg.Any<CancellationToken>())
            .Returns(item);
        
        _listingRepository.ExistsAsync(Arg.Any<ListingId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ListingType.Should().Be(ListingType.ForwardAuction);
        result.Value.CurrentPrice.Should().Be(command.StartingPrice);
        
        await _listingRepository.Received(1).AddAsync(Arg.Any<Marketplace.Domain.Listings.Listing>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_CreateReverseAuctionListing_ShouldSucceed()
    {
        // Arrange
        var command = new CreateReverseAuctionListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "Looking for Moving Service",
            Description: "Need help moving furniture",
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid()),
            MaxPrice: new Money(500m, "USD"),
            Duration: TimeSpan.FromDays(3)
        );

        var item = Substitute.For<Marketplace.Domain.Items.Item>();
        _itemRepository.GetByIdAsync(command.ItemId, Arg.Any<CancellationToken>())
            .Returns(item);
        
        _listingRepository.ExistsAsync(Arg.Any<ListingId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ListingType.Should().Be(ListingType.ReverseAuction);
        result.Value.CurrentPrice.Should().Be(command.MaxPrice);
    }

    [Fact]
    public async Task Handle_CreateFixedPriceListing_ShouldSucceed()
    {
        // Arrange
        var command = new CreateFixedPriceListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "iPhone 15 Pro",
            Description: "Brand new iPhone",
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid()),
            Price: new Money(999m, "USD")
        );

        var item = Substitute.For<Marketplace.Domain.Items.Item>();
        _itemRepository.GetByIdAsync(command.ItemId, Arg.Any<CancellationToken>())
            .Returns(item);
        
        _listingRepository.ExistsAsync(Arg.Any<ListingId>(), Arg.Any<CancellationToken>())
            .Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ListingType.Should().Be(ListingType.FixedPrice);
        result.Value.CurrentPrice.Should().Be(command.Price);
    }

    [Fact]
    public async Task Handle_WhenItemDoesNotExist_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateFreeListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "Free Item",
            Description: "Description",
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid())
        );

        _itemRepository.GetByIdAsync(command.ItemId, Arg.Any<CancellationToken>())
            .Returns((Marketplace.Domain.Items.Item?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Item not found");
        
        await _listingRepository.DidNotReceive().AddAsync(Arg.Any<Marketplace.Domain.Listings.Listing>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransactionFails_ShouldRollback()
    {
        // Arrange
        var command = new CreateFreeListingCommand(
            ItemId: new ItemId(Guid.NewGuid()),
            SellerId: new UserId(Guid.NewGuid()),
            Title: "Free Item",
            Description: "Description",
            Location: new Location(40.7128, -74.0060),
            CategoryId: new CategoryId(Guid.NewGuid())
        );

        var item = Substitute.For<Marketplace.Domain.Items.Item>();
        _itemRepository.GetByIdAsync(command.ItemId, Arg.Any<CancellationToken>())
            .Returns(item);
        
        _unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to create listing");
        
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
    }
}