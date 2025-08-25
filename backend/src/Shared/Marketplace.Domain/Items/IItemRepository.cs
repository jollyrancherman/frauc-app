using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Domain.Items;

public interface IItemRepository
{
    // Basic CRUD operations
    Task<Item?> GetByIdAsync(ItemId id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetBySellerIdAsync(UserId sellerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Item>> GetByCategoryIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default);
    Task AddAsync(Item item, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Item> items, CancellationToken cancellationToken = default);
    Task UpdateAsync(Item item, CancellationToken cancellationToken = default);
    Task DeleteAsync(ItemId id, CancellationToken cancellationToken = default);

    // Performance optimized methods
    Task<bool> ExistsAsync(ItemId id, CancellationToken cancellationToken = default);
    Task<int> CountBySellerAsync(UserId sellerId, CancellationToken cancellationToken = default);
    Task<int> CountByCategoryAsync(CategoryId categoryId, CancellationToken cancellationToken = default);

    // Pagination support
    Task<(IEnumerable<Item> Items, int TotalCount)> GetPagedBySellerAsync(
        UserId sellerId, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);
    
    Task<(IEnumerable<Item> Items, int TotalCount)> GetPagedByCategoryAsync(
        CategoryId categoryId, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default);
}