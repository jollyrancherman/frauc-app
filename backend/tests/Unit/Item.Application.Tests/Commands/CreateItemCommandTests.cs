using FluentAssertions;
using Xunit;
using NSubstitute;
using Marketplace.Application.Items.Commands;
using Marketplace.Application.Items.Validators;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Domain.Items;

namespace Item.Application.Tests.Commands;

public class CreateItemCommandTests
{
    private readonly IItemRepository _mockRepository;

    public CreateItemCommandTests()
    {
        _mockRepository = Substitute.For<IItemRepository>();
    }
    [Fact]
    public void CreateItemCommand_ShouldHaveCorrectProperties()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var title = "iPhone 12 Pro";
        var description = "Excellent condition";
        var categoryId = new CategoryId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());
        var condition = ItemCondition.LikeNew;

        // Act
        var command = new CreateItemCommand(
            itemId,
            title,
            description,
            categoryId,
            sellerId,
            condition
        );

        // Assert
        command.ItemId.Should().Be(itemId);
        command.Title.Should().Be(title);
        command.Description.Should().Be(description);
        command.CategoryId.Should().Be(categoryId);
        command.SellerId.Should().Be(sellerId);
        command.Condition.Should().Be(condition);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateItemCommand_ShouldFailValidation_WhenTitleIsInvalid(string? invalidTitle)
    {
        // Arrange
        var validator = new CreateItemCommandValidator(_mockRepository);
        var command = new CreateItemCommand(
            new ItemId(Guid.NewGuid()),
            invalidTitle!,
            "Description",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.Good
        );

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateItemCommand.Title));
    }

    [Fact]
    public async Task CreateItemCommand_ShouldFailValidation_WhenTitleTooLong()
    {
        // Arrange
        var validator = new CreateItemCommandValidator(_mockRepository);
        var command = new CreateItemCommand(
            new ItemId(Guid.NewGuid()),
            new string('a', 201), // Max length is 200
            "Description",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.Good
        );

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateItemCommand.Title));
    }

    [Fact]
    public async Task CreateItemCommand_ShouldPassValidation_WhenAllFieldsValid()
    {
        // Arrange
        var validator = new CreateItemCommandValidator(_mockRepository);
        var command = new CreateItemCommand(
            new ItemId(Guid.NewGuid()),
            "Valid Title",
            "Valid Description",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.Good
        );

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}