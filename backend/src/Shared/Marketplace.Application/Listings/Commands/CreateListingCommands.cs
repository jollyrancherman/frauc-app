using MediatR;
using Marketplace.Application.Common;
using Marketplace.Application.Listings.DTOs;
using Marketplace.Domain.Listings.ValueObjects;
using Marketplace.Domain.Items.ValueObjects;
using Marketplace.Domain.Categories;

namespace Marketplace.Application.Listings.Commands;

public record CreateFreeListingCommand(
    ItemId ItemId,
    UserId SellerId,
    string Title,
    string Description,
    Location Location,
    CategoryId CategoryId
) : IRequest<Result<ListingDto>>;

public record CreateFreeToAuctionListingCommand(
    ItemId ItemId,
    UserId SellerId,
    string Title,
    string Description,
    Location Location,
    CategoryId CategoryId,
    TimeSpan Duration
) : IRequest<Result<ListingDto>>;

public record CreateForwardAuctionListingCommand(
    ItemId ItemId,
    UserId SellerId,
    string Title,
    string Description,
    Location Location,
    CategoryId CategoryId,
    Money StartingPrice,
    Money? ReservePrice,
    Money? BuyNowPrice,
    TimeSpan Duration
) : IRequest<Result<ListingDto>>;

public record CreateReverseAuctionListingCommand(
    ItemId ItemId,
    UserId SellerId,
    string Title,
    string Description,
    Location Location,
    CategoryId CategoryId,
    Money MaxPrice,
    TimeSpan Duration
) : IRequest<Result<ListingDto>>;

public record CreateFixedPriceListingCommand(
    ItemId ItemId,
    UserId SellerId,
    string Title,
    string Description,
    Location Location,
    CategoryId CategoryId,
    Money Price
) : IRequest<Result<ListingDto>>;

public record UpdateListingCommand(
    ListingId ListingId,
    string? Title,
    string? Description,
    Location? Location
) : IRequest<Result<ListingDto>>;

public record DeleteListingCommand(
    ListingId ListingId,
    UserId RequestingUserId
) : IRequest<Result<bool>>;