using FluentAssertions;
using Xunit;
using NSubstitute;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Marketplace.Application.Items.Commands;
using Marketplace.Application.Items.Handlers;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Application.Common;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using DomainItem = Marketplace.Domain.Items.Item;

namespace Item.Application.Tests.Handlers;

public class AddItemImageCommandHandlerTests
{
    private readonly IItemRepository _itemRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AddItemImageCommandHandler> _logger;
    private readonly AddItemImageCommandHandler _handler;

    public AddItemImageCommandHandlerTests()
    {
        _itemRepository = Substitute.For<IItemRepository>();
        _fileStorageService = Substitute.For<IFileStorageService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _logger = Substitute.For<ILogger<AddItemImageCommandHandler>>();

        _handler = new AddItemImageCommandHandler(
            _itemRepository,
            _fileStorageService,
            _unitOfWork,
            _logger);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldAddImageToItem()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        var formFile = CreateMockImageFile("test.jpg", 1024);
        var imageUrl = "https://storage.example.com/items/test.jpg";

        var item = DomainItem.Create(
            itemId,
            "Test Item",
            "Description",
            new CategoryId(Guid.NewGuid()),
            userId,
            ItemCondition.Good);

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(item);

        _fileStorageService.UploadImageAsync(formFile, "items", Arg.Any<CancellationToken>())
            .Returns(Result.Success(imageUrl));

        var command = new AddItemImageCommand(itemId, userId, formFile, false, "Test image");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Url.Should().Be(imageUrl);
        result.Value.AltText.Should().Be("Test image");

        item.Images.Should().HaveCount(1);
        item.Images.First().Url.Should().Be(imageUrl);

        await _itemRepository.Received(1).UpdateAsync(item, Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenItemNotFound_ShouldFail()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        var formFile = CreateMockImageFile("test.jpg", 1024);

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns((DomainItem?)null);

        var command = new AddItemImageCommand(itemId, userId, formFile, false, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Item not found");

        await _itemRepository.DidNotReceive().UpdateAsync(Arg.Any<DomainItem>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUserNotOwner_ShouldFail()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var ownerId = new UserId(Guid.NewGuid());
        var differentUserId = new UserId(Guid.NewGuid());
        var formFile = CreateMockImageFile("test.jpg", 1024);

        var item = DomainItem.Create(
            itemId,
            "Test Item",
            "Description",
            new CategoryId(Guid.NewGuid()),
            ownerId, // Different owner
            ItemCondition.Good);

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(item);

        var command = new AddItemImageCommand(itemId, differentUserId, formFile, false, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("not authorized");

        await _itemRepository.DidNotReceive().UpdateAsync(Arg.Any<DomainItem>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenFileUploadFails_ShouldFail()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        var formFile = CreateMockImageFile("test.jpg", 1024);

        var item = DomainItem.Create(
            itemId,
            "Test Item",
            "Description",
            new CategoryId(Guid.NewGuid()),
            userId,
            ItemCondition.Good);

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(item);

        _fileStorageService.UploadImageAsync(formFile, "items", Arg.Any<CancellationToken>())
            .Returns(Result.Failure<string>("Upload failed"));

        var command = new AddItemImageCommand(itemId, userId, formFile, false, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Upload failed");

        await _itemRepository.DidNotReceive().UpdateAsync(Arg.Any<DomainItem>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithPrimaryImage_ShouldSetAsPrimary()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var userId = new UserId(Guid.NewGuid());
        var formFile = CreateMockImageFile("test.jpg", 1024);
        var imageUrl = "https://storage.example.com/items/test.jpg";

        var item = DomainItem.Create(
            itemId,
            "Test Item",
            "Description",
            new CategoryId(Guid.NewGuid()),
            userId,
            ItemCondition.Good);

        _itemRepository.GetByIdAsync(itemId, Arg.Any<CancellationToken>())
            .Returns(item);

        _fileStorageService.UploadImageAsync(formFile, "items", Arg.Any<CancellationToken>())
            .Returns(Result.Success(imageUrl));

        var command = new AddItemImageCommand(itemId, userId, formFile, true, "Primary image");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.IsPrimary.Should().BeTrue();
        item.Images.First().IsPrimary.Should().BeTrue();
    }

    private static IFormFile CreateMockImageFile(string fileName, long sizeInBytes)
    {
        var formFile = Substitute.For<IFormFile>();
        formFile.FileName.Returns(fileName);
        formFile.Length.Returns(sizeInBytes);
        formFile.ContentType.Returns("image/jpeg");
        
        var stream = new MemoryStream(new byte[sizeInBytes]);
        formFile.OpenReadStream().Returns(stream);
        
        return formFile;
    }
}