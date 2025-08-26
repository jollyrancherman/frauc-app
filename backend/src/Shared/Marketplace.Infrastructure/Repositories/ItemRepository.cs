using Microsoft.EntityFrameworkCore;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Infrastructure.Data;

namespace Marketplace.Infrastructure.Repositories;

public class ItemRepository : IItemRepository
{
    private readonly MarketplaceDbContext _context;

    public ItemRepository(MarketplaceDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Item?> GetByIdAsync(ItemId id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(id, includeImages: false, cancellationToken);
    }

    public async Task<Item?> GetByIdAsync(ItemId id, bool includeImages, CancellationToken cancellationToken = default)
    {
        var query = _context.Items.AsQueryable();
        
        if (includeImages)
        {
            query = query.Include(i => i.Images);
        }
        
        return await query.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Item>> GetBySellerIdAsync(UserId sellerId, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .Include(i => i.Images)
            .Where(i => i.SellerId == sellerId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Item>> GetByCategoryIdAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .Include(i => i.Images)
            .Where(i => i.CategoryId == categoryId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Item item, CancellationToken cancellationToken = default)
    {
        await _context.Items.AddAsync(item, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Item> items, CancellationToken cancellationToken = default)
    {
        await _context.Items.AddRangeAsync(items, cancellationToken);
    }

    public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
    {
        _context.Items.Update(item);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(ItemId id, CancellationToken cancellationToken = default)
    {
        var item = await GetByIdAsync(id, cancellationToken);
        if (item != null)
        {
            _context.Items.Remove(item);
        }
    }

    public async Task<bool> ExistsAsync(ItemId id, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .AnyAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<int> CountBySellerAsync(UserId sellerId, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .CountAsync(i => i.SellerId == sellerId, cancellationToken);
    }

    public async Task<int> CountByCategoryAsync(CategoryId categoryId, CancellationToken cancellationToken = default)
    {
        return await _context.Items
            .CountAsync(i => i.CategoryId == categoryId, cancellationToken);
    }

    public async Task<(IEnumerable<Item> Items, int TotalCount)> GetPagedBySellerAsync(
        UserId sellerId, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Items.Where(i => i.SellerId == sellerId);
        
        // Execute count and data queries in parallel
        var countTask = baseQuery.CountAsync(cancellationToken);
        
        var itemsTask = baseQuery
            .OrderByDescending(i => i.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(i => i.Images) // Only include images for the paginated results
            .ToListAsync(cancellationToken);
        
        await Task.WhenAll(countTask, itemsTask);
        
        return (itemsTask.Result, countTask.Result);
    }

    public async Task<(IEnumerable<Item> Items, int TotalCount)> GetPagedByCategoryAsync(
        CategoryId categoryId, 
        int pageNumber, 
        int pageSize, 
        CancellationToken cancellationToken = default)
    {
        var baseQuery = _context.Items.Where(i => i.CategoryId == categoryId);
        
        // Execute count and data queries in parallel
        var countTask = baseQuery.CountAsync(cancellationToken);
        
        var itemsTask = baseQuery
            .OrderByDescending(i => i.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(i => i.Images) // Only include images for the paginated results
            .ToListAsync(cancellationToken);
        
        await Task.WhenAll(countTask, itemsTask);
        
        return (itemsTask.Result, countTask.Result);
    }
}