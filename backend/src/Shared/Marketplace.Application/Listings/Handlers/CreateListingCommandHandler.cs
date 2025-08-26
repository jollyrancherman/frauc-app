using MediatR;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Common;
using Marketplace.Application.Listings.Commands;
using Marketplace.Application.Listings.DTOs;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Items;
using Marketplace.Domain.Common;

namespace Marketplace.Application.Listings.Handlers;

public class CreateListingCommandHandler :
    IRequestHandler<CreateFreeListingCommand, Result<ListingDto>>,
    IRequestHandler<CreateFreeToAuctionListingCommand, Result<ListingDto>>,
    IRequestHandler<CreateForwardAuctionListingCommand, Result<ListingDto>>,
    IRequestHandler<CreateReverseAuctionListingCommand, Result<ListingDto>>,
    IRequestHandler<CreateFixedPriceListingCommand, Result<ListingDto>>
{
    private readonly IListingRepository _listingRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateListingCommandHandler> _logger;

    public CreateListingCommandHandler(
        IListingRepository listingRepository,
        IItemRepository itemRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateListingCommandHandler> logger)
    {
        _listingRepository = listingRepository;
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ListingDto>> Handle(CreateFreeListingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _itemRepository.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                return Result.Failure<ListingDto>("Item not found");
            }

            if (item.SellerId != request.SellerId)
            {
                return Result.Failure<ListingDto>("User is not authorized to list this item");
            }

            var listing = Domain.Listings.Listing.CreateFreeListing(
                Domain.Listings.ValueObjects.ListingId.New(),
                request.ItemId,
                request.SellerId,
                request.Title,
                request.Description,
                request.Location,
                request.CategoryId);

            await _listingRepository.AddAsync(listing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Free listing created successfully for item {ItemId} by seller {SellerId}", 
                request.ItemId, request.SellerId);

            return Result.Success(ListingDto.FromDomain(listing));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating free listing for item {ItemId}", request.ItemId);
            return Result.Failure<ListingDto>("Failed to create listing");
        }
    }

    public async Task<Result<ListingDto>> Handle(CreateFreeToAuctionListingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _itemRepository.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                return Result.Failure<ListingDto>("Item not found");
            }

            if (item.SellerId != request.SellerId)
            {
                return Result.Failure<ListingDto>("User is not authorized to list this item");
            }

            var listing = Domain.Listings.Listing.CreateFreeToAuctionListing(
                Domain.Listings.ValueObjects.ListingId.New(),
                request.ItemId,
                request.SellerId,
                request.Title,
                request.Description,
                request.Location,
                request.CategoryId,
                request.Duration);

            await _listingRepository.AddAsync(listing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Free-to-auction listing created successfully for item {ItemId} by seller {SellerId}", 
                request.ItemId, request.SellerId);

            return Result.Success(ListingDto.FromDomain(listing));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating free-to-auction listing for item {ItemId}", request.ItemId);
            return Result.Failure<ListingDto>("Failed to create listing");
        }
    }

    public async Task<Result<ListingDto>> Handle(CreateForwardAuctionListingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _itemRepository.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                return Result.Failure<ListingDto>("Item not found");
            }

            if (item.SellerId != request.SellerId)
            {
                return Result.Failure<ListingDto>("User is not authorized to list this item");
            }

            var listing = Domain.Listings.Listing.CreateForwardAuctionListing(
                Domain.Listings.ValueObjects.ListingId.New(),
                request.ItemId,
                request.SellerId,
                request.Title,
                request.Description,
                request.Location,
                request.CategoryId,
                request.StartingPrice,
                request.ReservePrice,
                request.Duration);

            await _listingRepository.AddAsync(listing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Forward auction listing created successfully for item {ItemId} by seller {SellerId}", 
                request.ItemId, request.SellerId);

            return Result.Success(ListingDto.FromDomain(listing));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating forward auction listing for item {ItemId}", request.ItemId);
            return Result.Failure<ListingDto>("Failed to create listing");
        }
    }

    public async Task<Result<ListingDto>> Handle(CreateReverseAuctionListingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _itemRepository.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                return Result.Failure<ListingDto>("Item not found");
            }

            if (item.SellerId != request.SellerId)
            {
                return Result.Failure<ListingDto>("User is not authorized to list this item");
            }

            var listing = Domain.Listings.Listing.CreateReverseAuctionListing(
                Domain.Listings.ValueObjects.ListingId.New(),
                request.ItemId,
                request.SellerId,
                request.Title,
                request.Description,
                request.Location,
                request.CategoryId,
                request.MaxPrice,
                request.Duration);

            await _listingRepository.AddAsync(listing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reverse auction listing created successfully for item {ItemId} by seller {SellerId}", 
                request.ItemId, request.SellerId);

            return Result.Success(ListingDto.FromDomain(listing));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reverse auction listing for item {ItemId}", request.ItemId);
            return Result.Failure<ListingDto>("Failed to create listing");
        }
    }

    public async Task<Result<ListingDto>> Handle(CreateFixedPriceListingCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var item = await _itemRepository.GetByIdAsync(request.ItemId, cancellationToken);
            if (item == null)
            {
                return Result.Failure<ListingDto>("Item not found");
            }

            if (item.SellerId != request.SellerId)
            {
                return Result.Failure<ListingDto>("User is not authorized to list this item");
            }

            var listing = Domain.Listings.Listing.CreateFixedPriceListing(
                Domain.Listings.ValueObjects.ListingId.New(),
                request.ItemId,
                request.SellerId,
                request.Title,
                request.Description,
                request.Location,
                request.CategoryId,
                request.Price);

            await _listingRepository.AddAsync(listing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Fixed price listing created successfully for item {ItemId} by seller {SellerId}", 
                request.ItemId, request.SellerId);

            return Result.Success(ListingDto.FromDomain(listing));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating fixed price listing for item {ItemId}", request.ItemId);
            return Result.Failure<ListingDto>("Failed to create listing");
        }
    }
}