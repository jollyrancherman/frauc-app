using Microsoft.Extensions.Logging;
using Marketplace.Application.Common;
using Marketplace.Application.Listings.DTOs;
using Marketplace.Domain.Listings;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;
using Marketplace.Domain.Common;

namespace Marketplace.Application.Listings.Services;

public class ListingDomainService : IListingDomainService
{
    private readonly IListingRepository _listingRepository;
    private readonly IItemRepository _itemRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ListingDomainService> _logger;

    public ListingDomainService(
        IListingRepository listingRepository,
        IItemRepository itemRepository,
        IUnitOfWork unitOfWork,
        ILogger<ListingDomainService> logger)
    {
        _listingRepository = listingRepository;
        _itemRepository = itemRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ListingDto>> CreateFreeListingAsync(
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId,
        CancellationToken cancellationToken = default)
    {
        return await CreateListingInternalAsync(
            () => Listing.CreateFreeListing(
                ListingId.New(),
                itemId,
                sellerId,
                title,
                description,
                location,
                categoryId),
            itemId,
            sellerId,
            "free",
            cancellationToken);
    }

    public async Task<Result<ListingDto>> CreateFreeToAuctionListingAsync(
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        return await CreateListingInternalAsync(
            () => Listing.CreateFreeToAuctionListing(
                ListingId.New(),
                itemId,
                sellerId,
                title,
                description,
                location,
                categoryId,
                duration),
            itemId,
            sellerId,
            "free-to-auction",
            cancellationToken);
    }

    public async Task<Result<ListingDto>> CreateForwardAuctionListingAsync(
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId,
        Money startingPrice,
        Money? reservePrice,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        return await CreateListingInternalAsync(
            () => Listing.CreateForwardAuctionListing(
                ListingId.New(),
                itemId,
                sellerId,
                title,
                description,
                location,
                categoryId,
                startingPrice,
                reservePrice,
                duration),
            itemId,
            sellerId,
            "forward-auction",
            cancellationToken);
    }

    public async Task<Result<ListingDto>> CreateReverseAuctionListingAsync(
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId,
        Money maxPrice,
        TimeSpan duration,
        CancellationToken cancellationToken = default)
    {
        return await CreateListingInternalAsync(
            () => Listing.CreateReverseAuctionListing(
                ListingId.New(),
                itemId,
                sellerId,
                title,
                description,
                location,
                categoryId,
                maxPrice,
                duration),
            itemId,
            sellerId,
            "reverse-auction",
            cancellationToken);
    }

    public async Task<Result<ListingDto>> CreateFixedPriceListingAsync(
        ItemId itemId,
        UserId sellerId,
        string title,
        string description,
        Location location,
        CategoryId categoryId,
        Money price,
        CancellationToken cancellationToken = default)
    {
        return await CreateListingInternalAsync(
            () => Listing.CreateFixedPriceListing(
                ListingId.New(),
                itemId,
                sellerId,
                title,
                description,
                location,
                categoryId,
                price),
            itemId,
            sellerId,
            "fixed-price",
            cancellationToken);
    }

    private async Task<Result<ListingDto>> CreateListingInternalAsync(
        Func<Listing> listingFactory,
        ItemId itemId,
        UserId sellerId,
        string listingType,
        CancellationToken cancellationToken)
    {
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // 1. Validate item exists and user owns it
            var validationResult = await ValidateItemOwnershipAsync(itemId, sellerId, cancellationToken);
            if (validationResult.IsFailure)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<ListingDto>(validationResult.ErrorMessage!);
            }

            // 2. Check for existing listing for this item
            var existingListing = await _listingRepository.GetByItemIdAsync(itemId, cancellationToken);
            if (existingListing != null && !existingListing.IsDeleted)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure<ListingDto>("Item already has an active listing");
            }

            // 3. Create listing using factory
            var listing = listingFactory();

            // 4. Persist listing
            await _listingRepository.AddAsync(listing, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            
            // 5. Commit transaction
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            _logger.LogInformation("Successfully created {ListingType} listing {ListingId} for item {ItemId} by seller {SellerId}", 
                listingType, listing.Id, itemId, sellerId);

            return Result.Success(ListingDto.FromDomain(listing));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            _logger.LogError(ex, "Error creating {ListingType} listing for item {ItemId}", listingType, itemId);
            return Result.Failure<ListingDto>("Failed to create listing");
        }
    }

    private async Task<Result> ValidateItemOwnershipAsync(
        ItemId itemId, 
        UserId sellerId, 
        CancellationToken cancellationToken)
    {
        var item = await _itemRepository.GetByIdAsync(itemId, cancellationToken);
        if (item == null)
        {
            return Result.Failure("Item not found");
        }

        if (item.SellerId != sellerId)
        {
            return Result.Failure("User is not authorized to list this item");
        }

        return Result.Success();
    }
}