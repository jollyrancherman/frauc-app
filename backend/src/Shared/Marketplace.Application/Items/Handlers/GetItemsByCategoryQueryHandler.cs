using MediatR;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Items.Queries;
using Marketplace.Application.Items.DTOs;
using Marketplace.Application.Common;
using Marketplace.Domain.Items;

namespace Marketplace.Application.Items.Handlers;

public class GetItemsByCategoryQueryHandler : IRequestHandler<GetItemsByCategoryQuery, Result<PaginatedResponse<ItemDto>>>
{
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<GetItemsByCategoryQueryHandler> _logger;

    public GetItemsByCategoryQueryHandler(
        IItemRepository itemRepository,
        ILogger<GetItemsByCategoryQueryHandler> logger)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<PaginatedResponse<ItemDto>>> Handle(
        GetItemsByCategoryQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Retrieving items for category {CategoryId}, page {PageNumber}, size {PageSize}",
                request.CategoryId, request.PageNumber, request.PageSize);

            // Validate pagination parameters
            if (request.PageNumber < 1)
            {
                return Result.Failure<PaginatedResponse<ItemDto>>("Page number must be greater than 0");
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                return Result.Failure<PaginatedResponse<ItemDto>>("Page size must be between 1 and 100");
            }

            var (items, totalCount) = await _itemRepository.GetPagedByCategoryAsync(
                request.CategoryId,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var itemDtos = items.Select(ItemDto.FromDomain);

            var response = new PaginatedResponse<ItemDto>(
                itemDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogDebug("Retrieved {ItemCount} items for category {CategoryId} (total: {TotalCount})",
                items.Count(), request.CategoryId, totalCount);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items for category {CategoryId}", request.CategoryId);
            return Result.Failure<PaginatedResponse<ItemDto>>($"Error retrieving items: {ex.Message}");
        }
    }
}