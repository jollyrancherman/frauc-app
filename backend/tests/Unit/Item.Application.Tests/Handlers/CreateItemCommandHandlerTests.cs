using FluentAssertions;
using Xunit;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Items.Commands;
using Marketplace.Application.Items.Handlers;
using Marketplace.Application.Common;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Item.Application.Tests.Handlers;

public class CreateItemCommandHandlerTests
{
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDomainEventDispatcher _domainEventDispatcher;
    private readonly ILogger<CreateItemCommandHandler> _logger;
    private readonly CreateItemCommandHandler _handler;

    public CreateItemCommandHandlerTests()
    {
        _itemRepository = Substitute.For<IItemRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _domainEventDispatcher = Substitute.For<IDomainEventDispatcher>();
        _logger = Substitute.For<ILogger<CreateItemCommandHandler>>();
        
        _handler = new CreateItemCommandHandler(
            _itemRepository,
            _unitOfWork,
            _domainEventDispatcher,
            _logger);
    }

    [Fact]
    public async Task Handle_ShouldCreateAndSaveItem_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateItemCommand(
            new ItemId(Guid.NewGuid()),
            "iPhone 12 Pro",
            "Excellent condition",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.LikeNew
        );

        _itemRepository.GetByIdAsync(command.ItemId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Marketplace.Domain.Items.Item?>(null));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.ItemId.Should().Be(command.ItemId);
        result.Value.Title.Should().Be(command.Title);
        result.Value.Description.Should().Be(command.Description);
        result.Value.CategoryId.Should().Be(command.CategoryId);
        result.Value.SellerId.Should().Be(command.SellerId);
        result.Value.Condition.Should().Be(command.Condition);

        await _itemRepository.Received(1).AddAsync(Arg.Any<Marketplace.Domain.Items.Item>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).CommitTransactionAsync(Arg.Any<CancellationToken>());
        await _domainEventDispatcher.Received(1).DispatchDomainEventsAsync<Marketplace.Domain.Items.Item, ItemId>(Arg.Any<Marketplace.Domain.Items.Item>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenItemAlreadyExists()
    {
        // Arrange
        var command = new CreateItemCommand(
            new ItemId(Guid.NewGuid()),
            "iPhone 12 Pro",
            "Excellent condition",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.LikeNew
        );

        var existingItem = Marketplace.Domain.Items.Item.Create(
            command.ItemId,
            "Existing Item",
            "Description",
            command.CategoryId,
            command.SellerId,
            ItemCondition.Good
        );

        _itemRepository.GetByIdAsync(command.ItemId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Marketplace.Domain.Items.Item?>(existingItem));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("already exists");

        await _itemRepository.DidNotReceive().AddAsync(Arg.Any<Marketplace.Domain.Items.Item>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRollbackTransaction_WhenExceptionOccurs()
    {
        // Arrange
        var command = new CreateItemCommand(
            new ItemId(Guid.NewGuid()),
            "iPhone 12 Pro",
            "Excellent condition",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.LikeNew
        );

        _itemRepository.GetByIdAsync(command.ItemId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Marketplace.Domain.Items.Item?>(null));

        _itemRepository.AddAsync(Arg.Any<Marketplace.Domain.Items.Item>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("Database error")));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Failed to create item");

        await _unitOfWork.Received(1).BeginTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).RollbackTransactionAsync(Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().CommitTransactionAsync(Arg.Any<CancellationToken>());
    }
}