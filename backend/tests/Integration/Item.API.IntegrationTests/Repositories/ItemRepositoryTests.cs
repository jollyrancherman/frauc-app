using FluentAssertions;
using Item.API.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Infrastructure.Data;
using ItemEntity = Marketplace.Domain.Items.Item;

namespace Item.API.IntegrationTests.Repositories;

public class ItemRepositoryTests : IntegrationTestBase
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnItem_WhenItemExists()
    {
        // Arrange
        await ClearDatabase();
        
        var item = await SeedItemInDatabaseAsync();

        // Act
        var result = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.GetByIdAsync(item.Id);
        });

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
        result.Title.Should().Be(item.Title);
        result.Description.Should().Be(item.Description);
        result.SellerId.Should().Be(item.SellerId);
        result.CategoryId.Should().Be(item.CategoryId);
        result.Condition.Should().Be(item.Condition);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        var nonExistentId = new ItemId(Guid.NewGuid());

        // Act
        var result = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.GetByIdAsync(nonExistentId);
        });

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_ShouldPersistItem_WhenItemIsValid()
    {
        // Arrange
        await ClearDatabase();
        
        var item = ItemEntity.Create(
            new ItemId(Guid.NewGuid()),
            "Test Item",
            "Test Description",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.LikeNew
        );

        // Act
        await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            
            await repository.AddAsync(item);
            await context.SaveChangesAsync();
        });

        // Assert - Verify item was persisted
        var savedItem = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.GetByIdAsync(item.Id);
        });

        savedItem.Should().NotBeNull();
        savedItem!.Id.Should().Be(item.Id);
        savedItem.Title.Should().Be(item.Title);
    }

    [Fact]
    public async Task GetBySellerIdAsync_ShouldReturnSellerItems_WhenSellerHasItems()
    {
        // Arrange
        await ClearDatabase();
        
        var sellerId = new UserId(Guid.NewGuid());
        var otherSellerId = new UserId(Guid.NewGuid());
        
        var sellerItems = new[]
        {
            await SeedItemInDatabaseAsync(sellerId: sellerId, title: "Seller Item 1"),
            await SeedItemInDatabaseAsync(sellerId: sellerId, title: "Seller Item 2")
        };
        
        // Item from different seller - should not be returned
        await SeedItemInDatabaseAsync(sellerId: otherSellerId, title: "Other Seller Item");

        // Act
        var result = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.GetBySellerIdAsync(sellerId);
        });

        // Assert
        var items = result.ToList();
        items.Should().HaveCount(2);
        items.Should().AllSatisfy(item => item.SellerId.Should().Be(sellerId));
        items.Should().Contain(item => item.Title == "Seller Item 1");
        items.Should().Contain(item => item.Title == "Seller Item 2");
    }

    [Fact]
    public async Task GetByCategoryIdAsync_ShouldReturnCategoryItems_WhenCategoryHasItems()
    {
        // Arrange
        await ClearDatabase();
        
        var categoryId = new CategoryId(Guid.NewGuid());
        var otherCategoryId = new CategoryId(Guid.NewGuid());
        
        var categoryItems = new[]
        {
            await SeedItemInDatabaseAsync(categoryId: categoryId, title: "Category Item 1"),
            await SeedItemInDatabaseAsync(categoryId: categoryId, title: "Category Item 2")
        };
        
        // Item from different category - should not be returned
        await SeedItemInDatabaseAsync(categoryId: otherCategoryId, title: "Other Category Item");

        // Act
        var result = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.GetByCategoryIdAsync(categoryId);
        });

        // Assert
        var items = result.ToList();
        items.Should().HaveCount(2);
        items.Should().AllSatisfy(item => item.CategoryId.Should().Be(categoryId));
        items.Should().Contain(item => item.Title == "Category Item 1");
        items.Should().Contain(item => item.Title == "Category Item 2");
    }

    [Fact]
    public async Task GetPagedBySellerAsync_ShouldReturnCorrectPage_WhenSellerHasMultipleItems()
    {
        // Arrange
        await ClearDatabase();
        
        var sellerId = new UserId(Guid.NewGuid());
        
        // Create 5 items for pagination testing
        for (int i = 1; i <= 5; i++)
        {
            await SeedItemInDatabaseAsync(sellerId: sellerId, title: $"Item {i}");
        }

        // Act - Get page 2 with page size 2
        var (items, totalCount) = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.GetPagedBySellerAsync(sellerId, pageNumber: 2, pageSize: 2);
        });

        // Assert
        var itemList = items.ToList();
        itemList.Should().HaveCount(2);
        totalCount.Should().Be(5);
        itemList.Should().AllSatisfy(item => item.SellerId.Should().Be(sellerId));
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenItemExists()
    {
        // Arrange
        await ClearDatabase();
        
        var item = await SeedItemInDatabaseAsync();

        // Act
        var exists = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.ExistsAsync(item.Id);
        });

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        // Arrange
        var nonExistentId = new ItemId(Guid.NewGuid());

        // Act
        var exists = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.ExistsAsync(nonExistentId);
        });

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task CountBySellerAsync_ShouldReturnCorrectCount_WhenSellerHasItems()
    {
        // Arrange
        await ClearDatabase();
        
        var sellerId = new UserId(Guid.NewGuid());
        
        // Create 3 items for the seller
        for (int i = 1; i <= 3; i++)
        {
            await SeedItemInDatabaseAsync(sellerId: sellerId);
        }
        
        // Create items for different seller
        await SeedItemInDatabaseAsync(sellerId: new UserId(Guid.NewGuid()));

        // Act
        var count = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.CountBySellerAsync(sellerId);
        });

        // Assert
        count.Should().Be(3);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateItem_WhenItemIsModified()
    {
        // Arrange
        await ClearDatabase();
        
        var item = await SeedItemInDatabaseAsync(title: "Original Title");

        // Act
        await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            
            var itemToUpdate = await repository.GetByIdAsync(item.Id);
            itemToUpdate!.UpdateDetails("Updated Title", "Updated Description");
            
            await repository.UpdateAsync(itemToUpdate);
            await context.SaveChangesAsync();
        });

        // Assert
        var updatedItem = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.GetByIdAsync(item.Id);
        });

        updatedItem.Should().NotBeNull();
        updatedItem!.Title.Should().Be("Updated Title");
        updatedItem.Description.Should().Be("Updated Description");
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveItem_WhenItemExists()
    {
        // Arrange
        await ClearDatabase();
        
        var item = await SeedItemInDatabaseAsync();

        // Act
        await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            
            await repository.DeleteAsync(item.Id);
            await context.SaveChangesAsync();
        });

        // Assert
        var deletedItem = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var repository = serviceProvider.GetRequiredService<IItemRepository>();
            return await repository.GetByIdAsync(item.Id);
        });

        deletedItem.Should().BeNull();
    }

    private async Task<ItemEntity> SeedItemInDatabaseAsync(
        ItemId? itemId = null,
        UserId? sellerId = null,
        CategoryId? categoryId = null,
        string title = "Test Item",
        string description = "Test Description",
        ItemCondition condition = ItemCondition.Good)
    {
        return await ExecuteInTransactionAsync(async context =>
        {
            var item = ItemEntity.Create(
                itemId ?? new ItemId(Guid.NewGuid()),
                title,
                description,
                categoryId ?? new CategoryId(Guid.NewGuid()),
                sellerId ?? new UserId(Guid.NewGuid()),
                condition
            );

            await context.Items.AddAsync(item);
            await context.SaveChangesAsync();
            
            return item;
        });
    }
}