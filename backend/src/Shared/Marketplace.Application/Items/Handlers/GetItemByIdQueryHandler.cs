using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Items.Queries;
using Marketplace.Application.Items.DTOs;
using Marketplace.Application.Items.Exceptions;
using Marketplace.Application.Common;
using Marketplace.Domain.Items;

namespace Marketplace.Application.Items.Handlers;

public class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, Result<ItemDto>>
{
    private readonly IItemRepository _itemRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetItemByIdQueryHandler> _logger;

    public GetItemByIdQueryHandler(
        IItemRepository itemRepository,
        IMemoryCache cache,
        ILogger<GetItemByIdQueryHandler> logger)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ItemDto>> Handle(GetItemByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = $"item:{request.ItemId}";

            // Try to get from cache first
            if (_cache.TryGetValue(cacheKey, out ItemDto? cachedItem) && cachedItem != null)
            {
                _logger.LogDebug("Retrieved item {ItemId} from cache", request.ItemId);
                return Result.Success(cachedItem);
            }

            // Get from repository
            var item = await _itemRepository.GetByIdAsync(request.ItemId, cancellationToken);
            
            if (item == null)
            {
                _logger.LogWarning("Item {ItemId} not found", request.ItemId);
                return Result.Failure<ItemDto>($"Item with ID '{request.ItemId}' was not found");
            }

            var itemDto = ItemDto.FromDomain(item);

            // Cache for 5 minutes
            _cache.Set(cacheKey, itemDto, TimeSpan.FromMinutes(5));

            _logger.LogDebug("Retrieved item {ItemId} from repository", request.ItemId);
            
            return Result.Success(itemDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving item {ItemId}", request.ItemId);
            return Result.Failure<ItemDto>($"Error retrieving item: {ex.Message}");
        }
    }
}