using FluentAssertions;
using Xunit;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Item.Service.Tests.Domain;

public class ItemTests
{
    [Fact]
    public void Create_ShouldCreateValidItem_WhenAllRequiredFieldsProvided()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var title = "iPhone 12 Pro";
        var description = "Excellent condition, barely used";
        var categoryId = new CategoryId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());
        var condition = ItemCondition.LikeNew;

        // Act
        var item = Marketplace.Domain.Items.Item.Create(
            itemId,
            title,
            description,
            categoryId,
            sellerId,
            condition
        );

        // Assert
        item.Should().NotBeNull();
        item.Id.Should().Be(itemId);
        item.Title.Should().Be(title);
        item.Description.Should().Be(description);
        item.CategoryId.Should().Be(categoryId);
        item.SellerId.Should().Be(sellerId);
        item.Condition.Should().Be(condition);
        item.Images.Should().BeEmpty();
        item.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenTitleIsInvalid(string invalidTitle)
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var categoryId = new CategoryId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());

        // Act & Assert
        var act = () => Marketplace.Domain.Items.Item.Create(
            itemId,
            invalidTitle,
            "Description",
            categoryId,
            sellerId,
            ItemCondition.Good
        );

        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot be empty*");
    }

    [Fact]
    public void Create_ShouldThrowException_WhenTitleExceedsMaxLength()
    {
        // Arrange
        var itemId = new ItemId(Guid.NewGuid());
        var tooLongTitle = new string('a', 201); // Max length is 200
        var categoryId = new CategoryId(Guid.NewGuid());
        var sellerId = new UserId(Guid.NewGuid());

        // Act & Assert
        var act = () => Marketplace.Domain.Items.Item.Create(
            itemId,
            tooLongTitle,
            "Description",
            categoryId,
            sellerId,
            ItemCondition.Good
        );

        act.Should().Throw<ArgumentException>()
            .WithMessage("Title cannot exceed 200 characters*");
    }

    [Fact]
    public void AddImage_ShouldAddImageToCollection()
    {
        // Arrange
        var item = CreateValidItem();
        var imageId = new ImageId(Guid.NewGuid());
        var imageUrl = "https://storage.example.com/image1.jpg";
        var isPrimary = true;

        // Act
        item.AddImage(new ItemImage(imageId, imageUrl, isPrimary));

        // Assert
        item.Images.Should().HaveCount(1);
        item.Images.First().Id.Should().Be(imageId);
        item.Images.First().Url.Should().Be(imageUrl);
        item.Images.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void AddImage_ShouldThrowException_WhenAddingMoreThan10Images()
    {
        // Arrange
        var item = CreateValidItem();
        
        // Add 10 images
        for (int i = 0; i < 10; i++)
        {
            item.AddImage(new ItemImage(
                new ImageId(Guid.NewGuid()),
                $"https://storage.example.com/image{i}.jpg",
                i == 0
            ));
        }

        // Act & Assert
        var act = () => item.AddImage(new ItemImage(
            new ImageId(Guid.NewGuid()),
            "https://storage.example.com/image11.jpg",
            false
        ));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot add more than 10 images*");
    }

    [Fact]
    public void RemoveImage_ShouldRemoveImageFromCollection()
    {
        // Arrange
        var item = CreateValidItem();
        var imageId = new ImageId(Guid.NewGuid());
        var image = new ItemImage(imageId, "https://storage.example.com/image1.jpg", true);
        item.AddImage(image);

        // Act
        item.RemoveImage(imageId);

        // Assert
        item.Images.Should().BeEmpty();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateTitleAndDescription()
    {
        // Arrange
        var item = CreateValidItem();
        var newTitle = "Updated iPhone 12 Pro";
        var newDescription = "Updated description with more details";

        // Act
        item.UpdateDetails(newTitle, newDescription);

        // Assert
        item.Title.Should().Be(newTitle);
        item.Description.Should().Be(newDescription);
    }

    [Fact]
    public void ChangeCondition_ShouldUpdateItemCondition()
    {
        // Arrange
        var item = CreateValidItem();
        var newCondition = ItemCondition.Fair;

        // Act
        item.ChangeCondition(newCondition);

        // Assert
        item.Condition.Should().Be(newCondition);
    }

    [Fact]
    public void SetPrimaryImage_ShouldUpdatePrimaryImageStatus()
    {
        // Arrange
        var item = CreateValidItem();
        var imageId1 = new ImageId(Guid.NewGuid());
        var imageId2 = new ImageId(Guid.NewGuid());
        
        item.AddImage(new ItemImage(imageId1, "https://example.com/image1.jpg", true));
        item.AddImage(new ItemImage(imageId2, "https://example.com/image2.jpg", false));

        // Act
        item.SetPrimaryImage(imageId2);

        // Assert
        var primaryImage = item.Images.Single(i => i.IsPrimary);
        primaryImage.Id.Should().Be(imageId2);
        item.Images.Count(i => i.IsPrimary).Should().Be(1);
    }

    private static Marketplace.Domain.Items.Item CreateValidItem()
    {
        return Marketplace.Domain.Items.Item.Create(
            new ItemId(Guid.NewGuid()),
            "Test Item",
            "Test Description",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.Good
        );
    }
}