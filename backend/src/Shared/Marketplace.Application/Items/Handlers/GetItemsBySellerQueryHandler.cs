using MediatR;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Items.Queries;
using Marketplace.Application.Items.DTOs;
using Marketplace.Application.Common;
using Marketplace.Domain.Items;

namespace Marketplace.Application.Items.Handlers;

public class GetItemsBySellerQueryHandler : IRequestHandler<GetItemsBySellerQuery, Result<PaginatedResponse<ItemDto>>>
{
    private readonly IItemRepository _itemRepository;
    private readonly ILogger<GetItemsBySellerQueryHandler> _logger;

    public GetItemsBySellerQueryHandler(
        IItemRepository itemRepository,
        ILogger<GetItemsBySellerQueryHandler> logger)
    {
        _itemRepository = itemRepository ?? throw new ArgumentNullException(nameof(itemRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<PaginatedResponse<ItemDto>>> Handle(
        GetItemsBySellerQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Retrieving items for seller {SellerId}, page {PageNumber}, size {PageSize}",
                request.SellerId, request.PageNumber, request.PageSize);

            // Validate pagination parameters
            if (request.PageNumber < 1)
            {
                return Result.Failure<PaginatedResponse<ItemDto>>("Page number must be greater than 0");
            }

            if (request.PageSize < 1 || request.PageSize > 100)
            {
                return Result.Failure<PaginatedResponse<ItemDto>>("Page size must be between 1 and 100");
            }

            var (items, totalCount) = await _itemRepository.GetPagedBySellerAsync(
                request.SellerId,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            var itemDtos = items.Select(ItemDto.FromDomain);

            var response = new PaginatedResponse<ItemDto>(
                itemDtos,
                totalCount,
                request.PageNumber,
                request.PageSize);

            _logger.LogDebug("Retrieved {ItemCount} items for seller {SellerId} (total: {TotalCount})",
                items.Count(), request.SellerId, totalCount);

            return Result.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving items for seller {SellerId}", request.SellerId);
            return Result.Failure<PaginatedResponse<ItemDto>>($"Error retrieving items: {ex.Message}");
        }
    }
}