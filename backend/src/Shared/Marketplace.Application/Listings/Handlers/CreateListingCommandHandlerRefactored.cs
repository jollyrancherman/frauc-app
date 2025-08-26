using MediatR;
using Microsoft.Extensions.Logging;
using Marketplace.Application.Common;
using Marketplace.Application.Listings.Commands;
using Marketplace.Application.Listings.DTOs;
using Marketplace.Application.Listings.Services;

namespace Marketplace.Application.Listings.Handlers;

public class CreateListingCommandHandlerRefactored :
    IRequestHandler<CreateFreeListingCommand, Result<ListingDto>>,
    IRequestHandler<CreateFreeToAuctionListingCommand, Result<ListingDto>>,
    IRequestHandler<CreateForwardAuctionListingCommand, Result<ListingDto>>,
    IRequestHandler<CreateReverseAuctionListingCommand, Result<ListingDto>>,
    IRequestHandler<CreateFixedPriceListingCommand, Result<ListingDto>>
{
    private readonly IListingDomainService _listingDomainService;
    private readonly ILogger<CreateListingCommandHandlerRefactored> _logger;

    public CreateListingCommandHandlerRefactored(
        IListingDomainService listingDomainService,
        ILogger<CreateListingCommandHandlerRefactored> logger)
    {
        _listingDomainService = listingDomainService;
        _logger = logger;
    }

    public async Task<Result<ListingDto>> Handle(CreateFreeListingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating free listing for item {ItemId} by seller {SellerId}", 
            request.ItemId, request.SellerId);

        return await _listingDomainService.CreateFreeListingAsync(
            request.ItemId,
            request.SellerId,
            request.Title,
            request.Description,
            request.Location,
            request.CategoryId,
            cancellationToken);
    }

    public async Task<Result<ListingDto>> Handle(CreateFreeToAuctionListingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating free-to-auction listing for item {ItemId} by seller {SellerId}", 
            request.ItemId, request.SellerId);

        return await _listingDomainService.CreateFreeToAuctionListingAsync(
            request.ItemId,
            request.SellerId,
            request.Title,
            request.Description,
            request.Location,
            request.CategoryId,
            request.Duration,
            cancellationToken);
    }

    public async Task<Result<ListingDto>> Handle(CreateForwardAuctionListingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating forward auction listing for item {ItemId} by seller {SellerId}", 
            request.ItemId, request.SellerId);

        return await _listingDomainService.CreateForwardAuctionListingAsync(
            request.ItemId,
            request.SellerId,
            request.Title,
            request.Description,
            request.Location,
            request.CategoryId,
            request.StartingPrice,
            request.ReservePrice,
            request.Duration,
            cancellationToken);
    }

    public async Task<Result<ListingDto>> Handle(CreateReverseAuctionListingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating reverse auction listing for item {ItemId} by seller {SellerId}", 
            request.ItemId, request.SellerId);

        return await _listingDomainService.CreateReverseAuctionListingAsync(
            request.ItemId,
            request.SellerId,
            request.Title,
            request.Description,
            request.Location,
            request.CategoryId,
            request.MaxPrice,
            request.Duration,
            cancellationToken);
    }

    public async Task<Result<ListingDto>> Handle(CreateFixedPriceListingCommand request, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating fixed price listing for item {ItemId} by seller {SellerId}", 
            request.ItemId, request.SellerId);

        return await _listingDomainService.CreateFixedPriceListingAsync(
            request.ItemId,
            request.SellerId,
            request.Title,
            request.Description,
            request.Location,
            request.CategoryId,
            request.Price,
            cancellationToken);
    }
}