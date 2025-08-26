using FluentAssertions;
using Xunit;
using Marketplace.Domain.Categories;

namespace Item.Service.Tests.Domain;

public class CategoryTests
{
    [Fact]
    public void Create_ShouldCreateValidCategory_WhenAllFieldsProvided()
    {
        // Arrange
        var categoryId = new CategoryId(Guid.NewGuid());
        var name = "Electronics";
        var description = "Electronic devices and accessories";

        // Act
        var category = Category.Create(categoryId, name, description);

        // Assert
        category.Should().NotBeNull();
        category.Id.Should().Be(categoryId);
        category.Name.Should().Be(name);
        category.Description.Should().Be(description);
        category.ParentCategoryId.Should().BeNull();
        category.IsActive.Should().BeTrue();
        category.SubCategories.Should().BeEmpty();
        category.CategoryPath.Should().Be(name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldThrowException_WhenNameIsInvalid(string? invalidName)
    {
        // Arrange
        var categoryId = new CategoryId(Guid.NewGuid());

        // Act & Assert
        var act = () => Category.Create(categoryId, invalidName!, "Description");

        act.Should().Throw<ArgumentException>()
            .WithMessage("Category name cannot be empty*");
    }

    [Fact]
    public void Create_ShouldCreateSubCategory_WhenParentProvided()
    {
        // Arrange
        var parentId = new CategoryId(Guid.NewGuid());
        var parent = Category.Create(parentId, "Electronics", "Electronics category");
        
        var childId = new CategoryId(Guid.NewGuid());

        // Act
        var child = Category.CreateSubCategory(childId, "Phones", "Mobile phones", parent);

        // Assert
        child.ParentCategoryId.Should().Be(parentId);
        child.CategoryPath.Should().Be("Electronics/Phones");
        parent.SubCategories.Should().Contain(childId);
    }

    [Fact]
    public void AddSubCategory_ShouldAddCategoryToSubCategories()
    {
        // Arrange
        var parent = CreateValidCategory();
        var childId = new CategoryId(Guid.NewGuid());

        // Act
        parent.AddSubCategory(childId);

        // Assert
        parent.SubCategories.Should().Contain(childId);
        parent.SubCategories.Should().HaveCount(1);
    }

    [Fact]
    public void AddSubCategory_ShouldThrowException_WhenAddingDuplicateSubCategory()
    {
        // Arrange
        var parent = CreateValidCategory();
        var childId = new CategoryId(Guid.NewGuid());
        parent.AddSubCategory(childId);

        // Act & Assert
        var act = () => parent.AddSubCategory(childId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Subcategory already exists*");
    }

    [Fact]
    public void RemoveSubCategory_ShouldRemoveCategoryFromSubCategories()
    {
        // Arrange
        var parent = CreateValidCategory();
        var childId = new CategoryId(Guid.NewGuid());
        parent.AddSubCategory(childId);

        // Act
        parent.RemoveSubCategory(childId);

        // Assert
        parent.SubCategories.Should().BeEmpty();
    }

    [Fact]
    public void UpdatePath_ShouldUpdateCategoryPath_WhenParentPathChanges()
    {
        // Arrange
        var grandparent = Category.Create(new CategoryId(Guid.NewGuid()), "Home", "Home category");
        var parent = Category.CreateSubCategory(
            new CategoryId(Guid.NewGuid()), 
            "Electronics", 
            "Electronics category", 
            grandparent
        );
        var child = Category.CreateSubCategory(
            new CategoryId(Guid.NewGuid()),
            "Phones",
            "Mobile phones",
            parent
        );

        // Initial paths
        child.CategoryPath.Should().Be("Home/Electronics/Phones");

        // Act - Change grandparent name
        grandparent.UpdateDetails("House", "House category");
        parent.UpdatePath("House");
        child.UpdatePath("House/Electronics");

        // Assert
        child.CategoryPath.Should().Be("House/Electronics/Phones");
    }

    [Fact]
    public void HasCircularReference_ShouldReturnTrue_WhenCircularReferenceDetected()
    {
        // Arrange
        var categoryId1 = new CategoryId(Guid.NewGuid());
        var categoryId2 = new CategoryId(Guid.NewGuid());
        var categoryId3 = new CategoryId(Guid.NewGuid());
        
        var category1 = Category.Create(categoryId1, "Cat1", "");
        var category2 = Category.CreateSubCategory(categoryId2, "Cat2", "", category1);
        var category3 = Category.CreateSubCategory(categoryId3, "Cat3", "", category2);

        // Act - Try to make category3 the parent of category1 (circular)
        var hasCircular = category1.HasCircularReference(categoryId3);

        // Assert
        hasCircular.Should().BeTrue();
    }

    [Fact]
    public void Deactivate_ShouldSetIsActiveToFalse()
    {
        // Arrange
        var category = CreateValidCategory();

        // Act
        category.Deactivate();

        // Assert
        category.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Activate_ShouldSetIsActiveToTrue()
    {
        // Arrange
        var category = CreateValidCategory();
        category.Deactivate();

        // Act
        category.Activate();

        // Assert
        category.IsActive.Should().BeTrue();
    }

    [Fact]
    public void UpdateDetails_ShouldUpdateNameAndDescription()
    {
        // Arrange
        var category = CreateValidCategory();
        var newName = "Updated Electronics";
        var newDescription = "Updated description";

        // Act
        category.UpdateDetails(newName, newDescription);

        // Assert
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
    }

    [Fact]
    public void GetFullPath_ShouldReturnCompleteCategoryPath()
    {
        // Arrange
        var root = Category.Create(new CategoryId(Guid.NewGuid()), "Electronics", "");
        var level1 = Category.CreateSubCategory(new CategoryId(Guid.NewGuid()), "Computers", "", root);
        var level2 = Category.CreateSubCategory(new CategoryId(Guid.NewGuid()), "Laptops", "", level1);

        // Act
        var path = level2.GetFullPath();

        // Assert
        path.Should().Be("Electronics/Computers/Laptops");
    }

    private static Category CreateValidCategory()
    {
        return Category.Create(
            new CategoryId(Guid.NewGuid()),
            "Test Category",
            "Test Description"
        );
    }
}