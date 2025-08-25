using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Item.API.Controllers;
using Item.API.IntegrationTests.Infrastructure;
using Marketplace.Application.Items.DTOs;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using ItemEntity = Marketplace.Domain.Items.Item;

namespace Item.API.IntegrationTests.Controllers;

public class ItemsControllerTests : IntegrationTestBase
{
    private HttpClient _client = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        _client = CreateClient();
    }

    [Fact]
    public async Task CreateItem_ShouldReturnCreated_WhenRequestIsValid()
    {
        // Arrange
        await ClearDatabase();
        
        var request = new CreateItemRequest(
            Title: "iPhone 14 Pro",
            Description: "Excellent condition smartphone",
            CategoryId: Guid.NewGuid(),
            SellerId: Guid.NewGuid(),
            Condition: ItemCondition.LikeNew
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var createdItem = await response.Content.ReadFromJsonAsync<ItemDto>();
        createdItem.Should().NotBeNull();
        createdItem!.Title.Should().Be(request.Title);
        createdItem.Description.Should().Be(request.Description);
        createdItem.CategoryId.Value.Should().Be(request.CategoryId);
        createdItem.SellerId.Value.Should().Be(request.SellerId);
        createdItem.Condition.Should().Be(request.Condition);
        
        // Verify Location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain($"/api/Items/{createdItem.ItemId.Value}");
    }

    [Fact]
    public async Task CreateItem_ShouldReturnBadRequest_WhenTitleIsEmpty()
    {
        // Arrange
        var request = new CreateItemRequest(
            Title: "",
            Description: "Valid description",
            CategoryId: Guid.NewGuid(),
            SellerId: Guid.NewGuid(),
            Condition: ItemCondition.Good
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateItem_ShouldReturnBadRequest_WhenTitleIsTooLong()
    {
        // Arrange
        var request = new CreateItemRequest(
            Title: new string('a', 201), // Exceeds max length of 200
            Description: "Valid description",
            CategoryId: Guid.NewGuid(),
            SellerId: Guid.NewGuid(),
            Condition: ItemCondition.Good
        );

        // Act
        var response = await _client.PostAsJsonAsync("/api/items", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetItem_ShouldReturnOk_WhenItemExists()
    {
        // Arrange
        await ClearDatabase();
        
        var item = await SeedItemAsync();

        // Act
        var response = await _client.GetAsync($"/api/items/{item.Id.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var returnedItem = await response.Content.ReadFromJsonAsync<ItemDto>();
        returnedItem.Should().NotBeNull();
        returnedItem!.ItemId.Should().Be(item.Id);
        returnedItem.Title.Should().Be(item.Title);
        returnedItem.Description.Should().Be(item.Description);
    }

    [Fact]
    public async Task GetItem_ShouldReturnNotFound_WhenItemDoesNotExist()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/items/{nonExistentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetItemsBySeller_ShouldReturnPaginatedItems_WhenSellerHasItems()
    {
        // Arrange
        await ClearDatabase();
        
        var sellerId = new UserId(Guid.NewGuid());
        var items = new[]
        {
            await SeedItemAsync(sellerId: sellerId, title: "Item 1"),
            await SeedItemAsync(sellerId: sellerId, title: "Item 2"),
            await SeedItemAsync(sellerId: sellerId, title: "Item 3")
        };

        // Act
        var response = await _client.GetAsync($"/api/items/seller/{sellerId.Value}?pageNumber=1&pageSize=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<ItemDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
        result.HasNextPage.Should().BeTrue();
        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task GetItemsBySeller_ShouldReturnEmptyList_WhenSellerHasNoItems()
    {
        // Arrange
        await ClearDatabase();
        
        var sellerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/items/seller/{sellerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<ItemDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task GetItemsByCategory_ShouldReturnPaginatedItems_WhenCategoryHasItems()
    {
        // Arrange
        await ClearDatabase();
        
        var categoryId = new CategoryId(Guid.NewGuid());
        var items = new[]
        {
            await SeedItemAsync(categoryId: categoryId, title: "Category Item 1"),
            await SeedItemAsync(categoryId: categoryId, title: "Category Item 2")
        };

        // Act
        var response = await _client.GetAsync($"/api/items/category/{categoryId.Value}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var result = await response.Content.ReadFromJsonAsync<PaginatedResponse<ItemDto>>();
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Items.Should().AllSatisfy(item => item.CategoryId.Should().Be(categoryId));
    }

    [Fact]
    public async Task GetItemsBySeller_ShouldReturnBadRequest_WhenPageNumberIsInvalid()
    {
        // Arrange
        var sellerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/items/seller/{sellerId}?pageNumber=0");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetItemsBySeller_ShouldReturnBadRequest_WhenPageSizeIsInvalid()
    {
        // Arrange
        var sellerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/items/seller/{sellerId}?pageSize=101");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private async Task<ItemEntity> SeedItemAsync(
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

/// <summary>
/// Response model for paginated data (matches the one in queries)
/// </summary>
public record PaginatedResponse<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize
)
{
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}