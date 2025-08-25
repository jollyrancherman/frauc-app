using FluentAssertions;
using Item.API.IntegrationTests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Marketplace.Application.Common;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Infrastructure.Data;
using ItemEntity = Marketplace.Domain.Items.Item;

namespace Item.API.IntegrationTests.Infrastructure;

public class UnitOfWorkTests : IntegrationTestBase
{
    [Fact]
    public async Task BeginTransaction_CommitTransaction_ShouldPersistChanges_WhenNoExceptionsOccur()
    {
        // Arrange
        await ClearDatabase();
        
        var item = ItemEntity.Create(
            new ItemId(Guid.NewGuid()),
            "Transaction Test Item",
            "Test Description",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.Good
        );

        // Act
        await ExecuteInScopeAsync(async serviceProvider =>
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            
            await unitOfWork.BeginTransactionAsync();
            
            await context.Items.AddAsync(item);
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitTransactionAsync();
        });

        // Assert
        var savedItem = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            return await context.Items.FindAsync(item.Id);
        });

        savedItem.Should().NotBeNull();
        savedItem!.Id.Should().Be(item.Id);
    }

    [Fact]
    public async Task BeginTransaction_RollbackTransaction_ShouldNotPersistChanges_WhenRollbackCalled()
    {
        // Arrange
        await ClearDatabase();
        
        var item = ItemEntity.Create(
            new ItemId(Guid.NewGuid()),
            "Rollback Test Item",
            "Test Description",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.Good
        );

        // Act
        await ExecuteInScopeAsync(async serviceProvider =>
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            
            await unitOfWork.BeginTransactionAsync();
            
            await context.Items.AddAsync(item);
            await unitOfWork.SaveChangesAsync();
            
            // Rollback instead of commit
            await unitOfWork.RollbackTransactionAsync();
        });

        // Assert
        var savedItem = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            return await context.Items.FindAsync(item.Id);
        });

        savedItem.Should().BeNull();
    }

    [Fact]
    public async Task BeginTransaction_ShouldRollbackAutomatically_WhenExceptionOccurs()
    {
        // Arrange
        await ClearDatabase();
        
        var item = ItemEntity.Create(
            new ItemId(Guid.NewGuid()),
            "Exception Test Item",
            "Test Description",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.Good
        );

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await ExecuteInScopeAsync(async serviceProvider =>
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
                
                await unitOfWork.BeginTransactionAsync();
                
                await context.Items.AddAsync(item);
                await unitOfWork.SaveChangesAsync();
                
                // Simulate an exception before commit
                throw new InvalidOperationException("Simulated exception");
            });
        });

        // Assert that changes were not persisted due to exception
        var savedItem = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            return await context.Items.FindAsync(item.Id);
        });

        savedItem.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldPersistChanges_WithoutTransaction()
    {
        // Arrange
        await ClearDatabase();
        
        var item = ItemEntity.Create(
            new ItemId(Guid.NewGuid()),
            "Save Test Item",
            "Test Description",
            new CategoryId(Guid.NewGuid()),
            new UserId(Guid.NewGuid()),
            ItemCondition.Good
        );

        // Act
        await ExecuteInScopeAsync(async serviceProvider =>
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            
            await context.Items.AddAsync(item);
            await unitOfWork.SaveChangesAsync();
        });

        // Assert
        var savedItem = await ExecuteInScopeAsync(async serviceProvider =>
        {
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            return await context.Items.FindAsync(item.Id);
        });

        savedItem.Should().NotBeNull();
        savedItem!.Id.Should().Be(item.Id);
    }

    [Fact]
    public async Task BeginTransactionAsync_ShouldThrowException_WhenTransactionAlreadyStarted()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await ExecuteInScopeAsync(async serviceProvider =>
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                
                await unitOfWork.BeginTransactionAsync();
                
                // Try to start another transaction - should throw
                await unitOfWork.BeginTransactionAsync();
            });
        });
    }

    [Fact]
    public async Task CommitTransactionAsync_ShouldThrowException_WhenNoTransactionStarted()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await ExecuteInScopeAsync(async serviceProvider =>
            {
                var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
                
                // Try to commit without starting a transaction
                await unitOfWork.CommitTransactionAsync();
            });
        });
    }

    [Fact]
    public async Task MultipleOperations_ShouldAllBeCommitted_InSingleTransaction()
    {
        // Arrange
        await ClearDatabase();
        
        var items = new[]
        {
            ItemEntity.Create(new ItemId(Guid.NewGuid()), "Multi Item 1", "Description 1", new CategoryId(Guid.NewGuid()), new UserId(Guid.NewGuid()), ItemCondition.Good),
            ItemEntity.Create(new ItemId(Guid.NewGuid()), "Multi Item 2", "Description 2", new CategoryId(Guid.NewGuid()), new UserId(Guid.NewGuid()), ItemCondition.LikeNew),
            ItemEntity.Create(new ItemId(Guid.NewGuid()), "Multi Item 3", "Description 3", new CategoryId(Guid.NewGuid()), new UserId(Guid.NewGuid()), ItemCondition.New)
        };

        // Act
        await ExecuteInScopeAsync(async serviceProvider =>
        {
            var unitOfWork = serviceProvider.GetRequiredService<IUnitOfWork>();
            var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
            
            await unitOfWork.BeginTransactionAsync();
            
            // Add multiple items in same transaction
            foreach (var item in items)
            {
                await context.Items.AddAsync(item);
            }
            
            await unitOfWork.SaveChangesAsync();
            await unitOfWork.CommitTransactionAsync();
        });

        // Assert
        foreach (var item in items)
        {
            var savedItem = await ExecuteInScopeAsync(async serviceProvider =>
            {
                var context = serviceProvider.GetRequiredService<MarketplaceDbContext>();
                return await context.Items.FindAsync(item.Id);
            });

            savedItem.Should().NotBeNull();
            savedItem!.Id.Should().Be(item.Id);
        }
    }
}