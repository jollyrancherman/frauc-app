using FluentAssertions;
using Xunit;
using Marketplace.Domain.Items.ValueObjects;

namespace Item.Service.Tests.Domain.ValueObjects;

public class ItemImageTests
{
    [Fact]
    public void Constructor_ShouldCreateValidItemImage_WhenAllFieldsValid()
    {
        // Arrange
        var imageId = new ImageId(Guid.NewGuid());
        var url = "https://storage.example.com/image.jpg";
        var isPrimary = true;

        // Act
        var itemImage = new ItemImage(imageId, url, isPrimary);

        // Assert
        itemImage.Id.Should().Be(imageId);
        itemImage.Url.Should().Be(url);
        itemImage.IsPrimary.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldThrowException_WhenUrlIsInvalid(string invalidUrl)
    {
        // Arrange
        var imageId = new ImageId(Guid.NewGuid());

        // Act & Assert
        var act = () => new ItemImage(imageId, invalidUrl, false);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Image URL cannot be empty*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenUrlIsNotValid()
    {
        // Arrange
        var imageId = new ImageId(Guid.NewGuid());
        var invalidUrl = "not-a-valid-url";

        // Act & Assert
        var act = () => new ItemImage(imageId, invalidUrl, false);

        act.Should().Throw<ArgumentException>()
            .WithMessage("Invalid image URL format*");
    }

    [Fact]
    public void SetPrimary_ShouldUpdatePrimaryStatus()
    {
        // Arrange
        var itemImage = new ItemImage(
            new ImageId(Guid.NewGuid()),
            "https://storage.example.com/image.jpg",
            false
        );

        // Act
        itemImage.SetPrimary(true);

        // Assert
        itemImage.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void Equality_ShouldReturnTrue_WhenImagesHaveSameId()
    {
        // Arrange
        var imageId = new ImageId(Guid.NewGuid());
        var image1 = new ItemImage(imageId, "https://storage.example.com/1.jpg", true);
        var image2 = new ItemImage(imageId, "https://storage.example.com/2.jpg", false);

        // Act & Assert
        image1.Should().Be(image2);
        (image1 == image2).Should().BeTrue();
    }

    [Fact]
    public void Equality_ShouldReturnFalse_WhenImagesHaveDifferentIds()
    {
        // Arrange
        var image1 = new ItemImage(
            new ImageId(Guid.NewGuid()),
            "https://storage.example.com/1.jpg",
            true
        );
        var image2 = new ItemImage(
            new ImageId(Guid.NewGuid()),
            "https://storage.example.com/1.jpg",
            true
        );

        // Act & Assert
        image1.Should().NotBe(image2);
        (image1 == image2).Should().BeFalse();
    }
}