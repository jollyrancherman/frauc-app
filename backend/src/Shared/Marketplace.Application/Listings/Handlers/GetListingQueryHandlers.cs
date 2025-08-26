using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Common;
using Marketplace.Application.Listings.Queries;
using Marketplace.Application.Listings.DTOs;
using Marketplace.Domain.Listings;

namespace Marketplace.Application.Listings.Handlers;

public class GetListingByIdQueryHandler : IRequestHandler<GetListingByIdQuery, Result<ListingDto>>
{
    private readonly IListingRepository _listingRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<GetListingByIdQueryHandler> _logger;

    public GetListingByIdQueryHandler(
        IListingRepository listingRepository,
        IMemoryCache cache,
        ILogger<GetListingByIdQueryHandler> logger)
    {
        _listingRepository = listingRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Result<ListingDto>> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var cacheKey = $"listing_{request.ListingId}";

            if (_cache.TryGetValue(cacheKey, out ListingDto? cachedListing) && cachedListing != null)
            {
                _logger.LogDebug("Retrieved listing {ListingId} from cache", request.ListingId);
                return Result.Success(cachedListing);
            }

            var listing = await _listingRepository.GetByIdAsync(request.ListingId, cancellationToken);
            if (listing == null || listing.IsDeleted)
            {
                return Result.Failure<ListingDto>("Listing not found");
            }

            var listingDto = ListingDto.FromDomain(listing);
            
            _cache.Set(cacheKey, listingDto, TimeSpan.FromMinutes(15));
            _logger.LogDebug("Cached listing {ListingId} for 15 minutes", request.ListingId);

            return Result.Success(listingDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving listing {ListingId}", request.ListingId);
            return Result.Failure<ListingDto>("Failed to retrieve listing");
        }
    }
}

public class GetListingByItemIdQueryHandler : IRequestHandler<GetListingByItemIdQuery, Result<ListingDto>>
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<GetListingByItemIdQueryHandler> _logger;

    public GetListingByItemIdQueryHandler(
        IListingRepository listingRepository,
        ILogger<GetListingByItemIdQueryHandler> logger)
    {
        _listingRepository = listingRepository;
        _logger = logger;
    }

    public async Task<Result<ListingDto>> Handle(GetListingByItemIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var listing = await _listingRepository.GetByItemIdAsync(request.ItemId, cancellationToken);
            if (listing == null || listing.IsDeleted)
            {
                return Result.Failure<ListingDto>("Listing not found for this item");
            }

            return Result.Success(ListingDto.FromDomain(listing));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving listing for item {ItemId}", request.ItemId);
            return Result.Failure<ListingDto>("Failed to retrieve listing");
        }
    }
}

public class GetListingsBySellerQueryHandler : IRequestHandler<GetListingsBySellerQuery, Result<PagedResult<ListingDto>>>
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<GetListingsBySellerQueryHandler> _logger;

    public GetListingsBySellerQueryHandler(
        IListingRepository listingRepository,
        ILogger<GetListingsBySellerQueryHandler> logger)
    {
        _listingRepository = listingRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ListingDto>>> Handle(GetListingsBySellerQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (listings, totalCount) = await _listingRepository.GetListingsBySellerAsync(
                request.SellerId,
                request.PageNumber,
                request.PageSize,
                request.Status,
                request.Type,
                cancellationToken);

            var listingDtos = listings.Select(ListingDto.FromDomain);

            var result = new PagedResult<ListingDto>
            {
                Items = listingDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving listings for seller {SellerId}", request.SellerId);
            return Result.Failure<PagedResult<ListingDto>>("Failed to retrieve seller listings");
        }
    }
}

public class GetListingsByCategoryQueryHandler : IRequestHandler<GetListingsByCategoryQuery, Result<PagedResult<ListingDto>>>
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<GetListingsByCategoryQueryHandler> _logger;

    public GetListingsByCategoryQueryHandler(
        IListingRepository listingRepository,
        ILogger<GetListingsByCategoryQueryHandler> logger)
    {
        _listingRepository = listingRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ListingDto>>> Handle(GetListingsByCategoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (listings, totalCount) = await _listingRepository.GetListingsByCategoryAsync(
                request.CategoryId,
                request.PageNumber,
                request.PageSize,
                request.Status,
                request.Type,
                cancellationToken);

            var listingDtos = listings.Select(ListingDto.FromDomain);

            var result = new PagedResult<ListingDto>
            {
                Items = listingDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving listings for category {CategoryId}", request.CategoryId);
            return Result.Failure<PagedResult<ListingDto>>("Failed to retrieve category listings");
        }
    }
}

public class GetNearbyListingsQueryHandler : IRequestHandler<GetNearbyListingsQuery, Result<PagedResult<ListingDto>>>
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<GetNearbyListingsQueryHandler> _logger;

    public GetNearbyListingsQueryHandler(
        IListingRepository listingRepository,
        ILogger<GetNearbyListingsQueryHandler> logger)
    {
        _listingRepository = listingRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ListingDto>>> Handle(GetNearbyListingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var listings = await _listingRepository.GetNearbyListingsAsync(
                request.Center,
                request.RadiusKm,
                cancellationToken);

            // Apply pagination and filtering in memory for now (could be optimized at DB level)
            var filteredListings = listings.AsEnumerable();

            if (request.Type.HasValue)
            {
                filteredListings = filteredListings.Where(l => l.ListingType == request.Type.Value);
            }

            var paginatedListings = filteredListings
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(ListingDto.FromDomain)
                .ToList();

            var totalCount = filteredListings.Count();

            var result = new PagedResult<ListingDto>
            {
                Items = paginatedListings,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving nearby listings for location {Location} within {Radius}km", 
                request.Center, request.RadiusKm);
            return Result.Failure<PagedResult<ListingDto>>("Failed to retrieve nearby listings");
        }
    }
}

public class SearchListingsQueryHandler : IRequestHandler<SearchListingsQuery, Result<PagedResult<ListingDto>>>
{
    private readonly IListingRepository _listingRepository;
    private readonly ILogger<SearchListingsQueryHandler> _logger;

    public SearchListingsQueryHandler(
        IListingRepository listingRepository,
        ILogger<SearchListingsQueryHandler> logger)
    {
        _listingRepository = listingRepository;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ListingDto>>> Handle(SearchListingsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var (listings, totalCount) = await _listingRepository.SearchListingsAsync(
                request.SearchTerm,
                request.CategoryId,
                request.Center,
                request.RadiusKm,
                request.MinPrice,
                request.MaxPrice,
                request.Type,
                request.Status,
                request.PageNumber,
                request.PageSize,
                request.SortBy,
                request.SortDirection,
                cancellationToken);

            var listingDtos = listings.Select(ListingDto.FromDomain);

            var result = new PagedResult<ListingDto>
            {
                Items = listingDtos,
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching listings with term '{SearchTerm}'", request.SearchTerm);
            return Result.Failure<PagedResult<ListingDto>>("Failed to search listings");
        }
    }
}