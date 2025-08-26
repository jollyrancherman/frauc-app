using FluentAssertions;
using Xunit;
using Marketplace.Domain.Items.ValueObjects;

namespace Item.Service.Tests.Domain.ValueObjects;

public class ItemIdTests
{
    [Fact]
    public void Constructor_ShouldCreateValidItemId_WhenGuidIsValid()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var itemId = new ItemId(guid);

        // Assert
        itemId.Value.Should().Be(guid);
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenGuidIsEmpty()
    {
        // Act & Assert
        var act = () => new ItemId(Guid.Empty);

        act.Should().Throw<ArgumentException>()
            .WithMessage("ItemId cannot be empty*");
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenItemIdsHaveSameValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var itemId1 = new ItemId(guid);
        var itemId2 = new ItemId(guid);

        // Act & Assert
        itemId1.Should().Be(itemId2);
        (itemId1 == itemId2).Should().BeTrue();
        (itemId1 != itemId2).Should().BeFalse();
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenItemIdsHaveDifferentValues()
    {
        // Arrange
        var itemId1 = new ItemId(Guid.NewGuid());
        var itemId2 = new ItemId(Guid.NewGuid());

        // Act & Assert
        itemId1.Should().NotBe(itemId2);
        (itemId1 == itemId2).Should().BeFalse();
        (itemId1 != itemId2).Should().BeTrue();
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var itemId = new ItemId(guid);

        // Act
        var result = itemId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }
}